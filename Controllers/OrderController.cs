using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Models;
using System.Security.Claims;
using System.Text.Json;

namespace PhoneStore.Controllers;

public class OrderController : Controller
{
    private readonly AppDbContext _db;
    private const string CartSessionKey = "CartItems";

    public OrderController(AppDbContext db) => _db = db;

    private List<CartItem> GetCart()
    {
        var json = HttpContext.Session.GetString(CartSessionKey);
        return json is null ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
    }

    private void ClearCart() => HttpContext.Session.Remove(CartSessionKey);

    private int? GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return id is not null ? int.Parse(id) : null;
    }

    [HttpGet]
    public IActionResult Checkout()
    {
        var cart = GetCart();
        if (cart.Count == 0)
            return RedirectToAction("Index", "Cart");

        ViewBag.Total = cart.Sum(c => c.Subtotal);
        ViewBag.WalletBalance = 0m;
        var userId = GetUserId();
        if (userId is null)
        {
            var raw = HttpContext.Session.GetString("WalletBalance");
            ViewBag.WalletBalance = decimal.TryParse(raw, out var b) ? b : 0m;
        }
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(string customerName, string phone, string email, string address, string note, string paymentMethod)
    {
        var cart = GetCart();
        if (cart.Count == 0)
            return RedirectToAction("Index", "Cart");

        var total = cart.Sum(c => c.Subtotal);
        var methodLabels = new Dictionary<string, string>
        {
            ["COD"] = "Tiền mặt (COD)",
            ["Wallet"] = "Ví PhoneStore",
            ["BankTransfer"] = "Chuyển khoản ngân hàng",
            ["ScratchCard"] = "Thẻ cào điện thoại",
            ["MoMo"] = "Ví MoMo",
            ["VNPAY"] = "VNPAY"
        };

        paymentMethod ??= "COD";
        if (!methodLabels.ContainsKey(paymentMethod))
            paymentMethod = "COD";

        if (paymentMethod == "Wallet")
        {
            var userId = GetUserId();
            if (userId is null)
            {
                if (!WalletController.DeductBalanceLegacy(HttpContext.Session, total))
                {
                    TempData["Error"] = "Số dư ví không đủ để thanh toán.";
                    return RedirectToAction("Checkout");
                }
            }
            else
            {
                var ok = await WalletController.DeductBalance(_db, userId.Value, total);
                if (!ok)
                {
                    TempData["Error"] = "Số dư ví không đủ để thanh toán.";
                    return RedirectToAction("Checkout");
                }
            }
        }

        var order = new Order
        {
            UserId = GetUserId(),
            CustomerName = customerName,
            Phone = phone,
            Email = email,
            Address = address,
            Note = note ?? string.Empty,
            OrderDate = DateTime.Now,
            Items = cart.Select(c => new OrderItem
            {
                ProductId = c.ProductId,
                ProductName = c.ProductName,
                Price = c.Price,
                Quantity = c.Quantity
            }).ToList(),
            TotalAmount = total,
            Status = "Chờ xác nhận",
            PaymentMethod = paymentMethod,
            PaymentMethodLabel = methodLabels[paymentMethod]
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        ClearCart();

        TempData["Success"] = $"Đặt hàng thành công! Mã đơn hàng: #{order.Id}";
        return RedirectToAction("Detail", new { id = order.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();
        return View(order);
    }
}
