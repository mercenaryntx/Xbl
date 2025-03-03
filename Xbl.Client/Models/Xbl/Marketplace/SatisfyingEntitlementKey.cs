using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class SatisfyingEntitlementKey
{
    public string[] EntitlementKeys { get; set; }
    public string[] LicensingKeyIds { get; set; }
    public DateTime PreOrderReleaseDate { get; set; }
}