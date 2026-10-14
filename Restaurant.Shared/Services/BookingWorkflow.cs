using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
namespace RestaurantManagement.Services;
public sealed class WorkflowException(string message):Exception(message);
public class BookingWorkflow(QuanLyNhaHangContext db) {
 public static bool Overlap(TimeOnly a,TimeOnly b)=>Math.Abs((a.ToTimeSpan()-b.ToTimeSpan()).TotalMinutes)<120;
 public static List<GiaMon> Basket(GiuBan hold)=>JsonSerializer.Deserialize<List<GiaMon>>(hold.GioHangJson??"[]")??[];
 public static decimal Total(IEnumerable<GiaMon> lines)=>lines.Sum(x=>x.DonGia*x.SoLuong);
 public static decimal Deposit(IEnumerable<GiaMon> lines)=>decimal.Round(Total(lines)/2m,0,MidpointRounding.AwayFromZero);
 private async Task<BanAn> LockTable(int id)=>await db.BanAns.FromSqlInterpolated($"SELECT * FROM dbo.BanAn WITH (UPDLOCK,HOLDLOCK) WHERE MaBan={id}").SingleOrDefaultAsync()??throw new WorkflowException("Bàn không còn tồn tại.");
 private async Task<GiuBan> LockHold(Guid id,int owner)=>await db.GiuBans.FromSqlInterpolated($"SELECT * FROM dbo.GiuBan WITH (UPDLOCK,HOLDLOCK) WHERE Id={id} AND MaKhachHang={owner}").SingleOrDefaultAsync()??throw new WorkflowException("Phiên giữ bàn không hợp lệ hoặc không thuộc tài khoản của bạn.");
 private async Task<DatBan> LockOrder(int id)=>await db.DatBans.FromSqlInterpolated($"SELECT * FROM dbo.DatBan WITH (UPDLOCK,HOLDLOCK) WHERE MaDatBan={id}").SingleOrDefaultAsync()??throw new WorkflowException("Không tìm thấy đơn đặt bàn.");
 private static void Live(GiuBan h){if(h.MaDatBan.HasValue||h.HetHanUtc<=DateTime.UtcNow)throw new WorkflowException("Phiên giữ bàn đã hết hạn. Vui lòng chọn và giữ bàn lại.");}
 private static void ValidSlot(BanAn b,DateOnly day,TimeOnly time,int guests){
  if(b.TrangThai!="Trống")throw new WorkflowException("Bàn đang tạm ngừng nhận khách.");
  if(day.ToDateTime(time)<=BookingRules.Now||time<new TimeOnly(10,0)||time>new TimeOnly(20,0))throw new WorkflowException("Chọn giờ trong tương lai từ 10:00 đến 20:00.");
  if(guests<1||guests>b.SoChoNgoi)throw new WorkflowException($"Bàn này nhận từ 1 đến {b.SoChoNgoi} khách.");
 }
 private async Task EnsureFree(int table,DateOnly day,TimeOnly time,Guid? except=null){
  var occupied=await db.DatBans.AsNoTracking().Where(x=>x.MaBan==table&&x.TrangThai=="Đang phục vụ").ToListAsync();
  var requested=day.ToDateTime(time);
  if(occupied.Any(x=>requested<x.NgayDat.ToDateTime(x.GioDat).AddHours(2)&&requested.AddHours(2)>(x.CheckInUtc?.AddHours(7)??BookingRules.Now)))throw new WorkflowException("Bàn đang phục vụ trong khung giờ này.");
  var booked=await db.DatBans.AsNoTracking().Where(x=>x.MaBan==table&&x.NgayDat==day&&(x.TrangThai=="Chờ xác nhận"||x.TrangThai=="Đã duyệt"||x.TrangThai=="Đang phục vụ")).Select(x=>x.GioDat).ToListAsync();
  if(booked.Any(x=>Overlap(x,time)))throw new WorkflowException("Bàn đã có khách đặt trong khung giờ này.");
  var now=DateTime.UtcNow;
  var holds=await db.GiuBans.AsNoTracking().Where(x=>x.MaBan==table&&x.Ngay==day&&x.MaDatBan==null&&x.HetHanUtc>now&&x.Id!=except).Select(x=>x.Gio).ToListAsync();
  if(holds.Any(x=>Overlap(x,time)))throw new WorkflowException("Bàn đang được khách khác giữ trong 5 phút. Vui lòng chọn bàn hoặc giờ khác.");
 }
 public async Task<GiuBan> Acquire(int owner,int table,DateOnly day,TimeOnly time,int guests){
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
  await db.KhachHangs.FromSqlInterpolated($"SELECT * FROM dbo.KhachHang WITH (UPDLOCK,HOLDLOCK) WHERE MaKhachHang={owner}").AsNoTracking().SingleAsync();
  var b=await LockTable(table);ValidSlot(b,day,time,guests);
  var now=DateTime.UtcNow;var mine=await db.GiuBans.Where(x=>x.MaKhachHang==owner&&x.MaDatBan==null&&x.HetHanUtc>now).ToListAsync();
  var same=mine.FirstOrDefault(x=>x.MaBan==table&&x.Ngay==day&&x.Gio==time);
  await EnsureFree(table,day,time,same?.Id);
  if(same!=null){await tx.CommitAsync();return same;}
  // One live hold per customer. Reopening the same slot does not extend its deadline.
  foreach(var previous in mine)previous.HetHanUtc=now;
  var h=new GiuBan{Id=Guid.NewGuid(),MaKhachHang=owner,MaBan=table,Ngay=day,Gio=time,SoKhach=guests,HetHanUtc=now.AddMinutes(5)};
  db.GiuBans.Add(h);await db.SaveChangesAsync();await tx.CommitAsync();return h;
 }
 public async Task<List<GiaMon>> Price(IEnumerable<ChonMon>? choices){
  var all=(choices??[]).ToList();if(all.Count>200||all.Any(x=>x.SoLuong<0||x.SoLuong>99))throw new WorkflowException("Số lượng mỗi món từ 0 đến 99.");
  var selected=all.Where(x=>x.SoLuong>0).GroupBy(x=>x.MaMon).Select(x=>new ChonMon(x.Key,x.Sum(y=>y.SoLuong))).ToList();
  if(selected.Any(x=>x.SoLuong>99))throw new WorkflowException("Tối đa 99 phần mỗi món.");
  var ids=selected.Select(x=>x.MaMon).ToArray();var dishes=await db.MonAns.AsNoTracking().Where(x=>ids.Contains(x.MaMon)&&x.TrangThai=="Hiển thị").ToListAsync();
  if(dishes.Count!=ids.Length)throw new WorkflowException("Có món đã hết hoặc ngừng bán. Vui lòng chọn lại.");
  var discounts=await PromotionPricing.DiscountsAsync(db,ids);
  return selected.Select(x=>{var m=dishes.Single(y=>y.MaMon==x.MaMon);var off=discounts.GetValueOrDefault(m.MaMon);return new GiaMon(m.MaMon,m.TenMon,x.SoLuong,m.Gia,off,PromotionPricing.DiscountedPrice(m.Gia,off));}).ToList();
 }
 public async Task<GiuBan> Prepare(int owner,Guid id,int guests,string? note,IEnumerable<ChonMon>? choices){
  var info=await db.GiuBans.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id&&x.MaKhachHang==owner)??throw new WorkflowException("Không tìm thấy phiên giữ bàn.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
  var table=await LockTable(info.MaBan);var h=await LockHold(id,owner);Live(h);ValidSlot(table,h.Ngay,h.Gio,guests);
  if(note?.Length>255)throw new WorkflowException("Ghi chú tối đa 255 ký tự.");
  var lines=await Price(choices);if(table.SoChoNgoi<=2&&lines.Count>0)throw new WorkflowException("Bàn 2 chỗ không nhận đặt món trước. Bạn có thể gọi món khi đến.");
  await EnsureFree(h.MaBan,h.Ngay,h.Gio,h.Id);
  Live(h);h.SoKhach=guests;h.GhiChu=note?.Trim();h.GioHangJson=JsonSerializer.Serialize(lines);await db.SaveChangesAsync();await tx.CommitAsync();return h;
 }
 public async Task<int> Confirm(int owner,Guid id,bool simulatedSuccess){
  var info=await db.GiuBans.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id&&x.MaKhachHang==owner)??throw new WorkflowException("Không tìm thấy phiên giữ bàn.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
  var b=await LockTable(info.MaBan);var h=await LockHold(id,owner);
  if(h.MaDatBan.HasValue){await tx.CommitAsync();return h.MaDatBan.Value;}
  Live(h);ValidSlot(b,h.Ngay,h.Gio,h.SoKhach);if(h.GioHangJson==null)throw new WorkflowException("Vui lòng kiểm tra món và số khách trước khi xác nhận.");
  var lines=Basket(h);if(b.SoChoNgoi<=2&&lines.Count>0)throw new WorkflowException("Bàn 2 chỗ không nhận đặt món trước.");
  if(lines.Count>0&&!simulatedSuccess)throw new WorkflowException("Thanh toán giả lập thất bại. Chưa thu cọc, bạn có thể thử lại trước khi hết giờ giữ bàn.");
  var ids=lines.Select(x=>x.MaMon).ToArray();if(await db.MonAns.CountAsync(x=>ids.Contains(x.MaMon)&&x.TrangThai=="Hiển thị")!=ids.Length)throw new WorkflowException("Có món vừa hết. Quay lại chọn món trước khi xác nhận.");
  await EnsureFree(h.MaBan,h.Ngay,h.Gio,h.Id);
  Live(h);var d=new DatBan{MaKhachHang=owner,MaBan=h.MaBan,NgayDat=h.Ngay,GioDat=h.Gio,SoLuongKhach=h.SoKhach,GhiChu=h.GhiChu,TrangThai="Đã duyệt",NgayTao=BookingRules.Now,MaCheckIn="NH-"+Convert.ToHexString(RandomNumberGenerator.GetBytes(10)),TienCoc=Deposit(lines)};
  foreach(var line in lines)d.CacMon.Add(ToLine(line,true,Guid.NewGuid()));
  db.DatBans.Add(d);await db.SaveChangesAsync();h.MaDatBan=d.MaDatBan;await db.SaveChangesAsync();await tx.CommitAsync();return d.MaDatBan;
 }
 private static ChiTietGoiMon ToLine(GiaMon line,bool preorder,Guid request)=>new(){MaMon=line.MaMon,TenMon=line.TenMon,SoLuong=line.SoLuong,GiaGoc=line.GiaGoc,Giam=line.Giam,DonGia=line.DonGia,DatTruoc=preorder,RequestId=request};
 public async Task Release(int owner,Guid id){await db.GiuBans.Where(x=>x.Id==id&&x.MaKhachHang==owner&&x.MaDatBan==null).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.HetHanUtc,DateTime.UtcNow));}
 public async Task<int> CheckIn(string? code){
  code=code?.Trim().ToUpperInvariant();if(code==null||!System.Text.RegularExpressions.Regex.IsMatch(code,"^NH-[A-F0-9]{20}$"))throw new WorkflowException("Mã check-in không hợp lệ.");
  var info=await db.DatBans.AsNoTracking().FirstOrDefaultAsync(x=>x.MaCheckIn==code)??throw new WorkflowException("Không tìm thấy mã đặt bàn.");
  return await CheckInOrder(info.MaDatBan);
 }
 public async Task<int> CheckInOrder(int id){
  var info=await db.DatBans.AsNoTracking().SingleOrDefaultAsync(x=>x.MaDatBan==id)??throw new WorkflowException("Không tìm thấy đơn đặt bàn.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);var table=await LockTable(info.MaBan);var d=await LockOrder(info.MaDatBan);
  if(d.TrangThai=="Đang phục vụ"){await tx.CommitAsync();return d.MaDatBan;}
  if(d.TrangThai is not("Đã duyệt" or "Chờ xác nhận"))throw new WorkflowException("Mã này đã hoàn tất hoặc đơn đã hủy.");
  var now=BookingRules.Now;var start=d.NgayDat.ToDateTime(d.GioDat);if(now>=start.AddHours(2))throw new WorkflowException("Đơn đã quá hạn check-in.");
  if(now<start){
   var reservations=await db.DatBans.AsNoTracking().Where(x=>x.MaBan==d.MaBan&&x.MaDatBan!=d.MaDatBan&&(x.TrangThai=="Đã duyệt"||x.TrangThai=="Chờ xác nhận")).ToListAsync();
   var utc=DateTime.UtcNow;var holds=await db.GiuBans.AsNoTracking().Where(x=>x.MaBan==d.MaBan&&x.MaDatBan==null&&x.HetHanUtc>utc).ToListAsync();
   if(reservations.Any(x=>x.NgayDat.ToDateTime(x.GioDat)<start.AddHours(2)&&x.NgayDat.ToDateTime(x.GioDat).AddHours(2)>now)||holds.Any(x=>x.Ngay.ToDateTime(x.Gio)<start.AddHours(2)&&x.Ngay.ToDateTime(x.Gio).AddHours(2)>now))throw new WorkflowException("Chưa thể nhận bàn sớm vì bàn có lượt đặt hoặc giữ chỗ khác.");
  }
  if(table.TrangThai!="Trống"||await db.DatBans.AnyAsync(x=>x.MaBan==d.MaBan&&x.MaDatBan!=d.MaDatBan&&x.TrangThai=="Đang phục vụ"))throw new WorkflowException("Bàn chưa sẵn sàng hoặc còn khách đang phục vụ.");
  d.TrangThai="Đang phục vụ";d.CheckInUtc=DateTime.UtcNow;await db.SaveChangesAsync();await tx.CommitAsync();return d.MaDatBan;
 }
 public async Task AddDish(int order,int dish,int quantity,Guid request){
  if(request==Guid.Empty||quantity is <1 or >99)throw new WorkflowException("Chọn từ 1 đến 99 phần.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);var d=await LockOrder(order);
  if(d.TrangThai!="Đang phục vụ"||d.MaHoaDon!=null)throw new WorkflowException("Chỉ thêm món cho bàn đang phục vụ, chưa xuất hóa đơn.");
  if(await db.ChiTietGoiMons.AnyAsync(x=>x.RequestId==request&&x.MaDatBan==order)){await tx.CommitAsync();return;}
  var line=(await Price([new ChonMon(dish,quantity)])).Single();d.CacMon.Add(ToLine(line,false,request));await db.SaveChangesAsync();await tx.CommitAsync();
 }
 public async Task Finish(int id,int staff,decimal expectedTotal){
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);var d=await LockOrder(id);
  if(d.TrangThai=="Hoàn thành"&&d.MaHoaDon!=null){await tx.CommitAsync();return;}
  if(d.TrangThai!="Đang phục vụ")throw new WorkflowException("Bàn chưa check-in hoặc đã đóng.");
  await db.Entry(d).Collection(x=>x.CacMon).LoadAsync();
  if(d.CacMon.Count==0)throw new WorkflowException("Chưa có món để xuất hóa đơn.");
  if(d.TongTien!=expectedTotal)throw new WorkflowException("Danh sách món vừa thay đổi. Kiểm tra lại tổng tiền trước khi thu.");
  if(d.TienCoc>d.TongTien)throw new WorkflowException("Tiền cọc vượt tổng hóa đơn. Cần kiểm tra lại đơn.");
  d.DaThuTaiBan=d.ConLai;d.NhanVienThu=staff;d.MaHoaDon=$"HD-{BookingRules.Now:yyyyMMdd}-{d.MaDatBan:D6}";d.KetThucUtc=DateTime.UtcNow;d.TrangThai="Hoàn thành";
  await db.SaveChangesAsync();await tx.CommitAsync();
 }
 public async Task Cancel(int id,int? owner,string reason){
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);var d=await LockOrder(id);
  if(owner.HasValue&&d.MaKhachHang!=owner)throw new WorkflowException("Không tìm thấy đơn của bạn.");
  if(d.TrangThai is not("Đã duyệt" or "Chờ xác nhận"))throw new WorkflowException("Chỉ hủy được đơn chưa check-in.");
  d.TrangThai="Đã hủy";d.LyDoHuy=reason;d.CocDaHoan=d.TienCoc>0;await db.SaveChangesAsync();await tx.CommitAsync();
 }
}
