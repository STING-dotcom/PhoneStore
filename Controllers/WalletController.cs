using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Models;
using System.Security.Claims;

namespace PhoneStore.Controllers;

public class WalletController : Controller
{
    private readonly AppDbContext _db;

    public WalletController(AppDbContext db) => _db = db;

    private int? GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return id is not null ? int.Parse(id) : null;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        if (userId is null)
        {
            // Fallback: session-based for non-logged-in users
            var raw = HttpContext.Session.GetString("WalletBalance");
            decimal.TryParse(raw, out var balance);
            ViewBag.Balance = balance;
            ViewBag.Transactions = new List<WalletTransaction>();
            return View();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return RedirectToAction("Logout", "Auth");

        ViewBag.Balance = user.WalletBalance;
        ViewBag.Transactions = await _db.WalletTransactions
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
        return View();
    }

    public IActionResult TopUp()
    {
        var userId = GetUserId();
        if (userId is null)
        {
            ViewBag.Balance = 0m;
            return View();
        }

        var user = _db.Users.Find(userId);
        ViewBag.Balance = user?.WalletBalance ?? 0m;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Deposit(decimal amount, string method)
    {
        if (amount < 20000)
        {
            TempData["Error"] = "Số tiền nạp tối thiểu là 20.000 VND.";
            return RedirectToAction("TopUp");
        }

        var userId = GetUserId();
        if (userId is null)
        {
            // Fallback: session-based
            var balance = 0m;
            var raw = HttpContext.Session.GetString("WalletBalance");
            decimal.TryParse(raw, out balance);

            var txs = new List<WalletTransaction>();
            var json = HttpContext.Session.GetString("WalletTransactions");
            if (json is not null)
                txs = System.Text.Json.JsonSerializer.Deserialize<List<WalletTransaction>>(json) ?? new();

            txs.Add(new WalletTransaction
            {
                Amount = amount,
                Type = "Nạp tiền",
                Method = method,
                CreatedAt = DateTime.Now,
                BalanceBefore = balance,
                BalanceAfter = balance + amount,
                Status = "Hoàn thành"
            });

            HttpContext.Session.SetString("WalletBalance", (balance + amount).ToString("F0"));
            HttpContext.Session.SetString("WalletTransactions", System.Text.Json.JsonSerializer.Serialize(txs));

            TempData["Success"] = $"Nạp thành công {amount:N0}đ vào ví!";
            return RedirectToAction("Index");
        }

        var user = await _db.Users.FindAsync(userId);
        if (user is null) return RedirectToAction("Logout", "Auth");

        var before = user.WalletBalance;
        user.WalletBalance += amount;

        _db.WalletTransactions.Add(new WalletTransaction
        {
            UserId = userId.Value,
            Amount = amount,
            Type = "Nạp tiền",
            Method = method,
            CreatedAt = DateTime.UtcNow,
            BalanceBefore = before,
            BalanceAfter = user.WalletBalance,
            Status = "Hoàn thành"
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Nạp thành công {amount:N0}đ vào ví!";
        return RedirectToAction("Index");
    }

    public static async Task<bool> DeductBalance(AppDbContext db, int userId, decimal amount)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is null || user.WalletBalance < amount)
            return false;

        var before = user.WalletBalance;
        user.WalletBalance -= amount;

        db.WalletTransactions.Add(new WalletTransaction
        {
            UserId = userId,
            Amount = -amount,
            Type = "Thanh toán đơn hàng",
            Method = "Ví PhoneStore",
            CreatedAt = DateTime.UtcNow,
            BalanceBefore = before,
            BalanceAfter = user.WalletBalance,
            Status = "Hoàn thành"
        });

        await db.SaveChangesAsync();
        return true;
    }

    // Legacy session-based fallback for non-logged-in users
    public static bool DeductBalanceLegacy(ISession session, decimal amount)
    {
        var raw = session.GetString("WalletBalance");
        if (!decimal.TryParse(raw, out var balance) || balance < amount)
            return false;

        session.SetString("WalletBalance", (balance - amount).ToString("F0"));
        return true;
    }
}
