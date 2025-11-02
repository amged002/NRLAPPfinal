namespace NRLApp.Models
{
    public sealed class ObstacleListItem
    {
        public long Id { get; set; }
        public double CenterLat { get; set; }
        public double CenterLng { get; set; }
        public int RadiusMeters { get; set; }
        public string Type { get; set; } = "";
        public int HeightMinMeters { get; set; }
        public int? HeightMaxMeters { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
