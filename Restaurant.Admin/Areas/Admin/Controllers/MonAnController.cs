using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
using RestaurantManagement.Hosting;
namespace RestaurantManagement.Areas.Admin.Controllers;
[Area("Admin")]
[RequestSizeLimit(6 * 1024 * 1024)]
public class MonAnController(QuanLyNhaHangContext db, ImageStorage images, ILogger<MonAnController> logger) : Controller
{
    public async Task<IActionResult> Index()=>View(await db.MonAns.AsNoTracking().Include(x=>x.MaDanhMucNavigation).OrderBy(x=>x.MaDanhMucNavigation.TenDanhMuc).ThenBy(x=>x.TenMon).ToListAsync());
    public async Task<IActionResult> Details(int id){var item=await db.MonAns.Include(x=>x.MaDanhMucNavigation).FirstOrDefaultAsync(x=>x.MaMon==id);return item==null?NotFound():View(item);}
    private async Task Choices(int? selected=null)=>ViewData["MaDanhMuc"]=new SelectList(await db.DanhMucMons.AsNoTracking().OrderBy(x=>x.TenDanhMuc).ToListAsync(),"MaDanhMuc","TenDanhMuc",selected);
    private async Task Validate(MonAn item){ModelState.Remove(nameof(item.MaDanhMucNavigation));if(!await db.DanhMucMons.AnyAsync(x=>x.MaDanhMuc==item.MaDanhMuc))ModelState.AddModelError("MaDanhMuc","Vui lòng chọn danh mục có trong danh sách.");}
    [HttpGet] public async Task<IActionResult> Create(){await Choices();return View(new MonAn{TrangThai="Hiển thị"});}
    [HttpPost] public async Task<IActionResult> Create([Bind("MaDanhMuc,TenMon,MoTa,Gia,TrangThai")] MonAn item,IFormFile? anhTaiLen)
    {
        await Choices(item.MaDanhMuc);await Validate(item);if(!ModelState.IsValid)return View(item);
        string? uploaded=null;
        try{uploaded=await images.SaveAsync(anhTaiLen,"mon-an");item.HinhAnh=uploaded;item.TenMon=item.TenMon.Trim();db.MonAns.Add(item);await db.SaveChangesAsync();TempData["ThongBao"]="Đã thêm món ăn.";return RedirectToAction(nameof(Index));}
        catch(InvalidDataException ex){ModelState.AddModelError("anhTaiLen",ex.Message);}
        catch(Exception ex)when(ex is DbUpdateException or IOException or UnauthorizedAccessException){logger.LogError(ex,"Create dish failed");ModelState.AddModelError("","Chưa lưu được món ăn. Kiểm tra thông tin hoặc quyền ghi thư mục ảnh rồi thử lại.");}
        if(uploaded!=null)images.Discard(uploaded);item.HinhAnh=null;return View(item);
    }
    [HttpGet] public async Task<IActionResult> Edit(int id){var item=await db.MonAns.FindAsync(id);if(item==null)return NotFound();await Choices(item.MaDanhMuc);return View(item);}
    [HttpPost] public async Task<IActionResult> Edit(int id,[Bind("MaMon,MaDanhMuc,TenMon,MoTa,Gia,TrangThai")] MonAn item,IFormFile? anhTaiLen)
    {
        if(id!=item.MaMon)return NotFound();var saved=await db.MonAns.FindAsync(id);if(saved==null)return NotFound();
        var oldImage=saved.HinhAnh;item.HinhAnh=oldImage;await Choices(item.MaDanhMuc);await Validate(item);if(!ModelState.IsValid)return View(item);
        string? uploaded=null;
        try{uploaded=await images.SaveAsync(anhTaiLen,"mon-an");saved.TenMon=item.TenMon.Trim();saved.MaDanhMuc=item.MaDanhMuc;saved.MoTa=item.MoTa;saved.Gia=item.Gia;saved.TrangThai=item.TrangThai;saved.HinhAnh=uploaded??oldImage;await db.SaveChangesAsync();TempData["ThongBao"]="Đã cập nhật món ăn.";return RedirectToAction(nameof(Index));}
        catch(InvalidDataException ex){ModelState.AddModelError("anhTaiLen",ex.Message);}
        catch(Exception ex)when(ex is DbUpdateException or IOException or UnauthorizedAccessException){logger.LogError(ex,"Edit dish failed");ModelState.AddModelError("","Chưa lưu được món ăn. Vui lòng thử lại.");}
        if(uploaded!=null)images.Discard(uploaded);return View(item);
    }
    [HttpGet] public async Task<IActionResult> Delete(int id){var item=await db.MonAns.Include(x=>x.MaDanhMucNavigation).FirstOrDefaultAsync(x=>x.MaMon==id);return item==null?NotFound():View(item);}
    [HttpPost,ActionName("Delete")] public async Task<IActionResult> DeleteConfirmed(int id){var item=await db.MonAns.FindAsync(id);if(item==null)return NotFound();db.Remove(item);try{await db.SaveChangesAsync();TempData["ThongBao"]="Đã xóa món ăn.";}catch(DbUpdateException ex){logger.LogError(ex,"Delete dish failed");TempData["Loi"]="Không xóa được món ăn; bạn có thể chuyển sang trạng thái Ẩn.";}return RedirectToAction(nameof(Index));}
}
