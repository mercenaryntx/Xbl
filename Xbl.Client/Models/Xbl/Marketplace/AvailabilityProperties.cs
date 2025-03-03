using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class AvailabilityProperties
{
    public DateTime OriginalReleaseDate { get; set; }
    public DateTime PreOrderReleaseDate { get; set; }
    public string[] MerchandisingTags { get; set; }
}