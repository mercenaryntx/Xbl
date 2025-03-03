using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class RelatedProduct
{
    public string RelatedProductId { get; set; }
    public string RelationshipType { get; set; }
}