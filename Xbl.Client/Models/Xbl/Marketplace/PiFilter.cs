using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class PiFilter
{
    public object[] ExclusionProperties { get; set; }
    public object[] InclusionProperties { get; set; }
}