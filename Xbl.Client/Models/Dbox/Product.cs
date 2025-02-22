namespace Xbl.Client.Models.Dbox;

public class Product
{
    public string TitleId { get; set; }
    public string Title { get; set; }
    public string DeveloperName { get; set; }
    public string PublisherName { get; set; }
    public string ProductGroupId { get; set; }
    public Dictionary<string, ProductVersion> Versions { get; set; }
    public string Category { get; set; }
}