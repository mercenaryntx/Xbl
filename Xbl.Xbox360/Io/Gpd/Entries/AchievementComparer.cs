namespace Xbl.Xbox360.Io.Gpd.Entries
{
    public class AchievementComparer : IComparer<AchievementEntry>
    {
        public static AchievementComparer Instance { get; private set; }

        static AchievementComparer()
        {
            Instance = new AchievementComparer();
        }

        private AchievementComparer()
        {

        }

        public int Compare(AchievementEntry x, AchievementEntry y)
        {
            if (x.IsUnlocked) return y.IsUnlocked ? x.UnlockTime.CompareTo(y.UnlockTime) : 1;
            return y.IsUnlocked ? -1 : -1 * x.CompareTo(y);
        }
    }
}