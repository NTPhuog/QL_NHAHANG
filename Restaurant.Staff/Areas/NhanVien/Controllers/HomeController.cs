using Microsoft.AspNetCore.Mvc;

namespace RestaurantManagement.Areas.NhanVien.Controllers
{
    [Area("NhanVien")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}