using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;

namespace RestaurantManagement.Services;

public static class BookingNotices
{
    public static string? Message(DatBan order, DateTime now)
    {
        if (order.CheckInUtc != null || order.TrangThai is not ("Đã duyệt" or "Chờ xác nhận")) return null;
        var start = order.NgayDat.ToDateTime(order.GioDat);
        if (now >= start.AddHours(2) || now < start.AddHours(-2)) return null;
        return now >= start ? "Đã đến giờ hẹn · Chưa check-in" : "Sắp đến giờ hẹn · Trong 2 tiếng tới";
    }

    public static bool Due(DatBan order, DateTime now) => Message(order, now) != null && now >= order.NgayDat.ToDateTime(order.GioDat);

    public static async Task<object[]> ReadAsync(QuanLyNhaHangContext db, int? owner = null)
    {
        var now = BookingRules.Now;
        var first = DateOnly.FromDateTime(now.AddHours(-2));
        var last = DateOnly.FromDateTime(now.AddHours(2));
        var query = db.DatBans.AsNoTracking().Include(x => x.MaBanNavigation).Include(x => x.MaKhachHangNavigation)
            .Where(x => x.CheckInUtc == null && (x.TrangThai == "Đã duyệt" || x.TrangThai == "Chờ xác nhận") && x.NgayDat >= first && x.NgayDat <= last);
        if (owner.HasValue) query = query.Where(x => x.MaKhachHang == owner.Value);
        var orders = await query.ToListAsync();
        return orders.Where(x => Message(x, now) != null).OrderByDescending(x => Due(x, now))
            .ThenBy(x => x.NgayDat).ThenBy(x => x.GioDat).Select(x => (object)new {
                id = x.MaDatBan, due = Due(x, now), message = Message(x, now),
                table = x.MaBanNavigation.TenBan, time = x.GioDat.ToString("HH:mm"), date = x.NgayDat.ToString("dd/MM/yyyy"),
                customer = x.MaKhachHangNavigation.HoTen, phone = x.MaKhachHangNavigation.SoDienThoai,
                url = owner.HasValue ? $"/DatBan/Details/{x.MaDatBan}" : "/NhanVien/DatBan"
            }).ToArray();
    }
}
