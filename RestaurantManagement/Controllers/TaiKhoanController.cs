using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;

namespace RestaurantManagement.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly QuanLyNhaHangContext _context;

        public TaiKhoanController(QuanLyNhaHangContext context)
        {
            _context = context;
        }

        // =========================
        // ĐĂNG KÝ - GET
        // =========================
        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

        // =========================
        // ĐĂNG KÝ - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(
            string hoTen,
            string soDienThoai,
            string email,
            string matKhau,
            string nhapLaiMatKhau,
            string? diaChi)
        {
            if (string.IsNullOrWhiteSpace(hoTen) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(matKhau))
            {
                ViewBag.Loi = "Vui lòng nhập đầy đủ thông tin bắt buộc.";
                return View();
            }

            if (matKhau != nhapLaiMatKhau)
            {
                ViewBag.Loi = "Mật khẩu nhập lại không khớp.";
                return View();
            }

            var emailTonTai = await _context.KhachHangs
                .AnyAsync(k => k.Email == email);

            if (emailTonTai)
            {
                ViewBag.Loi = "Email này đã được sử dụng.";
                return View();
            }

            var khachHang = new KhachHang
            {
                HoTen = hoTen,
                SoDienThoai = soDienThoai,
                Email = email,

                // Tạm thời để chạy demo.
                // Sau này nên hash mật khẩu.
                MatKhau = matKhau,

                DiaChi = diaChi,
                TrangThai = "Hoạt động"
            };

            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            // Đăng ký xong cho đăng nhập luôn
            HttpContext.Session.SetInt32(
                "MaKhachHang",
                khachHang.MaKhachHang
            );

            HttpContext.Session.SetString(
                "HoTen",
                khachHang.HoTen
            );

            return RedirectToAction(
                "Index",
                "Home"
            );
        }

        // =========================
        // ĐĂNG NHẬP - GET
        // =========================
        [HttpGet]
        public IActionResult DangNhap(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        // =========================
        // ĐĂNG NHẬP - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(
            string email,
            string matKhau,
            string? returnUrl = null)
        {
            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k =>
                    k.Email == email &&
                    k.MatKhau == matKhau &&
                    k.TrangThai == "Hoạt động"
                );

            if (khachHang == null)
            {
                ViewBag.Loi =
                    "Email hoặc mật khẩu không chính xác.";

                ViewBag.ReturnUrl = returnUrl;

                return View();
            }

            HttpContext.Session.SetInt32(
                "MaKhachHang",
                khachHang.MaKhachHang
            );

            HttpContext.Session.SetString(
                "HoTen",
                khachHang.HoTen
            );

            if (!string.IsNullOrEmpty(returnUrl)
                && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(
                "Index",
                "Home"
            );
        }

        // =========================
        // ĐĂNG XUẤT
        // =========================
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();

            return RedirectToAction(
                "Index",
                "Home"
            );
        }
    }
}