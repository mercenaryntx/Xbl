using Xbl.Xbox360.Attributes;
using Xbl.Xbox360.Constants;
using Xbl.Xbox360.Models;

namespace Xbl.Xbox360.Io.Stfs.Data
{
    public class LicenseEntry : BinaryModelBase
    {
        [BinaryData]
        public virtual ulong Data { get; set; }

        public LicenseType Type
        {
            get
            {
                var type = (int)(Data >> 48);
                if (!Enum.IsDefined(typeof(LicenseType), type))
                    throw new InvalidDataException("STFS: Invalid license type " + type);
                return (LicenseType) type;
            }
        }

        [BinaryData]
        public virtual uint Bits { get; set; }

        [BinaryData]
        public virtual uint Flags { get; set; } //TODO?

        public LicenseEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}