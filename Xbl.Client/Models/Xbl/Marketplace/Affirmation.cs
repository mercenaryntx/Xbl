using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class Affirmation
{
    public string AffirmationId { get; set; }
    public string AffirmationProductId { get; set; }
    public string Description { get; set; }
}