using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class DanhMucMon
{
    public int MaDanhMuc { get; set; }

    public string TenDanhMuc { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<MonAn> MonAns { get; set; } = new List<MonAn>();
}
