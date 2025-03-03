using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class AvailabilityRemediation
{
    public string RemediationId { get; set; }
    public string Type { get; set; }
    public string BigId { get; set; }
}