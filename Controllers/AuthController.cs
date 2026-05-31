using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Models;
using System.Security.Claims;

namespace PhoneStore.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin" });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
            return Json(new { success = false, message = "Email hoặc mật khẩu không đúng" });

        if (!user.IsActive)
            return Json(new { success = false, message = "Tài khoản đã bị khóa. Vui lòng liên hệ Admin để biết thêm chi tiết." });

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result != PasswordVerificationResult.Success)
            return Json(new { success = false, message = "Email hoặc mật khẩu không đúng" });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Json(new { success = true, message = "Đăng nhập thành công", role = user.Role });
    }

    [HttpPost]
    public async Task<IActionResult> Register(string fullName, string email, string phone, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin" });

        if (password != confirmPassword)
            return Json(new { success = false, message = "Mật khẩu xác nhận không khớp" });

        if (password.Length < 6)
            return Json(new { success = false, message = "Mật khẩu phải có ít nhất 6 ký tự" });

        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Json(new { success = false, message = "Email đã được đăng ký" });

        var user = new User
        {
            FullName = fullName,
            Email = email,
            Phone = phone ?? "",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Json(new { success = true, message = "Đăng ký thành công! Vui lòng đăng nhập." });
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
