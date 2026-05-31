using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Models;
using System.Security.Claims;

namespace PhoneStore.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db) => _db = db;

    private async Task<User?> GetCurrentUserAsync() =>
        await _db.Users.FirstOrDefaultAsync(u => u.Id.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier));

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return RedirectToAction("Logout", "Auth");

        ViewBag.Transactions = await _db.WalletTransactions
            .Where(w => w.UserId == user.Id)
            .OrderByDescending(w => w.CreatedAt)
            .Take(20)
            .ToListAsync();

        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return RedirectToAction("Logout", "Auth");
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string fullName, string phone)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return RedirectToAction("Logout", "Auth");

        user.FullName = fullName ?? user.FullName;
        user.Phone = phone ?? user.Phone;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Refresh claims
        await RefreshSignInAsync(user);

        TempData["Success"] = "Cập nhật thông tin thành công!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UploadAvatar(IFormFile avatar)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Json(new { success = false, message = "Phiên đăng nhập hết hạn" });

        if (avatar is null || avatar.Length == 0)
            return Json(new { success = false, message = "Vui lòng chọn ảnh" });

        var ext = Path.GetExtension(avatar.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        if (!allowed.Contains(ext))
            return Json(new { success = false, message = "Chỉ chấp nhận file ảnh (jpg, png, gif, webp)" });

        if (avatar.Length > 2 * 1024 * 1024)
            return Json(new { success = false, message = "Ảnh không được vượt quá 2MB" });

        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
        Directory.CreateDirectory(dir);

        // Xoá ảnh cũ nếu có
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarUrl.TrimStart('/'));
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        var fileName = $"u{user.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
        var filePath = Path.Combine(dir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await avatar.CopyToAsync(stream);
        }

        user.AvatarUrl = $"/images/avatars/{fileName}";
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Json(new { success = true, url = user.AvatarUrl });
    }

    [HttpGet]
    public IActionResult ChangePassword() => View();

    [HttpPost]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return RedirectToAction("Logout", "Auth");

        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
        {
            ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin");
            return View();
        }

        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError("", "Mật khẩu xác nhận không khớp");
            return View();
        }

        if (newPassword.Length < 6)
        {
            ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự");
            return View();
        }

        var hasher = new PasswordHasher<User>();
        var verify = hasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
        if (verify != PasswordVerificationResult.Success)
        {
            ModelState.AddModelError("", "Mật khẩu hiện tại không đúng");
            return View();
        }

        user.PasswordHash = hasher.HashPassword(user, newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đổi mật khẩu thành công!";
        return RedirectToAction("Index");
    }

    private async Task RefreshSignInAsync(User user)
    {
        var identity = (User.Identity as ClaimsIdentity)!;
        var claim = identity.FindFirst(ClaimTypes.Name);
        if (claim is not null)
            identity.RemoveClaim(claim);

        identity.AddClaim(new Claim(ClaimTypes.Name, user.FullName));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }

    [HttpGet]
    public async Task<IActionResult> Orders()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return RedirectToAction("Logout", "Auth");

        var orders = await _db.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }
}
