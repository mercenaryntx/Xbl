﻿using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class PreviewImage
{
    public string FileId { get; set; }
    public object EISListingIdentifier { get; set; }
    public object BackgroundColor { get; set; }
    public string Caption { get; set; }
    public int FileSizeInBytes { get; set; }
    public object ForegroundColor { get; set; }
    public int Height { get; set; }
    public object ImagePositionInfo { get; set; }
    public string ImagePurpose { get; set; }
    public string UnscaledImageSHA256Hash { get; set; }
    public string Uri { get; set; }
    public int Width { get; set; }
}