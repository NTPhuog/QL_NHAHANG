using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class KhachHang
{
    public int MaKhachHang { get; set; }

    public string HoTen { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string Email { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? DiaChi { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    public virtual ICollection<DatBan> DatBans { get; set; } = new List<DatBan>();
}
