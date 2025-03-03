using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class AlternateId
{
    public string IdType { get; set; }
    public string Value { get; set; }
}