using Microsoft.AspNetCore.Mvc;

namespace Turbina.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index(string workspace)
        {
            ViewData["Workspace"] = workspace ?? "<default-workspace>";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
