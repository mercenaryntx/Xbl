using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class PlatformDependency
{
    public long MaxTested { get; set; }
    public long MinVersion { get; set; }
    public string PlatformName { get; set; }
}