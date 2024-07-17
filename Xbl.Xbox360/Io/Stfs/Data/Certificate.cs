using Xbl.Xbox360.Attributes;
using Xbl.Xbox360.Constants;
using Xbl.Xbox360.Models;

namespace Xbl.Xbox360.Io.Stfs.Data
{
    public class Certificate : BinaryModelBase, IPackageSignature
    {
        [BinaryData]
        public virtual ushort PublicKeyCertificateSize { get; set; }

        [BinaryData(0x5)]
        public virtual byte[] OwnerConsoleId { get; set; }

        [BinaryData(0x11, "ascii", StringReadOptions.AutoTrim)]
        public virtual string OwnerConsolePartNumber { get; set; }

        [BinaryData(0x1)]
        public virtual ConsoleTypeFlags ConsoleTypeFlags { get; set; }

        [BinaryData(0x3)]
        public virtual ConsoleType OwnerConsoleType { get; set; }

        [BinaryData(0x8, "ascii")]
        public virtual string DateGeneration { get; set; }

        [BinaryData]
        public virtual uint PublicExponent { get; set; }

        [BinaryData(0x80)]
        public virtual byte[] PublicModulus { get; set; }

        [BinaryData(0x100)]
        public virtual byte[] CertificateSignature { get; set; }

        [BinaryData(0x80)]
        public virtual byte[] Signature { get; set; }

        public Certificate(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}