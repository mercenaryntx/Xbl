namespace Xbl.Client.Models.Xbl.Marketplace;

public class ProductMarketProperty
{
    public DateTime OriginalReleaseDate { get; set; }
    public int MinimumUserAge { get; set; }
    public ContentRating[] ContentRatings { get; set; }
    public RelatedProduct[] RelatedProducts { get; set; }
    public UsageData[] UsageData { get; set; }
    public object BundleConfig { get; set; }
    public string[] Markets { get; set; }
}