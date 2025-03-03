using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class UsageData
{
    public string AggregateTimeSpan { get; set; }
    public float AverageRating { get; set; }
    public int PlayCount { get; set; }
    public int RatingCount { get; set; }
    public string RentalCount { get; set; }
    public string TrialCount { get; set; }
    public string PurchaseCount { get; set; }
}