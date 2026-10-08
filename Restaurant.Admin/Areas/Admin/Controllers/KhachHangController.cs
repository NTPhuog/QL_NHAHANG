using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
namespace RestaurantManagement.Areas.Admin.Controllers;
[Area("Admin")]
public class KhachHangController(QuanLyNhaHangContext db):Controller {
 public async Task<IActionResult> Index(string? q){var list=db.KhachHangs.AsNoTracking();if(!string.IsNullOrWhiteSpace(q))list=list.Where(x=>x.HoTen.Contains(q)||x.Email.Contains(q));return View(await list.OrderBy(x=>x.HoTen).ToListAsync());}
 [HttpPost] public async Task<IActionResult> TrangThai(int id,string trangThai){if(trangThai is not("Hoạt động" or "Khóa"))return BadRequest();await db.KhachHangs.Where(x=>x.MaKhachHang==id).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.TrangThai,trangThai));TempData["ThongBao"]="Đã cập nhật tài khoản.";return RedirectToAction(nameof(Index));}
}

