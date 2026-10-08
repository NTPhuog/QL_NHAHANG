using Microsoft.AspNetCore.Mvc;
namespace RestaurantManagement.Controllers;
public class KhachHangController : Controller {
 public IActionResult Index()=>RedirectToAction("Index","KhachHang",new{area="Admin"});
}
