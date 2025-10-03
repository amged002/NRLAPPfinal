using System.ComponentModel.DataAnnotations;

namespace NRLApp.Models
{
    /// ViewModel for NRL obstacle reporting
    public class ObstacleData
    {
        [Required, StringLength(100)]
        [Display(Name = "Navn på hinder")]
        public string ObstacleName { get; set; } = string.Empty;

        [Range(1, 100000, ErrorMessage = "Høyden må være > 0")]
        [Display(Name = "Høyde (meter)")]
        public double ObstacleHeight { get; set; }

        [StringLength(500)]
        [Display(Name = "Beskrivelse")]
        public string? ObstacleDescription { get; set; }

        [Range(-90, 90)]
        [Display(Name = "Breddegrad (lat)")]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        [Display(Name = "Lengdegrad (lng)")]
        public double Longitude { get; set; }
    }
}