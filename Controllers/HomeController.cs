using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Models;

namespace PhoneStore.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _db;

    public HomeController(ILogger<HomeController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var newProducts = await _db.Products
            .OrderByDescending(p => p.Price)
            .Take(12)
            .ToListAsync();

        var trendingIds = await _db.Reviews
            .GroupBy(r => r.ProductId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(10)
            .ToListAsync();

        var trendingProducts = await _db.Products
            .Where(p => trendingIds.Contains(p.Id))
            .ToListAsync();

        if (trendingProducts.Count < 10)
        {
            var existingIds = new HashSet<int>(trendingProducts.Select(p => p.Id));
            var extra = await _db.Products
                .Where(p => !existingIds.Contains(p.Id))
                .OrderByDescending(p => p.Price)
                .Take(10 - trendingProducts.Count)
                .ToListAsync();
            trendingProducts.AddRange(extra);
        }

        // Pre-compute ratings for all product IDs displayed
        var allProductIds = newProducts.Concat(trendingProducts).Select(p => p.Id).Distinct().ToList();
        var ratingData = await _db.Reviews
            .Where(r => allProductIds.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new { ProductId = g.Key, Avg = g.Average(r => (double)r.Rating), Count = g.Count() })
            .ToListAsync();
        ViewBag.AvgRatings = ratingData.ToDictionary(r => r.ProductId, r => r.Avg);
        ViewBag.ReviewCounts = ratingData.ToDictionary(r => r.ProductId, r => r.Count);

        var model = Tuple.Create(newProducts, trendingProducts);
        return View(model);
    }

    [HttpGet]
    public IActionResult Security() => View();

    public IActionResult AccessDenied() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
