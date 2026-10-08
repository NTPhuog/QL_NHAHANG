using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
using RestaurantManagement.Security;
using System.ComponentModel.DataAnnotations;
namespace RestaurantManagement.Areas.Admin.Controllers;
[Area("Admin")]
public class NhanSuController(QuanLyNhaHangContext db):Controller {
 public async Task<IActionResult> Index()=>View(await db.NhanViens.AsNoTracking().OrderBy(x=>x.HoTen).ToListAsync());
 public async Task<IActionResult> Edit(int? id){var u=id.HasValue?await db.NhanViens.FindAsync(id):new RestaurantManagement.Models.NhanVien{TrangThai="Hoạt động",ChucVu="Lễ tân"};if(u==null)return NotFound();u.MatKhau="";return View(u);}
 [HttpPost] public async Task<IActionResult> Edit(int id,string hoTen,string email,string? soDienThoai,string? chucVu,string trangThai,string? matKhau){
  var u=id==0?new RestaurantManagement.Models.NhanVien():await db.NhanViens.FindAsync(id);if(u==null)return NotFound();
  hoTen=(hoTen??"").Trim();email=(email??"").Trim().ToLowerInvariant();
  if(hoTen.Length is <2 or >100 || email.Length>100 || !new EmailAddressAttribute().IsValid(email)||soDienThoai?.Length>15||chucVu?.Length>50||trangThai is not("Hoạt động" or "Khóa") || (id==0 && string.IsNullOrEmpty(matKhau)) || (!string.IsNullOrEmpty(matKhau)&&matKhau.Length is <8 or >128))ModelState.AddModelError("","Kiểm tra thông tin: tên, email hợp lệ; mật khẩu mới từ 8–128 ký tự.");
  if(await db.NhanViens.AnyAsync(x=>x.Email==email && x.MaNhanVien!=id))ModelState.AddModelError("","Email đã tồn tại.");
  u.HoTen=hoTen;u.Email=email;u.SoDienThoai=soDienThoai;u.ChucVu=chucVu;u.TrangThai=trangThai;
  if(!ModelState.IsValid){u.MatKhau="";return View(u);}
  if(!string.IsNullOrEmpty(matKhau))u.MatKhau=Passwords.Hash(matKhau);
  if(id==0)db.Add(u);
  try{await db.SaveChangesAsync();}catch(DbUpdateException){ModelState.AddModelError("","Không thể lưu nhân viên. Kiểm tra email và thử lại.");u.MatKhau="";return View(u);}
  TempData["ThongBao"]="Đã lưu nhân viên.";return RedirectToAction(nameof(Index));
 }
}


