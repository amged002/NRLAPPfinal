using Microsoft.AspNetCore.Mvc;
using NRLApp.Models;

namespace NRLApp.Controllers
{
    public class CurrencyController : Controller
    {
        // Viser valutakonverteringsskjemaet
        [HttpGet]
        public IActionResult Convert() => View(new CurrencyData());

        // Håndterer valutakonvertering
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Convert(CurrencyData data)
        {
            if (!ModelState.IsValid) return View(data);

            // Fast vekslingskurs for 26. august 2025: 11.50 NOK per EUR
            // Dette er en forenklet implementasjon med statisk kurs
            // I en produksjonsapp ville man hente kurs fra en API
            data.ExchangeRate = 11.50;
            
            // Beregn NOK beløp
            data.NokAmount = data.EuroAmount * data.ExchangeRate;
            
            return View(data);
        }
    }
}
