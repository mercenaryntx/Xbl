using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class Price
{
    public string CurrencyCode { get; set; }
    public bool IsPIRequired { get; set; }
    public float ListPrice { get; set; }
    public float MSRP { get; set; }
    public string TaxType { get; set; }
    public string WholesaleCurrencyCode { get; set; }
    public float WholesalePrice { get; set; }
}