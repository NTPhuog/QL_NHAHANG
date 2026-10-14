using RestaurantManagement.Models;
namespace RestaurantManagement.CustomerModels;
public class BookingForm {
 public BanAn Table {get;set;}=null!;
 public Guid? HoldId {get;set;}
 public DateOnly Date {get;set;}
 public TimeOnly Time {get;set;}
 public int Guests {get;set;}
 public string? Note {get;set;}
 public List<MonAn> Menu {get;set;}=[];
 public List<ChonMon> Selected {get;set;}=[];
 public Dictionary<int,decimal> Discounts {get;set;}=[];
}
