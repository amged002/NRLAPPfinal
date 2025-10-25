using System.ComponentModel.DataAnnotations;

namespace NRLApp.Models
{
    /// Model for currency conversion
    public class CurrencyData
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Beløp må være større enn 0")]
        [Display(Name = "Beløp i Euro (EUR)")]
        public double EuroAmount { get; set; }
        
        [Display(Name = "Beløp i Norske kroner (NOK)")]
        public double NokAmount { get; set; }
        
        [Display(Name = "Dato")]
        [DataType(DataType.Date)]
        public DateTime? ConversionDate { get; set; }
        
        [Display(Name = "Vekslingskurs (EUR til NOK)")]
        public double ExchangeRate { get; set; }
    }
}
