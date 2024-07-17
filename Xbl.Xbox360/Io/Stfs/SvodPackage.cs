using Xbl.Xbox360.Io.Stfs.Data;
using Xbl.Xbox360.Models;

namespace Xbl.Xbox360.Io.Stfs
{
    public abstract class SvodPackage : Package<SvodVolumeDescriptor>
    {
        protected SvodPackage(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }

        protected override void Parse()
        {
        }

        public override void Rehash()
        {
            throw new NotImplementedException();
        }


    }
}