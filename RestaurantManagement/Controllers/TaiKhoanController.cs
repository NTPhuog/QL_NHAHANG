using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
using RestaurantManagement.Security;
using System.ComponentModel.DataAnnotations;
namespace RestaurantManagement.Controllers;
public class TaiKhoanController(QuanLyNhaHangContext db) : Controller
{
 [HttpGet] public IActionResult DangNhap(string? returnUrl=null){ViewBag.ReturnUrl=returnUrl;return View();}
 [HttpPost] public async Task<IActionResult> DangNhap(string email,string matKhau,string vaiTro="KhachHang",string? returnUrl=null)
 {
  ViewBag.ReturnUrl=returnUrl;
  var internalLogin=string.Equals(RouteData.Values["action"]?.ToString(),"NoiBo",StringComparison.OrdinalIgnoreCase);
  if(!internalLogin)vaiTro="KhachHang"; else if(vaiTro is not("Admin" or "NhanVien")){ViewBag.Loi="Vai trò không hợp lệ.";return View("NoiBo");}
  email=(email??"").Trim().ToLowerInvariant();
  int id=0;string name="";string? hash=null;
  if(vaiTro=="Admin") {var u=await db.Admins.FirstOrDefaultAsync(x=>x.Email==email && x.TrangThai=="Hoạt động");if(u!=null&&Passwords.Verify(u.MatKhau,matKhau)){id=u.MaAdmin;name=u.HoTen;hash=u.MatKhau;if(!hash.StartsWith("AQAAAA"))u.MatKhau=Passwords.Hash(matKhau);}}
  else if(vaiTro=="NhanVien") {var u=await db.NhanViens.FirstOrDefaultAsync(x=>x.Email==email && x.TrangThai=="Hoạt động");if(u!=null&&Passwords.Verify(u.MatKhau,matKhau)){id=u.MaNhanVien;name=u.HoTen;hash=u.MatKhau;if(!hash.StartsWith("AQAAAA"))u.MatKhau=Passwords.Hash(matKhau);}}
  else if(vaiTro=="KhachHang") {var u=await db.KhachHangs.FirstOrDefaultAsync(x=>x.Email==email && x.TrangThai=="Hoạt động");if(u!=null&&Passwords.Verify(u.MatKhau,matKhau)){id=u.MaKhachHang;name=u.HoTen;hash=u.MatKhau;if(!hash.StartsWith("AQAAAA"))u.MatKhau=Passwords.Hash(matKhau);}}
  if(id==0){ViewBag.Loi="Thông tin đăng nhập không đúng hoặc tài khoản đã bị khóa.";return View();}
  await db.SaveChangesAsync();
  HttpContext.Session.Clear();HttpContext.Session.SetInt32("UserId",id);HttpContext.Session.SetString("Role",vaiTro);HttpContext.Session.SetString("HoTen",name);
  if(vaiTro=="KhachHang")HttpContext.Session.SetInt32("MaKhachHang",id);
  if(!string.IsNullOrEmpty(returnUrl)&&Url.IsLocalUrl(returnUrl))return LocalRedirect(returnUrl);
  return vaiTro switch {"Admin"=>RedirectToAction("Index","Home",new{area="Admin"}),"NhanVien"=>RedirectToAction("Index","DatBan",new{area="NhanVien"}),_=>RedirectToAction("Index","Home")};
 }
 [HttpGet] public IActionResult NoiBo(string? returnUrl=null){ViewBag.ReturnUrl=returnUrl;return View();}
 [HttpPost] public Task<IActionResult> NoiBo(string email,string matKhau,string vaiTro,string? returnUrl=null)=>DangNhap(email,matKhau,vaiTro,returnUrl);
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
 [HttpPost] public IActionResult DangXuat(){var staff=HttpContext.Session.GetString("Role") is "Admin" or "NhanVien";HttpContext.Session.Clear();return staff?RedirectToAction("NoiBo"):RedirectToAction("Index","Home");}
}


