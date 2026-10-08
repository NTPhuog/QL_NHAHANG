using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class BanAn
{
    public int MaBan { get; set; }

    [Required, StringLength(50)]
    public string TenBan { get; set; } = null!;

    [Range(1, 100)]
    public int SoChoNgoi { get; set; }

    [Required, RegularExpression("^(Trống|Bảo trì|Ngừng phục vụ)$")]
    public string? TrangThai { get; set; }

    public virtual ICollection<DatBan> DatBans { get; set; } = new List<DatBan>();
}

