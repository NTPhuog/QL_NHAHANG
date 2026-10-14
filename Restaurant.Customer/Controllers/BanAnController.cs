using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
using RestaurantManagement.CustomerModels;
namespace RestaurantManagement.Controllers;
public class BanAnController(QuanLyNhaHangContext db):Controller {
 [ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
 public async Task<IActionResult> Index(DateOnly? ngay,TimeOnly? gio,int khach=2,int? loai=null){
  var now=BookingRules.Now;
  var vm=new TableSelection{Date=ngay??DateOnly.FromDateTime(now.AddDays(1)),Time=gio??new TimeOnly(18,0),Guests=khach,Group=loai};
  if(!ModelState.IsValid||vm.Date.ToDateTime(vm.Time)<=now||vm.Time<new TimeOnly(10,0)||vm.Time>new TimeOnly(20,0)||khach<1||khach>24){vm.Valid=false;ModelState.AddModelError("","Chọn thời gian trong tương lai, giờ từ 10:00–20:00 và từ 1–24 khách.");}
  var tables=await db.BanAns.AsNoTracking().OrderBy(x=>x.TenBan).ToListAsync();
  var orders=await db.DatBans.AsNoTracking().Where(x=>x.NgayDat==vm.Date&&(x.TrangThai=="Chờ xác nhận"||x.TrangThai=="Đã duyệt"||x.TrangThai=="Đang phục vụ")).Select(x=>new{x.MaBan,x.GioDat}).ToListAsync();
  var busy=orders.Where(x=>Math.Abs((x.GioDat.ToTimeSpan()-vm.Time.ToTimeSpan()).TotalMinutes)<120).Select(x=>x.MaBan).ToHashSet();
  var serving=await db.DatBans.AsNoTracking().Where(x=>x.TrangThai=="Đang phục vụ").ToListAsync();var requested=vm.Date.ToDateTime(vm.Time);foreach(var d in serving.Where(x=>requested<x.NgayDat.ToDateTime(x.GioDat).AddHours(2)&&requested.AddHours(2)>(x.CheckInUtc?.AddHours(7)??now)))busy.Add(d.MaBan);
  var utc=DateTime.UtcNow;var holds=await db.GiuBans.AsNoTracking().Where(x=>x.Ngay==vm.Date&&x.HetHanUtc>utc&&x.MaDatBan==null).Select(x=>new{x.MaBan,x.Gio,x.MaKhachHang}).ToListAsync();var held=holds.Where(x=>RestaurantManagement.Services.BookingWorkflow.Overlap(x.Gio,vm.Time)).Select(x=>x.MaBan).ToHashSet();
  var owner=HttpContext.Session.GetInt32("MaKhachHang");var own=holds.Where(x=>x.MaKhachHang==owner&&x.Gio==vm.Time).Select(x=>x.MaBan).ToHashSet();
  vm.Tables=tables.Select(x=>new TableOption(x,busy.Contains(x.MaBan),held.Contains(x.MaBan),own.Contains(x.MaBan))).ToList();return View(vm);
 }
 public IActionResult Details(int id)=>RedirectToAction("Index");
}
