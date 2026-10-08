using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class DanhGium
{
    public int MaDanhGia { get; set; }

    public int MaKhachHang { get; set; }

    public int SoSao { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public string? TrangThai { get; set; }

    public virtual KhachHang MaKhachHangNavigation { get; set; } = null!;
}
