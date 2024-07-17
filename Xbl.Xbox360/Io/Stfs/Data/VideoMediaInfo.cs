using Xbl.Xbox360.Attributes;
using Xbl.Xbox360.Models;

namespace Xbl.Xbox360.Io.Stfs.Data
{
    public class VideoMediaInfo : BinaryModelBase, IMediaInfo
    {
        [BinaryData(0x10)]
        public byte[] SeriesId { get; set; }

        [BinaryData(0x10)]
        public byte[] SeasonId { get; set; }

        [BinaryData]
        public ushort SeasonNumber { get; set; }

        [BinaryData]
        public ushort EpisodeNumber { get; set; }

        public VideoMediaInfo(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}