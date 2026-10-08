using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class DanhMucMon
{
    public int MaDanhMuc { get; set; }

    [Required, StringLength(100)]
    public string TenDanhMuc { get; set; } = null!;

    [StringLength(255)]
    public string? MoTa { get; set; }

    public virtual ICollection<MonAn> MonAns { get; set; } = new List<MonAn>();
}

