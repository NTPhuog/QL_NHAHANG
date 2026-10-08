using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class KhuyenMai
{
    public int MaKhuyenMai { get; set; }

    [Required, StringLength(100)]
    public string TenKhuyenMai { get; set; } = null!;

    [StringLength(255)]
    public string? MoTa { get; set; }

    public DateOnly NgayBatDau { get; set; }

    public DateOnly NgayKetThuc { get; set; }

    [Required, Range(0, 100)]
    public decimal? PhanTramGiam { get; set; }

    [Required, RegularExpression("^(Hoạt động|Ngừng)$")]
    public string? TrangThai { get; set; }
}

