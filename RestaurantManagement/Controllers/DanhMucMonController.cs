using Microsoft.AspNetCore.Mvc;
namespace RestaurantManagement.Controllers;
public class DanhMucMonController : Controller {
 public IActionResult Index()=>RedirectToAction("Index","DanhMucMon",new{area="Admin"});
}
