using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;

namespace RestaurantManagement.Controllers
{
    public class DatBanController : Controller
    {
        private readonly QuanLyNhaHangContext _context;

        public DatBanController(QuanLyNhaHangContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var danhSachDatBan = _context.DatBans
                .Include(d => d.MaBanNavigation)
                .Include(d => d.MaKhachHangNavigation);

            return View(await danhSachDatBan.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var datBan = await _context.DatBans
                .Include(d => d.MaBanNavigation)
                .Include(d => d.MaKhachHangNavigation)
                .FirstOrDefaultAsync(m => m.MaDatBan == id);

            if (datBan == null) return NotFound();

            return View(datBan);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? maBan)
        {
            if (maBan == null)
                return RedirectToAction("Index", "BanAn");

            var ban = await _context.BanAns
                .FirstOrDefaultAsync(b => b.MaBan == maBan.Value);

            if (ban == null) return NotFound();

            ViewBag.TenBan = ban.TenBan;
            ViewBag.SoChoNgoi = ban.SoChoNgoi;
            ViewBag.TrangThaiBan = ban.TrangThai;

            return View(new DatBan { MaBan = ban.MaBan });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaBan,NgayDat,GioDat,SoLuongKhach,GhiChu")] DatBan datBan)
        {
            var maKhachHang = HttpContext.Session.GetInt32("MaKhachHang");

            if (maKhachHang == null)
            {
                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan",
                    new
                    {
                        returnUrl = Url.Action(
                            "Create",
                            "DatBan",
                            new { maBan = datBan.MaBan })
                    });
            }

            var ban = await _context.BanAns
                .FirstOrDefaultAsync(b => b.MaBan == datBan.MaBan);

            if (ban == null) return NotFound();

            datBan.MaKhachHang = maKhachHang.Value;
            datBan.TrangThai = "Chờ xác nhận";
            datBan.NgayTao = DateTime.Now;
            datBan.LyDoHuy = null;

            ModelState.Remove(nameof(DatBan.MaKhachHang));
            ModelState.Remove(nameof(DatBan.TrangThai));
            ModelState.Remove(nameof(DatBan.NgayTao));
            ModelState.Remove(nameof(DatBan.LyDoHuy));
            ModelState.Remove(nameof(DatBan.MaBanNavigation));
            ModelState.Remove(nameof(DatBan.MaKhachHangNavigation));

            if (datBan.SoLuongKhach <= 0)
            {
                ModelState.AddModelError(
                    nameof(DatBan.SoLuongKhach),
                    "Số lượng khách phải lớn hơn 0.");
            }

            if (datBan.SoLuongKhach > ban.SoChoNgoi)
            {
                ModelState.AddModelError(
                    nameof(DatBan.SoLuongKhach),
                    $"Bàn này chỉ có tối đa {ban.SoChoNgoi} người.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.TenBan = ban.TenBan;
                ViewBag.SoChoNgoi = ban.SoChoNgoi;
                ViewBag.TrangThaiBan = ban.TrangThai;

                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine(
                            $"MODEL ERROR [{state.Key}]: {error.ErrorMessage} {error.Exception?.Message}");
                    }
                }

                return View(datBan);
            }

            try
            {
                _context.DatBans.Add(datBan);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ThanhCong));
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== LỖI LƯU ĐẶT BÀN =====");
                Console.WriteLine(ex.ToString());

                ViewBag.TenBan = ban.TenBan;
                ViewBag.SoChoNgoi = ban.SoChoNgoi;
                ViewBag.TrangThaiBan = ban.TrangThai;

                ModelState.AddModelError(
                    string.Empty,
                    "Không thể lưu đơn đặt bàn. Hãy xem lỗi trong Terminal.");

                return View(datBan);
            }
        }

        public IActionResult ThanhCong()
        {
            return View();
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var datBan = await _context.DatBans.FindAsync(id);

            if (datBan == null) return NotFound();

            ViewData["MaBan"] = new SelectList(
                _context.BanAns, "MaBan", "MaBan", datBan.MaBan);

            ViewData["MaKhachHang"] = new SelectList(
                _context.KhachHangs, "MaKhachHang", "MaKhachHang", datBan.MaKhachHang);

            return View(datBan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaDatBan,MaKhachHang,MaBan,NgayDat,GioDat,SoLuongKhach,GhiChu,TrangThai,LyDoHuy,NgayTao")]
            DatBan datBan)
        {
            if (id != datBan.MaDatBan) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(datBan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DatBanExists(datBan.MaDatBan))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["MaBan"] = new SelectList(
                _context.BanAns, "MaBan", "MaBan", datBan.MaBan);

            ViewData["MaKhachHang"] = new SelectList(
                _context.KhachHangs, "MaKhachHang", "MaKhachHang", datBan.MaKhachHang);

            return View(datBan);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var datBan = await _context.DatBans
                .Include(d => d.MaBanNavigation)
                .Include(d => d.MaKhachHangNavigation)
                .FirstOrDefaultAsync(m => m.MaDatBan == id);

            if (datBan == null) return NotFound();

            return View(datBan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var datBan = await _context.DatBans.FindAsync(id);

            if (datBan != null)
                _context.DatBans.Remove(datBan);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool DatBanExists(int id)
        {
            return _context.DatBans.Any(e => e.MaDatBan == id);
        }
    }
}
