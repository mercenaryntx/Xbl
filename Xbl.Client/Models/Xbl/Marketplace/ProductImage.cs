namespace Xbl.Client.Models.Xbl.Marketplace;

public class ProductImage
{
    public string FileId { get; set; }
    public object EISListingIdentifier { get; set; }
    public string BackgroundColor { get; set; }
    public string Caption { get; set; }
    public int FileSizeInBytes { get; set; }
    public string ForegroundColor { get; set; }
    public int Height { get; set; }
    public string ImagePositionInfo { get; set; }
    public string ImagePurpose { get; set; }
    public string UnscaledImageSHA256Hash { get; set; }
    public string Uri { get; set; }
    public int Width { get; set; }
}