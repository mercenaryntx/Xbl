using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class Conditions
{
    public ClientConditions ClientConditions { get; set; }
    public DateTime EndDate { get; set; }
    public string[] ResourceSetIds { get; set; }
    public DateTime StartDate { get; set; }
    public string[] EligibilityPredicateIds { get; set; }
    public int SupportedCatalogVersion { get; set; }
}