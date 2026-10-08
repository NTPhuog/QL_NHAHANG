using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class KhuyenMai
{
    public int MaKhuyenMai { get; set; }

    public string TenKhuyenMai { get; set; } = null!;

    public string? MoTa { get; set; }

    public DateOnly NgayBatDau { get; set; }

    public DateOnly NgayKetThuc { get; set; }

    public decimal? PhanTramGiam { get; set; }

    public string? TrangThai { get; set; }
}
