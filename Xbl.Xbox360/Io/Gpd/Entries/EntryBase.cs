using Xbl.Xbox360.Models;

namespace Xbl.Xbox360.Io.Gpd.Entries
{
    public class EntryBase : BinaryModelBase, IComparable
    {
        public XdbfEntry Entry { get; set; }

        public byte[] AllBytes
        {
            get { return Binary.ReadBytes(StartOffset, BinarySize); }
        }

        public EntryBase(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }

        public virtual int CompareTo(object obj)
        {
            var other = obj as EntryBase;
            return other == null ? 1 : Entry.Id.CompareTo(other.Entry.Id);
        }
    }
}