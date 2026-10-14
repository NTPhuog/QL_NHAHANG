using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
using RestaurantManagement.Services;
namespace RestaurantManagement.Areas.NhanVien.Controllers;
[Area("NhanVien")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class DatBanController(QuanLyNhaHangContext db,BookingWorkflow workflow,ILogger<DatBanController> logger):Controller {
 public async Task<IActionResult> Index(DateOnly? ngay,string? trangThai,string? q,bool archive=false){
  var query=db.DatBans.AsNoTracking().Include(x=>x.MaBanNavigation).Include(x=>x.MaKhachHangNavigation).AsQueryable();
  query=archive?query.Where(x=>x.TrangThai=="Hoàn thành"||x.TrangThai=="Đã hủy"):query.Where(x=>x.TrangThai!="Hoàn thành"&&x.TrangThai!="Đã hủy");
  if(ngay.HasValue)query=query.Where(x=>x.NgayDat==ngay);if(!string.IsNullOrWhiteSpace(trangThai))query=query.Where(x=>x.TrangThai==trangThai);
  if(!string.IsNullOrWhiteSpace(q)){q=q.Trim();query=query.Where(x=>x.MaCheckIn==q||x.MaHoaDon==q||(x.MaKhachHangNavigation.HoTen.Contains(q)||x.MaKhachHangNavigation.SoDienThoai!.Contains(q)));}
  ViewBag.Archive=archive;return View(await query.OrderByDescending(x=>x.NgayDat).ThenBy(x=>x.GioDat).ToListAsync());
 }
 public async Task<IActionResult> Notifications()=>Json(await BookingNotices.ReadAsync(db));
 public IActionResult CheckIn()=>View();
 [HttpPost] public async Task<IActionResult> CheckIn(string? code){try{var id=await workflow.CheckIn(code);TempData["ThongBao"]="Check-in thành công. Có thể gọi món tại bàn.";return RedirectToAction(nameof(Serve),new{id});}catch(WorkflowException ex){ModelState.AddModelError("",ex.Message);return View();}catch(Exception ex)when(ex is DbUpdateException or Microsoft.Data.SqlClient.SqlException){logger.LogError(ex,"Check-in failed");ModelState.AddModelError("","Chưa check-in được. Vui lòng thử lại.");return View();}}
 private async Task<DatBan?> Order(int id)=>await db.DatBans.AsNoTracking().Include(x=>x.MaBanNavigation).Include(x=>x.MaKhachHangNavigation).Include(x=>x.CacMon).FirstOrDefaultAsync(x=>x.MaDatBan==id);
 public async Task<IActionResult> Serve(int id){var d=await Order(id);if(d==null)return NotFound();if(d.MaHoaDon!=null)return RedirectToAction(nameof(Bill),new{id});if(d.TrangThai!="Đang phục vụ"){TempData["Loi"]="Quét hoặc nhập mã để check-in trước khi gọi món.";return RedirectToAction(nameof(CheckIn));}var menu=await db.MonAns.AsNoTracking().Where(x=>x.TrangThai=="Hiển thị").OrderBy(x=>x.TenMon).ToListAsync();ViewBag.Menu=menu;ViewBag.Discounts=await PromotionPricing.DiscountsAsync(db,menu.Select(x=>x.MaMon));return View(d);}
 [HttpPost] public async Task<IActionResult> AddDish(int id,int maMon,int soLuong,Guid requestId){try{if(!ModelState.IsValid)throw new WorkflowException("Số lượng hoặc món không hợp lệ.");await workflow.AddDish(id,maMon,soLuong,requestId);TempData["ThongBao"]="Đã thêm món.";}catch(WorkflowException ex){TempData["Loi"]=ex.Message;}return RedirectToAction(nameof(Serve),new{id});}
 [HttpPost] public async Task<IActionResult> Finish(int id,decimal expectedTotal){try{if(!ModelState.IsValid)throw new WorkflowException("Tổng tiền không hợp lệ.");await workflow.Finish(id,HttpContext.Session.GetInt32("UserId")!.Value,expectedTotal);return RedirectToAction(nameof(Bill),new{id});}catch(WorkflowException ex){TempData["Loi"]=ex.Message;return RedirectToAction(nameof(Serve),new{id});}}
 public async Task<IActionResult> Bill(int id){var d=await Order(id);return d?.MaHoaDon==null?NotFound():View(d);}
 [HttpPost] public async Task<IActionResult> Huy(int id,string? lyDoHuy){try{if(string.IsNullOrWhiteSpace(lyDoHuy)||lyDoHuy.Length>255)throw new WorkflowException("Nhập lý do hủy từ 1–255 ký tự.");await workflow.Cancel(id,null,lyDoHuy.Trim());TempData["ThongBao"]="Đã hủy đơn và hoàn cọc giả lập nếu có.";}catch(WorkflowException ex){TempData["Loi"]=ex.Message;}return RedirectToAction(nameof(Index));}
}
