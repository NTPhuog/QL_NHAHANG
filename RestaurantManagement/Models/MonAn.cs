using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class MonAn
{
    public int MaMon { get; set; }

    public int MaDanhMuc { get; set; }

    public string TenMon { get; set; } = null!;

    public string? MoTa { get; set; }

    public decimal Gia { get; set; }

    public string? HinhAnh { get; set; }

    public string? TrangThai { get; set; }

    public virtual DanhMucMon MaDanhMucNavigation { get; set; } = null!;
}
