namespace RestaurantManagement.Security;
public static class AccessPolicy
{
    public static string? RequiredRole(string? area, string? controller, string? action)
    {
        if (!string.IsNullOrEmpty(area))
        {
            if (area.Equals("NhanVien", StringComparison.OrdinalIgnoreCase) ||
                area.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                return "Staff";
            }

            return "Admin";
        }

        return controller?.ToLowerInvariant() switch
        {
            "home" or "taikhoan" => null,
            "datban" => "KhachHang",
            "banan" or "monan" or "khuyenmai" when action?.ToLowerInvariant() is "index" or "details" => null,
            _ => "Admin"
        };
    }

    public static bool Allows(string? required, string? role)
    {
        if (required is null) return true;

        var requiredRole = required.Trim();
        var currentRole = role?.Trim();
        if (string.IsNullOrEmpty(currentRole)) return false;

        if (requiredRole.Equals("Staff", StringComparison.OrdinalIgnoreCase) ||
            requiredRole.Equals("NhanVien", StringComparison.OrdinalIgnoreCase))
        {
            return currentRole.Equals("Staff", StringComparison.OrdinalIgnoreCase) ||
                   currentRole.Equals("NhanVien", StringComparison.OrdinalIgnoreCase) ||
                   currentRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        if (requiredRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return currentRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        if (requiredRole.Equals("KhachHang", StringComparison.OrdinalIgnoreCase))
        {
            return currentRole.Equals("KhachHang", StringComparison.OrdinalIgnoreCase);
        }

        return requiredRole.Equals(currentRole, StringComparison.OrdinalIgnoreCase);
    }
}

