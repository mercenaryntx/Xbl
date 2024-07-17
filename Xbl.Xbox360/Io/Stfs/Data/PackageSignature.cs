using Xbl.Xbox360.Attributes;
using Xbl.Xbox360.Models;

namespace Xbl.Xbox360.Io.Stfs.Data
{
    public class PackageSignature : BinaryModelBase, IPackageSignature
    {
        [BinaryData(0x100)]
        public virtual byte[] Signature { get; set; }

        public PackageSignature(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}