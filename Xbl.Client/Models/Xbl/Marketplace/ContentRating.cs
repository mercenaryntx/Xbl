namespace Xbl.Client.Models.Xbl.Marketplace;

public class ContentRating
{
    public string RatingSystem { get; set; }
    public string RatingId { get; set; }
    public string[] RatingDescriptors { get; set; }
    public string[] RatingDisclaimers { get; set; }
    public string[] InteractiveElements { get; set; }
}