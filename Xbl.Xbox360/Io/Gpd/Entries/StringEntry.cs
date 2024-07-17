using System.Text;
using Xbl.Xbox360.Extensions;
using Xbl.Xbox360.Models;

namespace Xbl.Xbox360.Io.Gpd.Entries
{
    public class StringEntry : EntryBase
    {
        public string Text
        {
            get { return ByteArrayExtensions.ToTrimmedString(AllBytes, Encoding.BigEndianUnicode); }
            set
            {
                var bytes = Encoding.BigEndianUnicode.GetBytes(value);
                Binary.WriteBytes(StartOffset, bytes, 0, bytes.Length);
            }
        }

        public StringEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}