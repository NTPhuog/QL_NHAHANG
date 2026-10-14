namespace RestaurantManagement.Models;
public static class BookingRules
{
    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
    public static bool CanReceive(DatBan order, DateTime now) => order.TrangThai == "Đã duyệt" && now < order.NgayDat.ToDateTime(order.GioDat).AddHours(2);
    public static bool BlocksTableChange(DatBan order, DateTime now) => order.TrangThai == "Đang phục vụ" || (order.TrangThai is "Chờ xác nhận" or "Đã duyệt") && order.NgayDat.ToDateTime(order.GioDat).AddHours(2) > now;
}
