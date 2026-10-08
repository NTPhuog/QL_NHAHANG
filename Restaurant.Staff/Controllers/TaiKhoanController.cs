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
  var user=await db.NhanViens.FirstOrDefaultAsync(x=>x.Email==email&&x.TrangThai=="Hoạt động");
  if(user==null||!Passwords.Verify(user.MatKhau,matKhau)){ViewBag.Loi="Email hoặc mật khẩu không đúng, hoặc tài khoản bị khóa.";return View();}
  if(!user.MatKhau.StartsWith("AQAAAA")){user.MatKhau=Passwords.Hash(matKhau);await db.SaveChangesAsync();}
  HttpContext.Session.Clear();HttpContext.Session.SetInt32("UserId",user.MaNhanVien);HttpContext.Session.SetString("Role","NhanVien");HttpContext.Session.SetString("HoTen",user.HoTen);
  
  if(!string.IsNullOrEmpty(returnUrl)&&Url.IsLocalUrl(returnUrl))return LocalRedirect(returnUrl);
  return RedirectToAction("Index","Home",new{area="NhanVien"});
 }
 [HttpPost] public IActionResult DangXuat(){HttpContext.Session.Clear();return RedirectToAction("DangNhap");}
}




