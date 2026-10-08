using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class DatBan
{
    public int MaDatBan { get; set; }

    public int MaKhachHang { get; set; }

    public int MaBan { get; set; }

    public DateOnly NgayDat { get; set; }

    public TimeOnly GioDat { get; set; }

    public int SoLuongKhach { get; set; }

    public string? GhiChu { get; set; }

    public string? TrangThai { get; set; }

    public string? LyDoHuy { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual BanAn MaBanNavigation { get; set; } = null!;

    public virtual KhachHang MaKhachHangNavigation { get; set; } = null!;
}
