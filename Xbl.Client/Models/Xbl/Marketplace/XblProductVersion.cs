namespace Xbl.Client.Models.Xbl.Marketplace;

public class XblProductVersion
{
    public string Title { get; set; }
    public string ProductId { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string PackageFamilyName { get; set; }
    public string PackageIdentityName { get; set; }
    public string[] XboxConsoleGenOptimized { get; set; }
    public string[] XboxConsoleGenCompatible { get; set; }
    public DateTime? RevisionId { get; set; }
    public DateTime OriginalReleaseDate { get; set; }
}