using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
namespace RestaurantManagement.Areas.Admin.Controllers;
[Area("Admin")]
public class DatBanController(QuanLyNhaHangContext db, ILogger<DatBanController> logger, RestaurantManagement.Services.BookingWorkflow workflow) : Controller
{
    public async Task<IActionResult> Index(DateOnly? ngay, string? trangThai, string? q)
    {
        if (!ModelState.IsValid) TempData["Loi"] = "Ngày lọc không hợp lệ. Vui lòng chọn lại.";
        var list = db.DatBans.AsNoTracking().Include(x => x.MaBanNavigation).Include(x => x.MaKhachHangNavigation).AsQueryable();
        if (ngay.HasValue) list = list.Where(x => x.NgayDat == ngay);
        if (!string.IsNullOrWhiteSpace(trangThai)) list = list.Where(x => x.TrangThai == trangThai);
        if (!string.IsNullOrWhiteSpace(q)) { q=q.Trim(); list = list.Where(x => x.MaKhachHangNavigation.HoTen.Contains(q) || x.MaKhachHangNavigation.SoDienThoai!.Contains(q)); }
        return View(await list.OrderBy(x => x.NgayDat).ThenBy(x => x.GioDat).ToListAsync());
    }
    private IActionResult Result(bool success, string message) { TempData[success ? "ThongBao" : "Loi"] = message; return RedirectToAction(nameof(Index)); }
    private async Task<IActionResult> Change(int id, string from, string to, string? reason = null)
    {
        try {
            var count = await db.DatBans.Where(x => x.MaDatBan == id && x.TrangThai == from).ExecuteUpdateAsync(s => s.SetProperty(x => x.TrangThai, to).SetProperty(x => x.LyDoHuy, reason));
            return Result(count == 1, count == 1 ? "Đã cập nhật đơn." : "Trạng thái đơn đã thay đổi. Vui lòng kiểm tra lại.");
        } catch (Microsoft.Data.SqlClient.SqlException ex) { logger.LogError(ex,"Failed booking transition"); return Result(false,"Chưa lưu được trạng thái đơn. Vui lòng thử lại hoặc kiểm tra cấu hình dữ liệu."); }
    }
    [HttpPost] public async Task<IActionResult> Duyet(int id)
    {
        var now=BookingRules.Now; var day=DateOnly.FromDateTime(now);var time=TimeOnly.FromDateTime(now);
        try {
            var count=await db.DatBans.Where(x=>x.MaDatBan==id && x.TrangThai=="Chờ xác nhận" && (x.NgayDat>day || x.NgayDat==day && x.GioDat>time)).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.TrangThai,"Đã duyệt").SetProperty(x=>x.LyDoHuy,(string?)null));
            return Result(count==1,count==1?"Đã duyệt đơn.":"Không thể duyệt: đơn đã quá giờ hoặc đã đổi trạng thái.");
        } catch(Microsoft.Data.SqlClient.SqlException ex) {logger.LogError(ex,"Approval failed");return Result(false,"Không lưu được đơn. Vui lòng thử lại.");}
    }
    [HttpPost] public async Task<IActionResult> NhanBan(int id){
        try{await workflow.CheckInOrder(id);return Result(true,"Đã nhận bàn.");}
        catch(RestaurantManagement.Services.WorkflowException ex){return Result(false,ex.Message);}
        catch(Microsoft.Data.SqlClient.SqlException ex){logger.LogError(ex,"Check-in failed");return Result(false,"Không nhận bàn được. Vui lòng thử lại.");}
    }    [HttpPost] public IActionResult HoanThanh(int id)=>Result(false,"Hoàn tất đơn tại trang nhân viên bằng chức năng tính tiền và xuất bill.");
    [HttpPost] public async Task<IActionResult> Huy(int id,string lyDoHuy){if(string.IsNullOrWhiteSpace(lyDoHuy)||lyDoHuy.Length>255)return Result(false,"Nhập lý do hủy từ 1–255 ký tự.");try{await workflow.Cancel(id,null,lyDoHuy.Trim());return Result(true,"Đã hủy đơn và hoàn cọc giả lập nếu có.");}catch(RestaurantManagement.Services.WorkflowException ex){return Result(false,ex.Message);}}
}
