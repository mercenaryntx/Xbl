using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class XblProduct
{
    public DateTime LastModifiedDate { get; set; }
    public ProductLocalizedProperty[] LocalizedProperties { get; set; }
    public ProductMarketProperty[] MarketProperties { get; set; }
    public string ProductASchema { get; set; }
    public string ProductBSchema { get; set; }
    public string ProductId { get; set; }
    public ProductProperties Properties { get; set; }
    public AlternateId[] AlternateIds { get; set; }
    public object DomainDataVersion { get; set; }
    public string IngestionSource { get; set; }
    public bool IsMicrosoftProduct { get; set; }
    public string PreferredSkuId { get; set; }
    public string ProductType { get; set; }
    public ValidationData ValidationData { get; set; }
    public object[] MerchandizingTags { get; set; }
    public string PartD { get; set; }
    public string SandboxId { get; set; }
    public string ProductFamily { get; set; }
    public string SchemaVersion { get; set; }
    public bool IsSandboxedProduct { get; set; }
    public string ProductKind { get; set; }
    public DisplaySkuAvailability[] DisplaySkuAvailabilities { get; set; }
}