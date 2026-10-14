using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace RestaurantManagement.Models;
public partial class DatBan {
 [StringLength(32)] public string? MaCheckIn {get;set;}
 [Column(TypeName="decimal(18,2)")] public decimal TienCoc {get;set;}
 public bool CocDaHoan {get;set;}
 public DateTime? CheckInUtc {get;set;}
 public DateTime? KetThucUtc {get;set;}
 [StringLength(40)] public string? MaHoaDon {get;set;}
 [Column(TypeName="decimal(18,2)")] public decimal DaThuTaiBan {get;set;}
 public int? NhanVienThu {get;set;}
 public ICollection<ChiTietGoiMon> CacMon {get;set;}=new List<ChiTietGoiMon>();
 [NotMapped] public decimal TongGoc=>CacMon.Sum(x=>x.GiaGoc*x.SoLuong);
 [NotMapped] public decimal TongTien=>CacMon.Sum(x=>x.DonGia*x.SoLuong);
 [NotMapped] public decimal ConLai=>Math.Max(0,TongTien-(CocDaHoan?0:TienCoc));
}
public class GiuBan {
 [Key] public Guid Id {get;set;}
 public int MaKhachHang {get;set;}
 public int MaBan {get;set;}
 public DateOnly Ngay {get;set;}
 public TimeOnly Gio {get;set;}
 public int SoKhach {get;set;}
 public DateTime HetHanUtc {get;set;}
 public int? MaDatBan {get;set;}
 public string? GioHangJson {get;set;}
 [StringLength(255)] public string? GhiChu {get;set;}
}
public class ChiTietGoiMon {
 [Key] public int Id {get;set;}
 public int MaDatBan {get;set;}
 public DatBan DatBan {get;set;}=null!;
 public int MaMon {get;set;}
 [StringLength(100)] public string TenMon {get;set;}="";
 public int SoLuong {get;set;}
 [Column(TypeName="decimal(18,2)")] public decimal GiaGoc {get;set;}
 [Column(TypeName="decimal(5,2)")] public decimal Giam {get;set;}
 [Column(TypeName="decimal(18,2)")] public decimal DonGia {get;set;}
 public bool DatTruoc {get;set;}
 public Guid RequestId {get;set;}
}
public record ChonMon(int MaMon,int SoLuong);
public record GiaMon(int MaMon,string TenMon,int SoLuong,decimal GiaGoc,decimal Giam,decimal DonGia);
public partial class QuanLyNhaHangContext {
 public DbSet<GiuBan> GiuBans {get;set;}
 public DbSet<ChiTietGoiMon> ChiTietGoiMons {get;set;}
 private static void ConfigureWorkflow(ModelBuilder b){
  b.Entity<GiuBan>().ToTable("GiuBan");
  b.Entity<GiuBan>().HasIndex(x=>new{x.MaBan,x.Ngay,x.HetHanUtc});
  b.Entity<ChiTietGoiMon>().ToTable("ChiTietGoiMon");
  b.Entity<ChiTietGoiMon>().HasOne(x=>x.DatBan).WithMany(x=>x.CacMon).HasForeignKey(x=>x.MaDatBan).OnDelete(DeleteBehavior.Restrict);
  b.Entity<ChiTietGoiMon>().HasIndex(x=>x.RequestId).IsUnique();
  b.Entity<DatBan>().HasIndex(x=>x.MaCheckIn).IsUnique().HasFilter("[MaCheckIn] IS NOT NULL");
  b.Entity<DatBan>().HasIndex(x=>x.MaHoaDon).IsUnique().HasFilter("[MaHoaDon] IS NOT NULL");
 }
}
