namespace Xbl.Xbox360.Io.Gpd.Entries
{
    public class TitleComparer : IComparer<TitleEntry>
    {
        public static TitleComparer Instance { get; private set; }

        static TitleComparer()
        {
            Instance = new TitleComparer();
        }

        private TitleComparer()
        {

        }

        public int Compare(TitleEntry x, TitleEntry y)
        {
            return x.LastAchievementEarnedOn.CompareTo(y.LastAchievementEarnedOn);
        }
    }
}