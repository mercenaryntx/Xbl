namespace Xbl.Client.Models.Xbl.Marketplace;

public class SkuFulfillmentData
{
    public string ProductId { get; set; }
    public string WuBundleId { get; set; }
    public string WuCategoryId { get; set; }
    public string PackageFamilyName { get; set; }
    public string SkuId { get; set; }
    public object Content { get; set; }
    public object PackageFeatures { get; set; }
}