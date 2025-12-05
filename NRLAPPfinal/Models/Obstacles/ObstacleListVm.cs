namespace NRLApp.Models.Obstacles
{
    /// <summary>
    /// Filteringsverdier for listesiden.
    /// </summary>
    public enum ObstacleListStatusFilter
    {
        Draft,
        Pending,
        Approved,
        Rejected
    }

    /// <summary>
    /// Parametere som bygges fra forespørselens query-parametere.
    /// </summary>
    public sealed class ObstacleListFilter
    {
        public int? Id { get; set; }

        // NYTT: vi filtrerer på kategori i stedet for hinder-navn
        public string? Category { get; set; }

        public int? MinHeightMeters { get; set; }
        public int? MaxHeightMeters { get; set; }

        public ObstacleListStatusFilter? Status { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        // NYTT: filtrering på organisasjon
        public int? OrganizationId { get; set; }
    }

    /// <summary>
    /// Viewmodel for tabellen som vises i Obstacle/List
    /// </summary>
    public sealed class ObstacleListVm
    {
        public ObstacleListFilter Filter { get; set; } = new();

        public IEnumerable<ObstacleListItem> Items { get; set; } = Array.Empty<ObstacleListItem>();

        // NYTT: brukes til å fylle nedtrekksliste for organisasjoner
        public IEnumerable<NRLApp.Models.OrganizationVm> Organizations { get; set; }
            = Array.Empty<NRLApp.Models.OrganizationVm>();
    }
}
