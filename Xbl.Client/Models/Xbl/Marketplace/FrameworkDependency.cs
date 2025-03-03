using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class FrameworkDependency
{
    public int MaxTested { get; set; }
    public long MinVersion { get; set; }
    public string PackageIdentity { get; set; }
}