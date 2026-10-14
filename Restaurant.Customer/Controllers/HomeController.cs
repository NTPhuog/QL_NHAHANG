using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Models;

namespace RestaurantManagement.Controllers;

public class HomeController(QuanLyNhaHangContext db) : Controller
{
    public async Task<IActionResult> Index(){var cutoff=BookingRules.Now.AddDays(-30);var dishes=await db.MonAns.AsNoTracking().Where(x=>x.TrangThai=="Hiển thị"&&x.NgayThem>=cutoff).OrderByDescending(x=>x.NgayThem).ThenByDescending(x=>x.MaMon).Take(3).ToListAsync();ViewBag.Discounts=await PromotionPricing.DiscountsAsync(db,dishes.Select(x=>x.MaMon));return View(dishes);}

    public IActionResult Privadocy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
