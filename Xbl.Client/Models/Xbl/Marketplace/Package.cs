using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class Package
{
    public Application[] Applications { get; set; }
    public string[] Architectures { get; set; }
    public string[] Capabilities { get; set; }
    public object[] DeviceCapabilities { get; set; }
    public object[] ExperienceIds { get; set; }
    public FrameworkDependency[] FrameworkDependencies { get; set; }
    public string[] HardwareDependencies { get; set; }
    public object[] HardwareRequirements { get; set; }
    public string Hash { get; set; }
    public string HashAlgorithm { get; set; }
    public bool IsStreamingApp { get; set; }
    public string[] Languages { get; set; }
    public long MaxDownloadSizeInBytes { get; set; }
    public long? MaxInstallSizeInBytes { get; set; }
    public string PackageFormat { get; set; }
    public string PackageFamilyName { get; set; }
    public object MainPackageFamilyNameForDlc { get; set; }
    public string PackageFullName { get; set; }
    public string PackageId { get; set; }
    public string ContentId { get; set; }
    public string KeyId { get; set; }
    public int PackageRank { get; set; }
    public string PackageUri { get; set; }
    public PlatformDependency[] PlatformDependencies { get; set; }
    public string PlatformDependencyXmlBlob { get; set; }
    public string ResourceId { get; set; }
    public string Version { get; set; }
    public PackageDownloadUri[] PackageDownloadUris { get; set; }
    public object[] DriverDependencies { get; set; }
    public PackageFulfillmentData FulfillmentData { get; set; }
}