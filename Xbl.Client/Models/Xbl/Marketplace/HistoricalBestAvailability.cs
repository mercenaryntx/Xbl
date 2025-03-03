using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class HistoricalBestAvailability
{
    public string[] Actions { get; set; }
    public string AvailabilityASchema { get; set; }
    public string AvailabilityBSchema { get; set; }
    public string AvailabilityId { get; set; }
    public Conditions Conditions { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public string[] Markets { get; set; }
    public OrderManagementData OrderManagementData { get; set; }
    public AvailabilityProperties Properties { get; set; }
    public string SkuId { get; set; }
    public int DisplayRank { get; set; }
    public AlternateId[] AlternateIds { get; set; }
    public string ProductASchema { get; set; }
}