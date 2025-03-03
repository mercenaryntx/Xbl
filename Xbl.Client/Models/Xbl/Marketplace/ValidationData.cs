using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class ValidationData
{
    public bool PassedValidation { get; set; }
    public string RevisionId { get; set; }
    public string ValidationResultUri { get; set; }
}