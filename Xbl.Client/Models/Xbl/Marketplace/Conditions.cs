namespace Xbl.Client.Models.Xbl.Marketplace;

public class Conditions
{
    public ClientConditions ClientConditions { get; set; }
    public DateTime EndDate { get; set; }
    public string[] ResourceSetIds { get; set; }
    public DateTime StartDate { get; set; }
    public string[] EligibilityPredicateIds { get; set; }
    public int SupportedCatalogVersion { get; set; }
}