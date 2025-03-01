namespace Xbl.Client.Models.Xbl.Marketplace;

public class SkuProperties
{
    public object EarlyAdopterEnrollmentUrl { get; set; }
    public SkuFulfillmentData FulfillmentData { get; set; }
    public string FulfillmentType { get; set; }
    public string FulfillmentPluginId { get; set; }
    public bool HasThirdPartyIAPs { get; set; }
    public string LastUpdateDate { get; set; }
    public HardwareProperties HardwareProperties { get; set; }
    public object[] HardwareRequirements { get; set; }
    public string[] HardwareWarningList { get; set; }
    public string InstallationTerms { get; set; }
    public Package[] Packages { get; set; }
    public string VersionString { get; set; }
    public string[] SkuDisplayGroupIds { get; set; }
    public bool XboxXPA { get; set; }
    public object[] BundledSkus { get; set; }
    public bool IsRepurchasable { get; set; }
    public int SkuDisplayRank { get; set; }
    public object DisplayPhysicalStoreInventory { get; set; }
    public string[] VisibleToB2BServiceIds { get; set; }
    public object[] AdditionalIdentifiers { get; set; }
    public bool IsTrial { get; set; }
    public bool IsPreOrder { get; set; }
    public bool IsBundle { get; set; }
    public int TrialTimeInSeconds { get; set; }
}