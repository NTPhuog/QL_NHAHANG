using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
namespace RestaurantManagement.Controllers;
public class KhuyenMaiController(QuanLyNhaHangContext db) : Controller {
 public async Task<IActionResult> Index(){var today=DateOnly.FromDateTime(BookingRules.Now);return View(await db.KhuyenMais.AsNoTracking().Where(x=>x.TrangThai=="Hoạt động"&&x.NgayBatDau<=today&&x.NgayKetThuc>=today).ToListAsync());}
 public async Task<IActionResult> Details(int id){var today=DateOnly.FromDateTime(BookingRules.Now);var item=await db.KhuyenMais.AsNoTracking().FirstOrDefaultAsync(x=>x.MaKhuyenMai==id&&x.TrangThai=="Hoạt động"&&x.NgayBatDau<=today&&x.NgayKetThuc>=today);return item==null?NotFound():View(item);}
}
