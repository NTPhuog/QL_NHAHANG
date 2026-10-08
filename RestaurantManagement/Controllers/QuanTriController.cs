using Microsoft.AspNetCore.Mvc;
namespace RestaurantManagement.Controllers;
public class QuanTriController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Home", new { area = "Admin" });
}
