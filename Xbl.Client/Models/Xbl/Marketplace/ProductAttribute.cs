using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class ProductAttribute
{
    public string Name { get; set; }
    public int? Minimum { get; set; }
    public int? Maximum { get; set; }
    public string[] ApplicablePlatforms { get; set; }
    public object Group { get; set; }
}