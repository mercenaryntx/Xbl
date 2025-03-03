using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class ClientConditions
{
    public AllowedPlatform[] AllowedPlatforms { get; set; }
}