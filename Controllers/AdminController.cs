using Microsoft.AspNetCore.Mvc;

namespace MusicPortal.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
