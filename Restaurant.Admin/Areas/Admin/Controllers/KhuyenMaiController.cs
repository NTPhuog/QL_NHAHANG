using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
namespace RestaurantManagement.Areas.Admin.Controllers;
[Area("Admin")]
public class KhuyenMaiController(QuanLyNhaHangContext db, ILogger<KhuyenMaiController> logger) : Controller
{
    public async Task<IActionResult> Index()=>View(await db.KhuyenMais.AsNoTracking().Include(x=>x.MonApDung).ThenInclude(x=>x.MonAn).OrderByDescending(x=>x.NgayBatDau).ToListAsync());
    public async Task<IActionResult> Details(int id){var item=await db.KhuyenMais.AsNoTracking().Include(x=>x.MonApDung).ThenInclude(x=>x.MonAn).FirstOrDefaultAsync(x=>x.MaKhuyenMai==id);return item==null?NotFound():View(item);}
    private async Task Choices(IEnumerable<int>? ids=null){ViewBag.MonAn=await db.MonAns.AsNoTracking().OrderBy(x=>x.TenMon).ToListAsync();ViewBag.SelectedIds=(ids??Array.Empty<int>()).ToHashSet();}
    private async Task<int[]> Validate(KhuyenMai item,int[]? maMonApDung){
        var ids=(maMonApDung??Array.Empty<int>()).Distinct().ToArray();
        await Choices(ids);
        if(item.NgayBatDau==default)ModelState.AddModelError("NgayBatDau","Vui lòng chọn ngày bắt đầu.");
        if(item.NgayKetThuc==default||item.NgayKetThuc<item.NgayBatDau)ModelState.AddModelError("NgayKetThuc","Ngày kết thúc phải từ ngày bắt đầu trở đi.");
        if(ids.Length==0)ModelState.AddModelError("maMonApDung","Vui lòng chọn ít nhất một món áp dụng khuyến mãi.");
        else if(await db.MonAns.CountAsync(x=>ids.Contains(x.MaMon))!=ids.Length)ModelState.AddModelError("maMonApDung","Có món không còn tồn tại. Vui lòng chọn lại.");
        return ids;
    }
    [HttpGet] public async Task<IActionResult> Create(){await Choices();var today=DateOnly.FromDateTime(BookingRules.Now);return View(new KhuyenMai{NgayBatDau=today,NgayKetThuc=today.AddDays(7),PhanTramGiam=10,TrangThai="Hoạt động"});}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Create([Bind("TenKhuyenMai,MoTa,NgayBatDau,NgayKetThuc,PhanTramGiam,TrangThai")] KhuyenMai item,int[]? maMonApDung){
        var ids=await Validate(item,maMonApDung);if(!ModelState.IsValid)return View(item);
        item.TenKhuyenMai=item.TenKhuyenMai.Trim();foreach(var id in ids)item.MonApDung.Add(new KhuyenMaiMonAn{MaMon=id});db.Add(item);
        try{await db.SaveChangesAsync();TempData["ThongBao"]="Đã thêm khuyến mãi và các món áp dụng.";return RedirectToAction(nameof(Index));}
        catch(DbUpdateException ex){logger.LogError(ex,"Create promotion failed");ModelState.AddModelError("","Chưa lưu được khuyến mãi. Vui lòng thử lại.");return View(item);}
    }
    [HttpGet] public async Task<IActionResult> Edit(int id){var item=await db.KhuyenMais.Include(x=>x.MonApDung).FirstOrDefaultAsync(x=>x.MaKhuyenMai==id);if(item==null)return NotFound();await Choices(item.MonApDung.Select(x=>x.MaMon));return View(item);}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Edit(int id,[Bind("MaKhuyenMai,TenKhuyenMai,MoTa,NgayBatDau,NgayKetThuc,PhanTramGiam,TrangThai")] KhuyenMai item,int[]? maMonApDung){
        if(id!=item.MaKhuyenMai)return NotFound();var ids=await Validate(item,maMonApDung);if(!ModelState.IsValid)return View(item);
        var saved=await db.KhuyenMais.Include(x=>x.MonApDung).FirstOrDefaultAsync(x=>x.MaKhuyenMai==id);if(saved==null)return NotFound();
        saved.TenKhuyenMai=item.TenKhuyenMai.Trim();saved.MoTa=item.MoTa;saved.NgayBatDau=item.NgayBatDau;saved.NgayKetThuc=item.NgayKetThuc;saved.PhanTramGiam=item.PhanTramGiam;saved.TrangThai=item.TrangThai;
        db.KhuyenMaiMonAns.RemoveRange(saved.MonApDung.Where(x=>!ids.Contains(x.MaMon)));
        foreach(var dishId in ids.Except(saved.MonApDung.Select(x=>x.MaMon)).ToArray())saved.MonApDung.Add(new KhuyenMaiMonAn{MaMon=dishId});
        try{await db.SaveChangesAsync();TempData["ThongBao"]="Đã cập nhật khuyến mãi và các món áp dụng.";return RedirectToAction(nameof(Index));}
        catch(DbUpdateException ex){logger.LogError(ex,"Edit promotion failed");ModelState.AddModelError("","Chưa lưu được thay đổi. Vui lòng thử lại.");return View(item);}
    }
    [HttpGet] public async Task<IActionResult> Delete(int id){var item=await db.KhuyenMais.FindAsync(id);return item==null?NotFound():View(item);}
    [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken] public async Task<IActionResult> DeleteConfirmed(int id){var item=await db.KhuyenMais.FindAsync(id);if(item==null)return NotFound();db.Remove(item);try{await db.SaveChangesAsync();TempData["ThongBao"]="Đã xóa khuyến mãi.";}catch(DbUpdateException ex){logger.LogError(ex,"Delete promotion failed");TempData["Loi"]="Không xóa được khuyến mãi. Vui lòng thử lại.";}return RedirectToAction(nameof(Index));}
}
