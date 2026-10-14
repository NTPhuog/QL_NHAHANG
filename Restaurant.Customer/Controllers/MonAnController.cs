using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
namespace RestaurantManagement.Controllers;
public class MonAnController(QuanLyNhaHangContext db):Controller
{
 public async Task<IActionResult> Index(string? q,int? danhMuc,string? sapXep,int page=1,bool moi=false){
  q=q?.Trim();if(q?.Length>100)q=q[..100];
  var query=db.MonAns.AsNoTracking().Include(x=>x.MaDanhMucNavigation).Where(x=>x.TrangThai=="Hiển thị");
  ViewBag.DanhMuc=await db.DanhMucMons.AsNoTracking().OrderBy(x=>x.TenDanhMuc).ToListAsync();
  if(!string.IsNullOrWhiteSpace(q))query=query.Where(x=>x.TenMon.Contains(q)||x.MoTa!=null&&x.MoTa.Contains(q));
  ViewBag.Moi=moi;var cutoff=BookingRules.Now.AddDays(-30);if(moi)query=query.Where(x=>x.NgayThem>=cutoff);
  if(danhMuc.HasValue)query=query.Where(x=>x.MaDanhMuc==danhMuc);
  var count=await query.CountAsync();var pages=Math.Max(1,(int)Math.Ceiling(count/12.0));page=Math.Clamp(page,1,pages);
  query=sapXep switch{"gia-tang"=>query.OrderBy(x=>x.Gia).ThenBy(x=>x.MaMon),"gia-giam"=>query.OrderByDescending(x=>x.Gia).ThenBy(x=>x.MaMon),_=>moi?query.OrderByDescending(x=>x.NgayThem).ThenByDescending(x=>x.MaMon):query.OrderBy(x=>x.TenMon).ThenBy(x=>x.MaMon)};
  ViewBag.Query=q;ViewBag.Category=danhMuc;ViewBag.Sort=sapXep;ViewBag.Count=count;ViewBag.Page=page;ViewBag.Pages=pages;
  var dishes=await query.Skip((page-1)*12).Take(12).ToListAsync(); ViewBag.Discounts=await PromotionPricing.DiscountsAsync(db,dishes.Select(x=>x.MaMon)); return View(dishes);
 }
 public async Task<IActionResult> Details(int id){var item=await db.MonAns.AsNoTracking().Include(x=>x.MaDanhMucNavigation).FirstOrDefaultAsync(x=>x.MaMon==id&&x.TrangThai=="Hiển thị");if(item==null)return NotFound(); ViewBag.Discounts=await PromotionPricing.DiscountsAsync(db,new[]{id}); return View(item);}
}
