using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class Sku
{
    public DateTime LastModifiedDate { get; set; }
    public SkuLocalizedProperty[] LocalizedProperties { get; set; }
    public SkuMarketProperty[] MarketProperties { get; set; }
    public string ProductId { get; set; }
    public SkuProperties Properties { get; set; }
    public string SkuASchema { get; set; }
    public string SkuBSchema { get; set; }
    public string SkuId { get; set; }
    public string SkuType { get; set; }
    public object RecurrencePolicy { get; set; }
    public object SubscriptionPolicyId { get; set; }
}