using Xbl.Xbox360.Attributes;
using Xbl.Xbox360.Io.Stfs.Data;
using Xbl.Xbox360.Models;

namespace Xbl.Xbox360.Io.Stfs
{
    public class HashTable : BinaryModelBase
    {
        [BinaryData(0xAA)]
        public virtual HashEntry[] Entries { get; set; }

        [BinaryData]
        public virtual int AllocatedBlockCount { get; set; }

        public int Block { get; set; }

        public int EntryCount { get; set; }

        public List<HashTable> Tables { get; set; }

        public HashTable(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}