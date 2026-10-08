using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
namespace RestaurantManagement.Controllers;
public class MonAnController(QuanLyNhaHangContext db) : Controller {
 public async Task<IActionResult> Index()=>View(await db.MonAns.AsNoTracking().Include(x=>x.MaDanhMucNavigation).Where(x=>x.TrangThai=="Hiển thị").ToListAsync());
 public async Task<IActionResult> Details(int id){var item=await db.MonAns.AsNoTracking().Include(x=>x.MaDanhMucNavigation).FirstOrDefaultAsync(x=>x.MaMon==id&&x.TrangThai=="Hiển thị");return item==null?NotFound():View(item);}
}
