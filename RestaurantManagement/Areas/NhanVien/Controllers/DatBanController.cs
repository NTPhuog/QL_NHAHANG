using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;

namespace RestaurantManagement.Areas.NhanVien.Controllers
{
    [Area("NhanVien")]
    public class DatBanController : Controller
    {
        private readonly QuanLyNhaHangContext _context;

        public DatBanController(QuanLyNhaHangContext context)
        {
            _context = context;
        }

        // =====================================================
        // DANH SÁCH ĐƠN ĐẶT BÀN
        // /NhanVien/DatBan
        // =====================================================
        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.DatBans

                .Include(d => d.MaKhachHangNavigation)

                .Include(d => d.MaBanNavigation)

                .OrderByDescending(d => d.NgayTao)

                .ToListAsync();

            return View(danhSach);
        }


        // =====================================================
        // DUYỆT ĐƠN
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duyet(int id)
        {
            var datBan = await _context.DatBans
                .FindAsync(id);

            if (datBan == null)
            {
                return NotFound();
            }

            datBan.TrangThai = "Đã duyệt";
            datBan.LyDoHuy = null;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // HỦY ĐƠN
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(
            int id,
            string lyDoHuy)
        {
            var datBan = await _context.DatBans
                .FindAsync(id);

            if (datBan == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(lyDoHuy))
            {
                TempData["Loi"] =
                    "Vui lòng nhập lý do hủy.";

                return RedirectToAction(nameof(Index));
            }

            datBan.TrangThai = "Đã hủy";
            datBan.LyDoHuy = lyDoHuy;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}