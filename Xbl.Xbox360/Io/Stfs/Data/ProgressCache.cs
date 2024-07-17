using Xbl.Xbox360.Attributes;
using Xbl.Xbox360.Constants;
using Xbl.Xbox360.Models;

namespace Xbl.Xbox360.Io.Stfs.Data
{
    public class ProgressCache : BinaryModelBase, IInstallerInformation
    {
        [BinaryData(4)]
        public virtual OnlineContentResumeState ResumeState { get; set; }

        [BinaryData]
        public virtual uint CurrentFileIndex { get; set; }

        [BinaryData]
        public virtual uint CurrentFileOffset { get; set; }

        [BinaryData]
        public virtual long ByteProcessed { get; set; }

        [BinaryData]
        public virtual DateTime LastModified { get; set; }

        [BinaryData(0x15D0)]
        public virtual byte[] CabResumeData { get; set; }

        public ProgressCache(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}