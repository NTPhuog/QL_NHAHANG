using RestaurantManagement.Security;
using RestaurantManagement.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
var failures=0;
void Check(string name,bool ok){Console.WriteLine($"{(ok?"PASS":"FAIL")} {name}");if(!ok)failures++;}
var hash=Passwords.Hash("Test-Password-29");
Check("hashed password accepted",Passwords.Verify(hash,"Test-Password-29"));
Check("incorrect password rejected",!Passwords.Verify(hash,"wrong"));
Check("legacy password migration compatibility",Passwords.Verify("legacy-password","legacy-password"));
Check("empty password rejected",!Passwords.Verify("",""));
bool Valid(object value)=>Validator.TryValidateObject(value,new ValidationContext(value),new List<ValidationResult>(),true);
Check("zero table capacity rejected",!Valid(new BanAn{TenBan="Bàn 1",SoChoNgoi=0,TrangThai="Trống"}));
Check("normal table accepted",Valid(new BanAn{TenBan="Bàn 1",SoChoNgoi=4,TrangThai="Trống"}));
Check("negative menu price rejected",!Valid(new MonAn{TenMon="Món",Gia=-1,TrangThai="Hiển thị"}));
Check("discount above 100 rejected",!Valid(new KhuyenMai{TenKhuyenMai="KM",PhanTramGiam=101,TrangThai="Hoạt động"}));

Check("lowercase admin route protected",AccessPolicy.RequiredRole(null,"quantri","index")=="Admin");
Check("mixed-case staff area protected",AccessPolicy.RequiredRole("nHaNvIeN","datban","index")=="Staff");
Check("lowercase table mutation protected",AccessPolicy.RequiredRole(null,"banan","create")=="Admin");
Check("public catalog allowed",AccessPolicy.Allows(AccessPolicy.RequiredRole(null,"monan","index"),null));
Check("customer denied admin",!AccessPolicy.Allows("Admin","KhachHang"));
Check("employee denied admin",!AccessPolicy.Allows("Admin","NhanVien"));
Check("admin can coordinate bookings",AccessPolicy.Allows("Staff","Admin"));
Check("unclassified controller defaults to admin",AccessPolicy.RequiredRole(null,"NewManagement","Index")=="Admin");
var appointment=new DateTime(2026,10,9,18,0,0);
var order=new DatBan{NgayDat=DateOnly.FromDateTime(appointment),GioDat=TimeOnly.FromDateTime(appointment),TrangThai="Đã duyệt"};
Check("future check-in rejected",!BookingRules.CanReceive(order,appointment.AddMinutes(-1)));
Check("check-in at booking start accepted",BookingRules.CanReceive(order,appointment));
Check("check-in before slot end accepted",BookingRules.CanReceive(order,appointment.AddMinutes(119)));
Check("check-in at expired slot rejected",!BookingRules.CanReceive(order,appointment.AddHours(2)));
order.TrangThai="Đã hủy";
Check("cancelled booking cannot check in",!BookingRules.CanReceive(order,appointment));
Check("cancelled booking does not block maintenance",!BookingRules.BlocksTableChange(order,appointment));
order.TrangThai="Đang phục vụ";
Check("late ongoing meal still blocks table change",BookingRules.BlocksTableChange(order,appointment.AddDays(1)));
order.TrangThai="Chờ xác nhận";
Check("upcoming pending reservation blocks capacity reduction",BookingRules.BlocksTableChange(order,appointment.AddHours(-1)));
Check("expired pending reservation does not block maintenance",!BookingRules.BlocksTableChange(order,appointment.AddHours(2)));
return failures;

