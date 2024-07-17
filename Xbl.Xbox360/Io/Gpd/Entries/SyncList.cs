using Xbl.Xbox360.Models;

namespace Xbl.Xbox360.Io.Gpd.Entries
{
    public class SyncList : List<SyncEntry>
    {
        public XdbfEntry Entry { get; set; }

        public BinaryContainer Binary { get; set; }

        public byte[] AllBytes
        {
            get { return Binary.ReadAll(); }
        }
    }

}