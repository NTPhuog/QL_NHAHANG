using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
namespace RestaurantManagement.Security;
public class AccessFilter(QuanLyNhaHangContext db) : IAsyncActionFilter
{
 public async Task OnActionExecutionAsync(ActionExecutingContext c, ActionExecutionDelegate next)
 {
  var s=c.HttpContext.Session;
  var role=s.GetString("Role");
  var id=s.GetInt32("UserId");
  if(id.HasValue) {
   var active=role switch {
    "Admin" => await db.Admins.AnyAsync(x=>x.MaAdmin==id && x.TrangThai=="Hoạt động"),
    "NhanVien" => await db.NhanViens.AnyAsync(x=>x.MaNhanVien==id && x.TrangThai=="Hoạt động"),
    "KhachHang" => await db.KhachHangs.AnyAsync(x=>x.MaKhachHang==id && x.TrangThai=="Hoạt động"),
    _ => false };
   if(!active){s.Clear();role=null;}
  } else role=null;
  var descriptor = c.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
  var controller = descriptor?.ControllerName ?? c.RouteData.Values["controller"]?.ToString();
  var action = descriptor?.ActionName ?? c.RouteData.Values["action"]?.ToString();
  var area = c.RouteData.Values["area"]?.ToString();
  var required = AccessPolicy.RequiredRole(area, controller, action);
  if(required!=null && role==null) {
   c.Result=new RedirectToActionResult(required=="KhachHang"?"DangNhap":"NoiBo","TaiKhoan",new {area="",returnUrl=c.HttpContext.Request.Path+c.HttpContext.Request.QueryString});return;
  }
  if(!AccessPolicy.Allows(required, role)) { c.Result=new StatusCodeResult(403);return; }
  await next();
 }
}



