using System.ComponentModel.DataAnnotations;

namespace NRLApp.Models
{
    /// <summary>
    /// Model for currency conversion
    /// </summary>
    public class CurrencyData
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Beløp må være større enn 0")]
        [Display(Name = "Beløp i Euro (EUR)")]
        public decimal EuroAmount { get; set; }
        
        [Display(Name = "Beløp i Norske kroner (NOK)")]
        public decimal NokAmount { get; set; }
        
        [Display(Name = "Dato")]
        [DataType(DataType.Date)]
        public DateTime? ConversionDate { get; set; }
        
        [Display(Name = "Vekslingskurs (EUR til NOK)")]
        public decimal ExchangeRate { get; set; }
    }
}
