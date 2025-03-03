using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class ProductProperties
{
    public ProductAttribute[] Attributes { get; set; }
    public bool CanInstallToSDCard { get; set; }
    public string Category { get; set; }
    public string[] Categories { get; set; }
    public object Subcategory { get; set; }
    public bool IsAccessible { get; set; }
    public bool IsDemo { get; set; }
    public bool IsLineOfBusinessApp { get; set; }
    public bool IsPublishedToLegacyWindowsPhoneStore { get; set; }
    public bool IsPublishedToLegacyWindowsStore { get; set; }
    public string PackageFamilyName { get; set; }
    public string PackageIdentityName { get; set; }
    public string PublisherCertificateName { get; set; }
    public string PublisherId { get; set; }
    public SkuDisplayGroup[] SkuDisplayGroups { get; set; }
    public string XboxLiveTier { get; set; }
    public object XboxXPA { get; set; }
    public string XboxCrossGenSetId { get; set; }
    public string[] XboxConsoleGenOptimized { get; set; }
    public string[] XboxConsoleGenCompatible { get; set; }
    public bool XboxLiveGoldRequired { get; set; }
    public string ExtendedMetadata { get; set; }
    public Xbox XBOX { get; set; }
    public object OwnershipType { get; set; }
    public string PdpBackgroundColor { get; set; }
    public bool HasAddOns { get; set; }
    public DateTime RevisionId { get; set; }
    public string ProductGroupId { get; set; }
    public string ProductGroupName { get; set; }
    public DateTime IsPrivateBeforeDateHint { get; set; }
}