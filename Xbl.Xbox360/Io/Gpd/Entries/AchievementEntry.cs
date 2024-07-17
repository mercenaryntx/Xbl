using Xbl.Xbox360.Attributes;
using Xbl.Xbox360.Constants;
using Xbl.Xbox360.Models;

namespace Xbl.Xbox360.Io.Gpd.Entries
{
    public class AchievementEntry : EntryBase
    {
        [BinaryData]
        public virtual uint Size { get; set; }

        [BinaryData]
        public virtual int AchievementId { get; set; }

        [BinaryData]
        public virtual uint ImageId { get; set; }

        [BinaryData]
        public virtual int Gamerscore { get; set; }

        [BinaryData]
        public virtual AchievementLockFlags Flags { get; set; }

        [BinaryData]
        public virtual DateTime UnlockTime { get; set; }

        [BinaryData(StringReadOptions.NullTerminated)]
        public virtual string Name { get; set; }

        [BinaryData(StringReadOptions.NullTerminated)]
        public virtual string UnlockedDescription { get; set; }

        [BinaryData(StringReadOptions.NullTerminated)]
        public virtual string LockedDescription { get; set; }

        public bool IsUnlocked
        {
            get { return Flags.HasFlag(AchievementLockFlags.Unlocked) || Flags.HasFlag(AchievementLockFlags.UnlockedOnline); }
        }

        public bool IsSecret
        {
            get { return !Flags.HasFlag(AchievementLockFlags.Visible); }
        }

        public AchievementEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}