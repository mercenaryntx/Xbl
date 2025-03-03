using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class PackageFeatures
{
    public bool SupportsIntelligentDelivery { get; set; }
    public bool SupportsInstallFeatures { get; set; }
    public bool SupportsInstallRecipes { get; set; }
}