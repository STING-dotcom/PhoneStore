using Microsoft.AspNetCore.Mvc;
using PhoneStore.Models;
using System.Text.Json;

namespace PhoneStore.Controllers;

public class CartController : Controller
{
    private const string CartSessionKey = "CartItems";

    private List<CartItem> GetCart()
    {
        var json = HttpContext.Session.GetString(CartSessionKey);
        return json is null ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
    }

    private void SaveCart(List<CartItem> cart)
    {
        HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
    }

    public IActionResult Index()
    {
        var cart = GetCart();
        ViewBag.Total = cart.Sum(c => c.Subtotal);
        return View(cart);
    }

    [HttpPost]
    public IActionResult AddToCart(int productId)
    {
        var products = GetProducts();
        var product = products.FirstOrDefault(p => p.Id == productId);
        if (product is null) return NotFound();

        var cart = GetCart();
        var existing = cart.FirstOrDefault(c => c.ProductId == productId);
        if (existing is not null)
        {
            existing.Quantity++;
        }
        else
        {
            cart.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ImageUrl = product.ImageUrl,
                Price = product.Price,
                Quantity = 1
            });
        }

        SaveCart(cart);
        TempData["Success"] = $"Đã thêm &quot;{product.Name}&quot; vào giỏ hàng!";
        return RedirectToAction("Index", "Cart");
    }

    [HttpPost]
    public IActionResult BuyNow(int productId)
    {
        var products = GetProducts();
        var product = products.FirstOrDefault(p => p.Id == productId);
        if (product is null) return NotFound();

        var cart = GetCart();
        cart.Clear();
        cart.Add(new CartItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            ImageUrl = product.ImageUrl,
            Price = product.Price,
            Quantity = 1
        });

        SaveCart(cart);
        return RedirectToAction("Checkout", "Order");
    }

    [HttpPost]
    public IActionResult UpdateQuantity(int productId, int quantity)
    {
        if (quantity < 1) return RedirectToAction("Index");

        var cart = GetCart();
        var item = cart.FirstOrDefault(c => c.ProductId == productId);
        if (item is not null)
        {
            item.Quantity = quantity;
            SaveCart(cart);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult RemoveFromCart(int productId)
    {
        var cart = GetCart();
        cart.RemoveAll(c => c.ProductId == productId);
        SaveCart(cart);
        return RedirectToAction("Index");
    }

    private static List<Product> GetProducts() => ProductController.GetAllProducts();
}
