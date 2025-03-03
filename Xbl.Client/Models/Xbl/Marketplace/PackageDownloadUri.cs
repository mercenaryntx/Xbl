using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class PackageDownloadUri
{
    public int Rank { get; set; }
    public string Uri { get; set; }
}