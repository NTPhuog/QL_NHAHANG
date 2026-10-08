namespace RestaurantManagement.Security;
public static class AccessPolicy
{
    public static string? RequiredRole(string? area, string? controller, string? action)
    {
        if (!string.IsNullOrEmpty(area)) return area.Equals("NhanVien", StringComparison.OrdinalIgnoreCase) ? "NhanVien" : "Admin";
        return controller?.ToLowerInvariant() switch
        {
            "home" or "taikhoan" => null,
            "datban" => "KhachHang",
            "banan" or "monan" or "khuyenmai" when action?.ToLowerInvariant() is "index" or "details" => null,
            _ => "Admin"
        };
    }
    public static bool Allows(string? required, string? role) => required == null || required == role;
}

