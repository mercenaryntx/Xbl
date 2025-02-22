namespace Xbl.Client.Models.Dbox;

public class ProductVersion
{
    public string Title { get; set; }
    public string ProductId { get; set; }
    public string LegacyWindowsStoreProductId { get; set; }
    public string LegacyWindowsStoreParentProductId { get; set; }
    public string LegacyWindowsPhoneProductId { get; set; }
    public string LegacyWindowsPhoneParentProductId { get; set; }
    public string LegacyXboxProductId { get; set; }
    public string PackageFamilyName { get; set; }
    public string PackageIdentityName { get; set; }
    public string[] XboxConsoleGenOptimized { get; set; }
    public string[] XboxConsoleGenCompatible { get; set; }
    public DateTime? RevisionId { get; set; }
}