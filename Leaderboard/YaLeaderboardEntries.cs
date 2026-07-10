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
        public string userRank;
        [field: Preserve]
        public YaLeaderboardPlayerData[] entries;
    }

    [Serializable]
    public class YaLeaderboardRanges
    {
        [field: Preserve]
        public string start;
        [field: Preserve]
        public string size;

        public YaLeaderboardRanges()
        {
        }

        public YaLeaderboardRanges(string size, string start)
        {
            this.size = size;
            this.start = start;
        }
    }
}
