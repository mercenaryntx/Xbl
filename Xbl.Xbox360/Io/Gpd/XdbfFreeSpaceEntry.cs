using Xbl.Xbox360.Attributes;
using Xbl.Xbox360.Models;

namespace Xbl.Xbox360.Io.Gpd
{
    public class XdbfFreeSpaceEntry : BinaryModelBase
    {
        [BinaryData]
        public virtual int AddressSpecifier { get; set; }

        [BinaryData]
        public virtual int Length { get; set; }

        public XdbfFreeSpaceEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}