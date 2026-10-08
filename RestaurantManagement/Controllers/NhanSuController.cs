using Microsoft.AspNetCore.Mvc;
namespace RestaurantManagement.Controllers;
public class NhanSuController : Controller {
 public IActionResult Index()=>RedirectToAction("Index","NhanSu",new{area="Admin"});
}
