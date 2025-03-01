namespace Xbl.Client.Models.Xbl.Marketplace;

public class SkuLocalizedProperty
{
    public object[] Contributors { get; set; }
    public string[] Features { get; set; }
    public string MinimumNotes { get; set; }
    public string RecommendedNotes { get; set; }
    public string ReleaseNotes { get; set; }
    public object DisplayPlatformProperties { get; set; }
    public string SkuDescription { get; set; }
    public string SkuTitle { get; set; }
    public string SkuButtonTitle { get; set; }
    public object DeliveryDateOverlay { get; set; }
    public object[] SkuDisplayRank { get; set; }
    public object TextResources { get; set; }
    public object[] Images { get; set; }
    public LegalText LegalText { get; set; }
    public string Language { get; set; }
    public string[] Markets { get; set; }
}