using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
namespace RestaurantManagement.Controllers;
public class DatBanController(QuanLyNhaHangContext db, ILogger<DatBanController> logger) : Controller
{
 public IActionResult Index()=>RedirectToAction(nameof(LichSu));
 public async Task<IActionResult> Details(int id){var u=HttpContext.Session.GetInt32("MaKhachHang");var d=await db.DatBans.Include(x=>x.MaBanNavigation).Include(x=>x.MaKhachHangNavigation).FirstOrDefaultAsync(x=>x.MaDatBan==id && x.MaKhachHang==u);return d==null?NotFound():View(d);}
 [HttpGet] public async Task<IActionResult> Create(int? maBan){var b=await db.BanAns.FindAsync(maBan);if(b==null)return RedirectToAction("Index","BanAn");ViewBag.TenBan=b.TenBan;ViewBag.SoChoNgoi=b.SoChoNgoi;return View(new DatBan{MaBan=b.MaBan,NgayDat=DateOnly.FromDateTime(Now.AddDays(1)),GioDat=new TimeOnly(18,0),SoLuongKhach=1});}
 private static DateTime Now=>TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
 [HttpPost] public async Task<IActionResult> Create([Bind("MaBan,NgayDat,GioDat,SoLuongKhach,GhiChu")] DatBan d)
 {
  ModelState.Remove(nameof(d.MaBanNavigation));ModelState.Remove(nameof(d.MaKhachHangNavigation));
  var b=await db.BanAns.FindAsync(d.MaBan);if(b==null)return NotFound();
  ViewBag.TenBan=b.TenBan;ViewBag.SoChoNgoi=b.SoChoNgoi;
  if(d.NgayDat.ToDateTime(d.GioDat)<=Now)ModelState.AddModelError("NgayDat","Vui lòng chọn thời gian trong tương lai.");
  if(d.GioDat<new TimeOnly(10,0)||d.GioDat>new TimeOnly(20,0))ModelState.AddModelError("GioDat","Nhận đặt bàn từ 10:00 đến 20:00, mỗi lượt 2 giờ.");
  if(d.SoLuongKhach<1||d.SoLuongKhach>b.SoChoNgoi)ModelState.AddModelError("SoLuongKhach",$"Số khách phải từ 1 đến {b.SoChoNgoi}.");
  if(d.GhiChu?.Length>255)ModelState.AddModelError("GhiChu","Ghi chú tối đa 255 ký tự.");
  if(b.TrangThai!="Trống")ModelState.AddModelError("","Bàn hiện không nhận đặt chỗ.");
  if(!ModelState.IsValid)return View(d);
  try {
   await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
   // Serialize bookings per table, including concurrent requests for an empty slot.
   var locked=await db.BanAns.FromSqlInterpolated($"SELECT * FROM BanAn WITH (UPDLOCK,HOLDLOCK) WHERE MaBan={d.MaBan}").AsNoTracking().SingleAsync();
   if(locked.TrangThai!="Trống"||d.SoLuongKhach>locked.SoChoNgoi){ModelState.AddModelError("","Bàn vừa thay đổi trạng thái hoặc sức chứa. Hãy chọn lại.");return View(d);}
   var orders=await db.DatBans.Where(x=>x.MaBan==d.MaBan && x.NgayDat==d.NgayDat && x.TrangThai!="Đã hủy").ToListAsync();
   if(orders.Any(x=>Math.Abs((x.GioDat.ToTimeSpan()-d.GioDat.ToTimeSpan()).TotalMinutes)<120)){ModelState.AddModelError("GioDat","Bàn đã có lịch trong khoảng 2 giờ này. Hãy chọn giờ hoặc bàn khác.");return View(d);}
   d.MaKhachHang=HttpContext.Session.GetInt32("MaKhachHang")!.Value;d.TrangThai="Chờ xác nhận";d.NgayTao=Now;db.DatBans.Add(d);await db.SaveChangesAsync();await tx.CommitAsync();return RedirectToAction(nameof(ThanhCong));
  }catch(Exception ex)when(ex is DbUpdateException or Microsoft.Data.SqlClient.SqlException){logger.LogError(ex,"Booking failed");ModelState.AddModelError("","Không thể lưu đơn. Vui lòng thử lại.");return View(d);}
 }
 public async Task<IActionResult> LichSu(){var u=HttpContext.Session.GetInt32("MaKhachHang");return View(await db.DatBans.Include(x=>x.MaBanNavigation).Where(x=>x.MaKhachHang==u).OrderByDescending(x=>x.NgayTao).ToListAsync());}
 [HttpPost] public async Task<IActionResult> HuyDon(int id){var u=HttpContext.Session.GetInt32("MaKhachHang");var count=await db.DatBans.Where(x=>x.MaDatBan==id&&x.MaKhachHang==u&&x.TrangThai=="Chờ xác nhận").ExecuteUpdateAsync(s=>s.SetProperty(x=>x.TrangThai,"Đã hủy").SetProperty(x=>x.LyDoHuy,"Khách hàng chủ động hủy"));TempData[count==1?"ThongBao":"Loi"]=count==1?"Đã hủy đơn.":"Không thể hủy đơn này; hãy tải lại lịch sử.";return RedirectToAction(nameof(LichSu));}
 public IActionResult ThanhCong()=>View();
}

