using Xbl.Xbox360.Models;

namespace Xbl.Xbox360.Io.Stfs.Data
{
    public interface IPackageSignature : IBinaryModel
    {
        byte[] Signature { get; set; }
    }
}