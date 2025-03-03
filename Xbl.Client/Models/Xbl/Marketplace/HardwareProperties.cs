using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class HardwareProperties
{
    public string[] MinimumHardware { get; set; }
    public string[] RecommendedHardware { get; set; }
    public string MinimumProcessor { get; set; }
    public string RecommendedProcessor { get; set; }
    public string MinimumGraphics { get; set; }
    public string RecommendedGraphics { get; set; }
}