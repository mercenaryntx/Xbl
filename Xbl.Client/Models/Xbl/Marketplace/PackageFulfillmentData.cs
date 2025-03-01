namespace Xbl.Client.Models.Xbl.Marketplace;

public class PackageFulfillmentData
{
    public string ProductId { get; set; }
    public string WuBundleId { get; set; }
    public string WuCategoryId { get; set; }
    public string PackageFamilyName { get; set; }
    public string SkuId { get; set; }
    public string PackageContentId { get; set; }
    public object Content { get; set; }
    public PackageFeatures PackageFeatures { get; set; }
}