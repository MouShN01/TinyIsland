using TinyIsland.Core;

namespace TinyIsland.Tide
{
    public static class MiniTideController
    {
        public static float GetLowLevel(DayConfig dayConfig, int miniTideIndex)
        {
            return dayConfig.FirstLowTideLevel;
        }

        public static float GetPeakLevel(DayConfig dayConfig, int miniTideIndex)
        {
            return GetLowLevel(dayConfig, miniTideIndex) + dayConfig.MiniTideHeight;
        }
    }
}
