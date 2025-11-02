namespace NRLApp.Models
{
    public sealed class ObstacleData
    {
        public double CenterLat { get; set; }
        public double CenterLng { get; set; }
        public int RadiusMeters { get; set; }
        public ObstacleType Type { get; set; }
        public int HeightMinMeters { get; set; }
        public int? HeightMaxMeters { get; set; } // null = 100+
    }
}
