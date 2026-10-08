using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
namespace RestaurantManagement.Areas.Admin.Controllers;
[Area("Admin")]
public class BanAnController(QuanLyNhaHangContext db, ILogger<BanAnController> logger) : Controller
{
    public async Task<IActionResult> Index()=>View(await db.BanAns.AsNoTracking().OrderBy(x=>x.TenBan).ToListAsync());
    public async Task<IActionResult> Details(int id){var b=await db.BanAns.FindAsync(id);return b==null?NotFound():View(b);}
    [HttpGet] public IActionResult Create()=>View(new BanAn{SoChoNgoi=4,TrangThai="Trống"});
    [HttpPost] public async Task<IActionResult> Create([Bind("TenBan,SoChoNgoi,TrangThai")] BanAn b)
    {
        if(!ModelState.IsValid)return View(b);
        b.TenBan=b.TenBan.Trim();db.Add(b);
        try{await db.SaveChangesAsync();TempData["ThongBao"]="Đã thêm bàn.";return RedirectToAction(nameof(Index));}
        catch(DbUpdateException ex){logger.LogError(ex,"Create table failed");ModelState.AddModelError("","Không lưu được bàn. Vui lòng thử lại.");return View(b);}
    }
    [HttpGet] public async Task<IActionResult> Edit(int id){var b=await db.BanAns.FindAsync(id);return b==null?NotFound():View(b);}
    [HttpPost] public async Task<IActionResult> Edit(int id,[Bind("MaBan,TenBan,SoChoNgoi,TrangThai")] BanAn b)
    {
        if(id!=b.MaBan)return NotFound();if(!ModelState.IsValid)return View(b);
        try {
            await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var current=await db.BanAns.FromSqlInterpolated($"SELECT * FROM BanAn WITH (UPDLOCK,HOLDLOCK) WHERE MaBan={id}").SingleOrDefaultAsync();
            if(current==null)return NotFound();
            var bookings=await db.DatBans.Where(x=>x.MaBan==id && (x.TrangThai=="Chờ xác nhận"||x.TrangThai=="Đã duyệt"||x.TrangThai=="Đang phục vụ")).ToListAsync();
            var active=bookings.Where(x=>BookingRules.BlocksTableChange(x,BookingRules.Now)).ToList();
            if(active.Any(x=>x.SoLuongKhach>b.SoChoNgoi))ModelState.AddModelError("SoChoNgoi","Sức chứa mới không đủ cho các đơn còn hiệu lực.");
            if(b.TrangThai!="Trống" && active.Any())ModelState.AddModelError("TrangThai","Bàn còn đơn hiệu lực. Hãy xử lý các đơn trước khi ngừng phục vụ.");
            if(!ModelState.IsValid)return View(b);
            current.TenBan=b.TenBan.Trim();current.SoChoNgoi=b.SoChoNgoi;current.TrangThai=b.TrangThai;
            await db.SaveChangesAsync();await tx.CommitAsync();TempData["ThongBao"]="Đã cập nhật bàn.";return RedirectToAction(nameof(Index));
        }catch(Exception ex)when(ex is DbUpdateException or Microsoft.Data.SqlClient.SqlException){logger.LogError(ex,"Update table failed");ModelState.AddModelError("","Không lưu được thay đổi. Vui lòng thử lại.");return View(b);}
    }
    [HttpGet] public async Task<IActionResult> Delete(int id){var b=await db.BanAns.FindAsync(id);return b==null?NotFound():View(b);}
    [HttpPost,ActionName("Delete")] public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try {
            await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var b=await db.BanAns.FromSqlInterpolated($"SELECT * FROM BanAn WITH (UPDLOCK,HOLDLOCK) WHERE MaBan={id}").SingleOrDefaultAsync();
            if(b==null)return NotFound();
            if(await db.DatBans.AnyAsync(x=>x.MaBan==id)){TempData["Loi"]="Bàn có lịch sử đặt nên không thể xóa.";return RedirectToAction(nameof(Index));}
            db.Remove(b);await db.SaveChangesAsync();await tx.CommitAsync();TempData["ThongBao"]="Đã xóa bàn.";
        }catch(Exception ex)when(ex is DbUpdateException or Microsoft.Data.SqlClient.SqlException){logger.LogError(ex,"Delete table failed");TempData["Loi"]="Không xóa được bàn. Có thể dữ liệu đã thay đổi; hãy tải lại.";}
        return RedirectToAction(nameof(Index));
    }
}

