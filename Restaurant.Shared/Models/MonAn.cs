using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class MonAn
{
    public int MaMon { get; set; }

    public int MaDanhMuc { get; set; }

    [Required, StringLength(100)]
    public string TenMon { get; set; } = null!;

    [StringLength(255)]
    public string? MoTa { get; set; }

    [Range(typeof(decimal), "0.01", "999999999", ParseLimitsInInvariantCulture = true) ]
    public decimal Gia { get; set; }

    [StringLength(255)]
    public string? HinhAnh { get; set; }

    [Required, RegularExpression("^(Hiển thị|Ẩn|Hết món)$")]
    public string? TrangThai { get; set; }

    public virtual DanhMucMon MaDanhMucNavigation { get; set; } = null!;
}


