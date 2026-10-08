using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
namespace RestaurantManagement.Areas.Admin.Controllers;
[Area("Admin")]
public class KhuyenMaiController(QuanLyNhaHangContext db, ILogger<KhuyenMaiController> logger) : Controller
{
    public async Task<IActionResult> Index() => View(await db.KhuyenMais.AsNoTracking().OrderByDescending(x=>x.NgayBatDau).ToListAsync());
    public async Task<IActionResult> Details(int id) {var item=await db.KhuyenMais.FindAsync(id);return item==null?NotFound():View(item);}
    [HttpGet] public IActionResult Create() {var today=DateOnly.FromDateTime(BookingRules.Now);return View(new KhuyenMai{NgayBatDau=today,NgayKetThuc=today.AddDays(7),PhanTramGiam=10,TrangThai="Hoạt động"});}
    private void Validate(KhuyenMai item) {
        if(item.NgayBatDau==default)ModelState.AddModelError("NgayBatDau","Vui lòng chọn ngày bắt đầu.");
        if(item.NgayKetThuc==default || item.NgayKetThuc<item.NgayBatDau)ModelState.AddModelError("NgayKetThuc","Ngày kết thúc phải từ ngày bắt đầu trở đi.");
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Create([Bind("TenKhuyenMai,MoTa,NgayBatDau,NgayKetThuc,PhanTramGiam,TrangThai")] KhuyenMai item) {
        Validate(item);if(!ModelState.IsValid)return View(item);
        item.MaKhuyenMai=0;item.TenKhuyenMai=item.TenKhuyenMai.Trim();db.Add(item);
        try{await db.SaveChangesAsync();TempData["ThongBao"]="Đã thêm khuyến mãi.";return RedirectToAction(nameof(Index));}
        catch(DbUpdateException ex){logger.LogError(ex,"Create promotion failed");ModelState.AddModelError("","Chưa lưu được khuyến mãi. Vui lòng kiểm tra thông tin và thử lại.");return View(item);}
    }
    [HttpGet] public async Task<IActionResult> Edit(int id){var item=await db.KhuyenMais.FindAsync(id);return item==null?NotFound():View(item);}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Edit(int id,[Bind("MaKhuyenMai,TenKhuyenMai,MoTa,NgayBatDau,NgayKetThuc,PhanTramGiam,TrangThai")] KhuyenMai item){
        if(id!=item.MaKhuyenMai)return NotFound();Validate(item);if(!ModelState.IsValid)return View(item);
        var saved=await db.KhuyenMais.FindAsync(id);if(saved==null)return NotFound();
        saved.TenKhuyenMai=item.TenKhuyenMai.Trim();saved.MoTa=item.MoTa;saved.NgayBatDau=item.NgayBatDau;saved.NgayKetThuc=item.NgayKetThuc;saved.PhanTramGiam=item.PhanTramGiam;saved.TrangThai=item.TrangThai;
        try{await db.SaveChangesAsync();TempData["ThongBao"]="Đã cập nhật khuyến mãi.";return RedirectToAction(nameof(Index));}
        catch(DbUpdateException ex){logger.LogError(ex,"Edit promotion failed");ModelState.AddModelError("","Chưa lưu được thay đổi. Vui lòng thử lại.");return View(item);}
    }
    [HttpGet] public async Task<IActionResult> Delete(int id){var item=await db.KhuyenMais.FindAsync(id);return item==null?NotFound():View(item);}
    [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken] public async Task<IActionResult> DeleteConfirmed(int id){var item=await db.KhuyenMais.FindAsync(id);if(item==null)return NotFound();db.Remove(item);try{await db.SaveChangesAsync();TempData["ThongBao"]="Đã xóa khuyến mãi.";}catch(DbUpdateException ex){logger.LogError(ex,"Delete promotion failed");TempData["Loi"]="Không xóa được khuyến mãi. Vui lòng thử lại.";}return RedirectToAction(nameof(Index));}
}
