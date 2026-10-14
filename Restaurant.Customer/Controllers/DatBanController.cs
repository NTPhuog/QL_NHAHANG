using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
using RestaurantManagement.Services;
using RestaurantManagement.CustomerModels;
using QRCoder;
namespace RestaurantManagement.Controllers;
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class DatBanController(QuanLyNhaHangContext db,BookingWorkflow workflow,ILogger<DatBanController> logger):Controller {
 public async Task<IActionResult> Notifications()=>Json(await BookingNotices.ReadAsync(db,UserId));
 private int UserId=>HttpContext.Session.GetInt32("MaKhachHang")!.Value;
 public IActionResult Index()=>RedirectToAction(nameof(LichSu));
 private async Task<BookingForm> Form(BanAn table,DateOnly day,TimeOnly time,int guests,Guid? hold=null,string? note=null,List<ChonMon>? items=null){var menu=await db.MonAns.AsNoTracking().Where(x=>x.TrangThai=="Hiển thị").OrderBy(x=>x.TenMon).ToListAsync();return new(){Table=table,Date=day,Time=time,Guests=guests,HoldId=hold,Note=note,Selected=items??[],Menu=menu,Discounts=await PromotionPricing.DiscountsAsync(db,menu.Select(x=>x.MaMon))};}
 [HttpGet] public async Task<IActionResult> Create(int? maBan,DateOnly? ngay,TimeOnly? gio,int? khach,Guid? hold){
  if(hold.HasValue){var h=await db.GiuBans.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==hold&&x.MaKhachHang==UserId);if(h==null)return NotFound();if(h.MaDatBan.HasValue)return RedirectToAction(nameof(Details),new{id=h.MaDatBan});return View(await Form(await db.BanAns.FindAsync(h.MaBan)??throw new InvalidOperationException(),h.Ngay,h.Gio,h.SoKhach,h.Id,h.GhiChu,BookingWorkflow.Basket(h).Select(x=>new ChonMon(x.MaMon,x.SoLuong)).ToList()));}
  var b=await db.BanAns.FindAsync(maBan);if(b==null)return RedirectToAction("Index","BanAn");return View(await Form(b,ngay??DateOnly.FromDateTime(BookingRules.Now.AddDays(1)),gio??new TimeOnly(18,0),Math.Clamp(khach??1,1,b.SoChoNgoi)));
 }
 [HttpPost] public async Task<IActionResult> GiuCho(int maBan,DateOnly ngay,TimeOnly gio,int khach){
  if(!ModelState.IsValid)return BadRequest(new{message="Ngày, giờ hoặc số khách không hợp lệ."});
  try{var h=await workflow.Acquire(UserId,maBan,ngay,gio,khach);return Json(new{id=h.Id,remaining=Math.Max(0,(h.HetHanUtc-DateTime.UtcNow).TotalSeconds)});}catch(WorkflowException ex){return Conflict(new{message=ex.Message});}catch(Exception ex)when(ex is DbUpdateException or Microsoft.Data.SqlClient.SqlException){logger.LogError(ex,"Hold acquisition failed");return Conflict(new{message="Có khách vừa thao tác cùng lúc. Vui lòng thử giữ bàn lại."});}
 }
 [HttpGet] public async Task<IActionResult> HoldStatus(Guid id){var h=await db.GiuBans.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id&&x.MaKhachHang==UserId);if(h==null)return NotFound();return Json(new{id=h.Id,remaining=h.MaDatBan.HasValue?0:Math.Max(0,(h.HetHanUtc-DateTime.UtcNow).TotalSeconds)});}
 [HttpPost] public async Task<IActionResult> NhaBan(Guid holdId){await workflow.Release(UserId,holdId);return RedirectToAction("Index","BanAn");}
 [HttpPost] public async Task<IActionResult> Create(Guid holdId,int guests,string? note,List<ChonMon>? items){
  try{if(!ModelState.IsValid)throw new WorkflowException("Kiểm tra lại số lượng món và số khách.");await workflow.Prepare(UserId,holdId,guests,note,items);return RedirectToAction(nameof(Checkout),new{id=holdId});}
  catch(WorkflowException ex){ModelState.AddModelError("",ex.Message);var h=await db.GiuBans.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==holdId&&x.MaKhachHang==UserId);if(h==null){TempData["Loi"]=ex.Message;return RedirectToAction("Index","BanAn");}return View(await Form((await db.BanAns.AsNoTracking().SingleAsync(x=>x.MaBan==h.MaBan)),h.Ngay,h.Gio,guests,h.Id,note,items));}
 }
 [HttpGet] public async Task<IActionResult> Checkout(Guid id){var h=await db.GiuBans.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id&&x.MaKhachHang==UserId);if(h==null)return NotFound();if(h.MaDatBan.HasValue)return RedirectToAction(nameof(Details),new{id=h.MaDatBan});ViewBag.Table=await db.BanAns.FindAsync(h.MaBan);return View(h);}
 [HttpPost] public async Task<IActionResult> Confirm(Guid holdId,bool simulatedSuccess){try{var id=await workflow.Confirm(UserId,holdId,simulatedSuccess);TempData["ThongBao"]="Đặt bàn thành công. Mang mã QR hoặc mã đặt bàn đến lễ tân để check-in.";return RedirectToAction(nameof(Details),new{id});}catch(WorkflowException ex){TempData["Loi"]=ex.Message;return RedirectToAction(nameof(Checkout),new{id=holdId});}catch(Exception ex)when(ex is DbUpdateException or Microsoft.Data.SqlClient.SqlException){logger.LogError(ex,"Booking confirmation failed");TempData["Loi"]="Chưa xác nhận được. Hãy tải lại; hệ thống không thu cọc hai lần.";return RedirectToAction(nameof(Checkout),new{id=holdId});}}
 public async Task<IActionResult> Details(int id){var d=await db.DatBans.AsNoTracking().Include(x=>x.MaKhachHangNavigation).Include(x=>x.MaBanNavigation).Include(x=>x.CacMon).FirstOrDefaultAsync(x=>x.MaDatBan==id&&x.MaKhachHang==UserId);return d==null?NotFound():View(d);}
 public async Task<IActionResult> Qr(int id){var d=await db.DatBans.AsNoTracking().FirstOrDefaultAsync(x=>x.MaDatBan==id&&x.MaKhachHang==UserId);if(d?.MaCheckIn==null)return NotFound();using var data=QRCodeGenerator.GenerateQrCode(d.MaCheckIn,QRCodeGenerator.ECCLevel.Q);using var qr=new PngByteQRCode(data);return File(qr.GetGraphic(8),"image/png");}
 public async Task<IActionResult> LichSu()=>View(await db.DatBans.AsNoTracking().Include(x=>x.MaKhachHangNavigation).Include(x=>x.MaBanNavigation).Where(x=>x.MaKhachHang==UserId).OrderByDescending(x=>x.NgayTao).ToListAsync());
 [HttpPost] public async Task<IActionResult> HuyDon(int id){try{await workflow.Cancel(id,UserId,"Khách hàng chủ động hủy");TempData["ThongBao"]="Đã hủy đơn; tiền cọc giả lập (nếu có) được hoàn.";}catch(WorkflowException ex){TempData["Loi"]=ex.Message;}return RedirectToAction(nameof(LichSu));}
 public IActionResult ThanhCong()=>RedirectToAction(nameof(LichSu));
}
