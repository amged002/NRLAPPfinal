using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NRLApp.Models.Obstacles
{
    public class DrawState
    {
        public string? GeoJson { get; set; }
    }

    public class ObstacleMetaVm
    {
        [StringLength(100, ErrorMessage = "Kategori kan maks være 100 tegn.")]
        [Display(Name = "Kategori")]
        public string? Category { get; set; }

        [StringLength(100, ErrorMessage = "Navn kan maks være 100 tegn.")]
        [Display(Name = "Hinder")]
        public string? ObstacleName { get; set; }

        [Required(ErrorMessage = "Oppgi høyde.")]
        [Range(0, 10000, ErrorMessage = "Høyden må være et tall ≥ 0.")]
        [Display(Name = "Høyde")]
        public double? HeightValue { get; set; }

        [Display(Name = "Enhet")]
        public string HeightUnit { get; set; } = "m";

        [StringLength(1000, ErrorMessage = "Beskrivelsen kan maks være 1000 tegn.")]
        [Display(Name = "Beskrivelse")]
        public string? Description { get; set; }

        [Display(Name = "Lagre som utkast")]
        public bool SaveAsDraft { get; set; }

        [Display(Name = "Bilde av hinder (valgfritt)")]
        public IFormFile? ImageFile { get; set; }
    }
}
