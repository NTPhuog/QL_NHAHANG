using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
namespace RestaurantManagement.Controllers;
public class BanAnController(QuanLyNhaHangContext db) : Controller {
 public async Task<IActionResult> Index()=>View(await db.BanAns.AsNoTracking().Where(x=>x.TrangThai=="Trống").OrderBy(x=>x.TenBan).ToListAsync());
 public async Task<IActionResult> Details(int id){var item=await db.BanAns.AsNoTracking().FirstOrDefaultAsync(x=>x.MaBan==id&&x.TrangThai=="Trống");return item==null?NotFound():View(item);}
}
