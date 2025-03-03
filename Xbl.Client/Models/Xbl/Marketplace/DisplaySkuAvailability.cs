using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class DisplaySkuAvailability
{
    public Sku Sku { get; set; }
    public Availability[] Availabilities { get; set; }
    public HistoricalBestAvailability[] HistoricalBestAvailabilities { get; set; }
}