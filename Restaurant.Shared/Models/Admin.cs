using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class Admin
{
    public int MaAdmin { get; set; }

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? TrangThai { get; set; }
}
