using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class ProductVideo
{
    public string Uri { get; set; }
    public string VideoPurpose { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public string AudioEncoding { get; set; }
    public string VideoEncoding { get; set; }
    public string VideoPositionInfo { get; set; }
    public string Caption { get; set; }
    public int FileSizeInBytes { get; set; }
    public PreviewImage PreviewImage { get; set; }
    public string TrailerId { get; set; }
    public object SortOrder { get; set; }
}