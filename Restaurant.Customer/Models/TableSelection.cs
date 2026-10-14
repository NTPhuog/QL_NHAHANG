using RestaurantManagement.Models;
namespace RestaurantManagement.CustomerModels;
public record TableOption(BanAn Table,bool Booked,bool Held=false,bool HeldByYou=false) {
 public bool Available=>Table.TrangThai=="Trống"&&!Booked&&(!Held||HeldByYou);
 public string Status=>Table.TrangThai!="Trống"?(Table.TrangThai??"Tạm ngừng"):Booked?"Đã đặt":Held?(HeldByYou?"Bạn đang giữ chỗ":"Đang giữ chỗ (5 phút)"):"Còn trống";
 public int Group=>Table.SoChoNgoi<=2?2:Table.SoChoNgoi<=6?6:Table.SoChoNgoi<=12?12:Table.SoChoNgoi<=24?24:100;
}
public class TableSelection {
 public DateOnly Date {get;set;}
 public TimeOnly Time {get;set;}
 public int Guests {get;set;}=2;
 public int? Group {get;set;}
 public bool Valid {get;set;}=true;
 public List<TableOption> Tables {get;set;}=[];
 public static string Label(int group)=>group switch{2=>"2 chỗ",6=>"4–6 chỗ",12=>"8–12 chỗ",24=>"Tối đa 24 chỗ",_=>"Nhóm lớn"};
 public static string Zone(int group)=>group switch{2=>"Góc hẹn bên cửa sổ",6=>"Không gian gia đình",12=>"Sum họp cùng bạn bè",24=>"Phòng tiệc riêng",_=>"Không gian mở"};
}
