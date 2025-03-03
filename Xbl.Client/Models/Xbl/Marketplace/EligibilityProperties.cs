using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class EligibilityProperties
{
    public Remediation[] Remediations { get; set; }
    public Affirmation[] Affirmations { get; set; }
}