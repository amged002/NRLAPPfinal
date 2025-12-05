using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace NRLApp.Controllers
{

/// Viser Homepage for innloggende brukere.

    [Authorize]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
