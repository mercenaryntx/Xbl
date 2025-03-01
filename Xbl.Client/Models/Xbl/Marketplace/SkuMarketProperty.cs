namespace Xbl.Client.Models.Xbl.Marketplace;

public class SkuMarketProperty
{
    public string FirstAvailableDate { get; set; }
    public string[] SupportedLanguages { get; set; }
    public object PackageIds { get; set; }
    public object PIFilter { get; set; }
    public string[] Markets { get; set; }
}