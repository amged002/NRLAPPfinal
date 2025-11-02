namespace NRLApp.Models
{
    public enum ObstacleType { Building, Mast, WindTurbine, PowerLine, Crane, Other }
    public enum HeightBand { H0_15, H15_30, H30_50, H50_75, H75_100, H100Plus }

    public sealed class ObstacleWizardState
    {
        public double? CenterLat { get; set; }
        public double? CenterLng { get; set; }
        public int? RadiusMeters { get; set; }
        public ObstacleType? Type { get; set; }
        public HeightBand? Height { get; set; }
    }
}
