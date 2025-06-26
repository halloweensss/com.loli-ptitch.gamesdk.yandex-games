using System;
using UnityEngine.Scripting;

namespace GameSDK.Plugins.YaGames.Leaderboard
{
    [Serializable]
    public class YaLeaderboardEntries
    {
        [field: Preserve]
        public YaLeaderboardDescription leaderboard;
        [field: Preserve]
        public YaLeaderboardRanges[] ranges;
        [field: Preserve]
        public int userRank;
        [field: Preserve]
        public YaLeaderboardPlayerData[] entries;
    }

    [Serializable]
    public class YaLeaderboardRanges
    {
        [field: Preserve]
        public int start;
        [field: Preserve]
        public int size;

        public YaLeaderboardRanges()
        {
        }

        public YaLeaderboardRanges(int size, int start)
        {
            this.size = size;
            this.start = start;
        }
    }
}