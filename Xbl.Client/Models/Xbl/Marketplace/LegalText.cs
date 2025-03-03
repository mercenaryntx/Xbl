using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class LegalText
{
    public string AdditionalLicenseTerms { get; set; }
    public string Copyright { get; set; }
    public string CopyrightUri { get; set; }
    public string PrivacyPolicy { get; set; }
    public string PrivacyPolicyUri { get; set; }
    public string Tou { get; set; }
    public string TouUri { get; set; }
}