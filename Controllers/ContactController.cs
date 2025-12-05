using Microsoft.AspNetCore.Mvc;

namespace NRLApp.Controllers
{

/// Controller til kontaktsiden.

    public class ContactController : Controller
    {
   
    /// Returnerer kontaktsiden uten modell-data
   
        public IActionResult Index()
        {
            return View();
        }
    }
}
