using Microsoft.EntityFrameworkCore;

namespace RestaurantManagement.Models;

public class KhuyenMaiMonAn
{
    public int MaKhuyenMai { get; set; }
    public int MaMon { get; set; }
    public KhuyenMai KhuyenMai { get; set; } = null!;
    public MonAn MonAn { get; set; } = null!;
}

public partial class QuanLyNhaHangContext
{
    public DbSet<KhuyenMaiMonAn> KhuyenMaiMonAns { get; set; }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        ConfigureWorkflow(modelBuilder);
        modelBuilder.Entity<KhuyenMaiMonAn>(entity =>
        {
            entity.ToTable("KhuyenMaiMonAn");
            entity.HasKey(x => new { x.MaKhuyenMai, x.MaMon });
            entity.HasOne(x => x.KhuyenMai).WithMany(x => x.MonApDung).HasForeignKey(x => x.MaKhuyenMai).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.MonAn).WithMany().HasForeignKey(x => x.MaMon).OnDelete(DeleteBehavior.Cascade);
        });
    }
}

public static class PromotionPricing
{
    public static decimal DiscountedPrice(decimal price, decimal percent) => decimal.Round(price * (100m - percent) / 100m, 0, MidpointRounding.AwayFromZero);

    public static async Task<Dictionary<int, decimal>> DiscountsAsync(QuanLyNhaHangContext db, IEnumerable<int> dishIds)
    {
        var ids = dishIds.Distinct().ToArray();
        var today = DateOnly.FromDateTime(BookingRules.Now);
        return await db.KhuyenMaiMonAns.AsNoTracking()
            .Where(x => ids.Contains(x.MaMon) && x.KhuyenMai.TrangThai == "Hoạt động" && x.KhuyenMai.NgayBatDau <= today && x.KhuyenMai.NgayKetThuc >= today)
            .GroupBy(x => x.MaMon)
            .Select(g => new { Id = g.Key, Percent = g.Max(x => x.KhuyenMai.PhanTramGiam ?? 0m) })
            .ToDictionaryAsync(x => x.Id, x => x.Percent);
    }
}
