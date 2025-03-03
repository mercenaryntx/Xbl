using System.Text.Json.Serialization;
using Xbl.Data;

namespace Xbl.Client.Models.Dbox;

[Database(DataSource.Dbox)]
public class Product : IHaveId
{
    [JsonIgnore]
    public int Id { get; set; }

    public string TitleId { get; set; }
    public string Title { get; set; }
    public string DeveloperName { get; set; }
    public string PublisherName { get; set; }
    public string ProductGroupId { get; set; }
    public Dictionary<string, ProductVersion> Versions { get; set; }
    public string Category { get; set; }
}