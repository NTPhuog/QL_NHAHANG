using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
namespace RestaurantManagement.Areas.Admin.Controllers;
[Area("Admin")]
public class HomeController(QuanLyNhaHangContext db):Controller {
 public async Task<IActionResult> Index(){ViewBag.Ban=await db.BanAns.CountAsync();ViewBag.Khach=await db.KhachHangs.CountAsync();ViewBag.NhanVien=await db.NhanViens.CountAsync();ViewBag.Cho=await db.DatBans.CountAsync(x=>x.TrangThai=="Chờ xác nhận");return View(await db.DatBans.AsNoTracking().Include(x=>x.MaBanNavigation).Include(x=>x.MaKhachHangNavigation).OrderByDescending(x=>x.NgayTao).Take(10).ToListAsync());}
}

