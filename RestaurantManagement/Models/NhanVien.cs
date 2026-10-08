using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class NhanVien
{
    public int MaNhanVien { get; set; }

    public string HoTen { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string Email { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? ChucVu { get; set; }

    public string? TrangThai { get; set; }
}
