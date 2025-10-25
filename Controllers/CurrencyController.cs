using Microsoft.AspNetCore.Mvc;
using NRLApp.Models;

namespace NRLApp.Controllers
{
    public class CurrencyController : Controller
    {
        // Fast vekslingskurs: 11.50 NOK per EUR
        private const decimal ExchangeRateEurToNok = 11.50m;

        // Viser valutakonverteringsskjemaet
        [HttpGet]
        public IActionResult Convert() => View(new CurrencyData());

        // Håndterer valutakonvertering
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Convert(CurrencyData data)
        {
            if (!ModelState.IsValid) return View(data);

            // Bruker fast vekslingskurs for konvertering
            // I en produksjonsapp ville man hente kurs fra en API
            data.ExchangeRate = ExchangeRateEurToNok;
            
            // Beregn NOK beløp
            data.NokAmount = data.EuroAmount * data.ExchangeRate;
            
            return View(data);
        }
    }
}
