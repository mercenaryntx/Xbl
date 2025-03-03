using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class AllowedPlatform
{
    public int MaxVersion { get; set; }
    public int MinVersion { get; set; }
    public string PlatformName { get; set; }
}