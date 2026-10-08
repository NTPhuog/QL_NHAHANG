using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Hosting;
using RestaurantManagement.Models;
namespace RestaurantManagement.Security;
public class SiteAccessFilter(QuanLyNhaHangContext db, SiteIdentity site):IAsyncActionFilter
{
 public async Task OnActionExecutionAsync(ActionExecutingContext context,ActionExecutionDelegate next)
 {
  var descriptor=(ControllerActionDescriptor)context.ActionDescriptor;
  var area=context.RouteData.Values["area"]?.ToString();
  if(!string.IsNullOrEmpty(area)&&!area.Equals(site.Role,StringComparison.OrdinalIgnoreCase)){context.Result=new NotFoundResult();return;}
  var session=context.HttpContext.Session;
  var role=session.GetString("Role");var id=session.GetInt32("UserId");
  if(role!=site.Role){session.Clear();id=null;role=null;}
  if(id.HasValue){
   var active=site.Role switch{
    "Admin"=>await db.Admins.AnyAsync(x=>x.MaAdmin==id&&x.TrangThai=="Hoạt động"),
    "NhanVien"=>await db.NhanViens.AnyAsync(x=>x.MaNhanVien==id&&x.TrangThai=="Hoạt động"),
    _=>await db.KhachHangs.AnyAsync(x=>x.MaKhachHang==id&&x.TrangThai=="Hoạt động")};
   if(!active){session.Clear();id=null;}
  }
  var publicPage=descriptor.ControllerName=="TaiKhoan" || site.Role=="KhachHang"&&descriptor.ControllerName is "Home" or "BanAn" or "MonAn" or "KhuyenMai";
  if(!publicPage&&!id.HasValue){context.Result=new RedirectToActionResult("DangNhap","TaiKhoan",new{area="",returnUrl=context.HttpContext.Request.Path+context.HttpContext.Request.QueryString});return;}
  await next();
 }
}
