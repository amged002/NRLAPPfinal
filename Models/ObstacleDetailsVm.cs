namespace NRLApp.Models
{
    public class ObstacleDetailsVm
    {
        public int Id { get; set; }
        public string? ObstacleName { get; set; }
        public int? HeightMeters { get; set; }
        public string? Description { get; set; }
        public string GeoJson { get; set; } = "{}";
        public bool IsDraft { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
