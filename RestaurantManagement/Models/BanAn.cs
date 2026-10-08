using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class BanAn
{
    public int MaBan { get; set; }

    public string TenBan { get; set; } = null!;

    public int SoChoNgoi { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<DatBan> DatBans { get; set; } = new List<DatBan>();
}
