using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
namespace RestaurantManagement.Controllers;
public class KhuyenMaiController(QuanLyNhaHangContext db):Controller {
 private IQueryable<KhuyenMai> Available(){var today=DateOnly.FromDateTime(BookingRules.Now);return db.KhuyenMais.AsNoTracking().Where(x=>x.TrangThai=="Hoạt động"&&x.NgayBatDau<=today&&x.NgayKetThuc>=today&&x.MonApDung.Any(m=>m.MonAn.TrangThai=="Hiển thị")).Include(x=>x.MonApDung.Where(m=>m.MonAn.TrangThai=="Hiển thị")).ThenInclude(x=>x.MonAn);}
 public async Task<IActionResult> Index()=>View(await Available().OrderByDescending(x=>x.PhanTramGiam).ToListAsync());
 public async Task<IActionResult> Details(int id){var item=await Available().FirstOrDefaultAsync(x=>x.MaKhuyenMai==id);return item==null?NotFound():View(item);}
}
