using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
using RestaurantManagement.Security;
using System.ComponentModel.DataAnnotations;
namespace RestaurantManagement.Controllers;
public class TaiKhoanController(QuanLyNhaHangContext db) : Controller
{
 [HttpGet] public IActionResult DangNhap(string? returnUrl=null){ViewBag.ReturnUrl=returnUrl;return View();}
 [HttpPost] public async Task<IActionResult> DangNhap(string email,string matKhau,string? returnUrl=null)
 {
  ViewBag.ReturnUrl=returnUrl;
  email=(email??"").Trim().ToLowerInvariant();
  var user=await db.KhachHangs.FirstOrDefaultAsync(x=>x.Email==email&&x.TrangThai=="Hoạt động");
  if(user==null||!Passwords.Verify(user.MatKhau,matKhau)){ViewBag.Loi="Email hoặc mật khẩu không đúng, hoặc tài khoản bị khóa.";return View();}
  if(!user.MatKhau.StartsWith("AQAAAA")){user.MatKhau=Passwords.Hash(matKhau);await db.SaveChangesAsync();}
  HttpContext.Session.Clear();HttpContext.Session.SetInt32("UserId",user.MaKhachHang);HttpContext.Session.SetString("Role","KhachHang");HttpContext.Session.SetString("HoTen",user.HoTen);
  HttpContext.Session.SetInt32("MaKhachHang",user.MaKhachHang);
  if(!string.IsNullOrEmpty(returnUrl)&&Url.IsLocalUrl(returnUrl))return LocalRedirect(returnUrl);
  return RedirectToAction("Index","Home");
 }
 [HttpGet] public IActionResult DangKy()=>View();
 [HttpPost] public async Task<IActionResult> DangKy(string hoTen,string soDienThoai,string email,string matKhau,string nhapLaiMatKhau,string? diaChi)
 {
  email=(email??"").Trim().ToLowerInvariant();hoTen=(hoTen??"").Trim();
  if(hoTen.Length is <2 or >100 || email.Length>100 || !new EmailAddressAttribute().IsValid(email) || !System.Text.RegularExpressions.Regex.IsMatch(soDienThoai??"",@"^\+?[0-9]{9,14}$") || string.IsNullOrEmpty(matKhau) || matKhau.Length<8 || matKhau.Length>128 || matKhau!=nhapLaiMatKhau || diaChi?.Length>255){ViewBag.Loi="Kiểm tra họ tên, email, số điện thoại; mật khẩu từ 8–128 ký tự và nhập lại phải khớp.";return View();}
  if(await db.KhachHangs.AnyAsync(x=>x.Email==email)){ViewBag.Loi="Email đã được sử dụng.";return View();}
  var u=new KhachHang{HoTen=hoTen,Email=email,SoDienThoai=soDienThoai,MatKhau=Passwords.Hash(matKhau),DiaChi=diaChi,TrangThai="Hoạt động"};
  db.Add(u);try{await db.SaveChangesAsync();}catch(DbUpdateException){ViewBag.Loi="Không thể tạo tài khoản. Vui lòng kiểm tra email hoặc thử lại.";return View();}
  HttpContext.Session.Clear();HttpContext.Session.SetInt32("UserId",u.MaKhachHang);HttpContext.Session.SetInt32("MaKhachHang",u.MaKhachHang);HttpContext.Session.SetString("Role","KhachHang");HttpContext.Session.SetString("HoTen",u.HoTen);
  return RedirectToAction("Index","Home");
 }
 [HttpPost] public IActionResult DangXuat(){HttpContext.Session.Clear();return RedirectToAction("DangNhap");}
}




