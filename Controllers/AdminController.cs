using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Models;

namespace PhoneStore.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Dashboard()
    {
        ViewBag.TotalProducts = await _db.Products.CountAsync();
        ViewBag.TotalUsers = await _db.Users.CountAsync();
        ViewBag.TotalRevenue = await _db.Orders.Where(o => o.Status == "Đã giao").SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
        ViewBag.TotalOrders = await _db.Orders.CountAsync();
        ViewBag.PendingOrders = await _db.Orders.CountAsync(o => o.Status == "Chờ xác nhận");
        return View();
    }

    // ===== PRODUCT CRUD =====
    public async Task<IActionResult> Products(int page = 1, string? search = null)
    {
        var pageSize = 20;
        var query = _db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s) || p.Brand.ToLower().Contains(s));
        }

        var total = await query.CountAsync();
        var products = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        return View(products);
    }

    public IActionResult ProductCreate() => View();

    [HttpPost]
    public async Task<IActionResult> ProductCreate(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            ModelState.AddModelError("", "Tên sản phẩm không được để trống");
            return View(product);
        }

        var maxId = await _db.Products.MaxAsync(p => (int?)p.Id) ?? 0;
        product.Id = maxId + 1;
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Thêm sản phẩm thành công!";
        return RedirectToAction("Products");
    }

    public async Task<IActionResult> ProductEdit(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> ProductEdit(Product product)
    {
        var existing = await _db.Products.FindAsync(product.Id);
        if (existing is null) return NotFound();

        existing.Name = product.Name;
        existing.Brand = product.Brand;
        existing.Price = product.Price;
        existing.ImageUrl = product.ImageUrl;
        existing.Description = product.Description;
        existing.IsNew = product.IsNew;
        existing.IsBestSeller = product.IsBestSeller;
        existing.SourceUrl = product.SourceUrl;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật sản phẩm thành công!";
        return RedirectToAction("Products");
    }

    [HttpPost]
    public async Task<IActionResult> ProductDelete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Xóa sản phẩm thành công!";
        return RedirectToAction("Products");
    }

    // ===== USER MANAGEMENT =====
    public async Task<IActionResult> Users(string? search = null)
    {
        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(s)
                                  || u.Email.ToLower().Contains(s)
                                  || u.Phone.Contains(s));
        }

        var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> UserDelete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        if (user.Role == "Admin")
        {
            TempData["Error"] = "Không thể xóa tài khoản Admin!";
            return RedirectToAction("Users");
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Xóa người dùng thành công!";
        return RedirectToAction("Users");
    }

    [HttpPost]
    public async Task<IActionResult> UserLock(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        if (user.Role == "Admin")
        {
            TempData["Error"] = "Không thể khóa tài khoản Admin!";
            return RedirectToAction("Users");
        }

        user.IsActive = false;
        user.LockedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã khóa tài khoản {user.Email}";
        return RedirectToAction("Users");
    }

    [HttpPost]
    public async Task<IActionResult> UserUnlock(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.IsActive = true;
        user.LockedAt = null;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã mở khóa tài khoản {user.Email}";
        return RedirectToAction("Users");
    }

    // ===== ORDER MANAGEMENT =====
    public async Task<IActionResult> Orders(string? status = null)
    {
        var query = _db.Orders.Include(o => o.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.Status == status);

        var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        ViewBag.Statuses = await _db.Orders.Select(o => o.Status).Distinct().ToListAsync();
        ViewBag.SelectedStatus = status;
        return View(orders);
    }

    public async Task<IActionResult> OrderDetail(int id)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();
        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(int id, string status)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order is null) return NotFound();

        order.Status = status;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Cập nhật trạng thái đơn hàng #{id} thành: {status}";
        return RedirectToAction("Orders");
    }
}
