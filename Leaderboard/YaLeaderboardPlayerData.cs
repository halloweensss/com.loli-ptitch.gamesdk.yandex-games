using System;
using UnityEngine.Scripting;

namespace GameSDK.Plugins.YaGames.Leaderboard
{
    [Serializable]
    public class YaLeaderboardPlayerData
    {
        [field: Preserve]
        public string score;
        [field: Preserve]
        public string extraData;
        [field: Preserve]
        public string rank;
        [field: Preserve]
        public Player player;
        public YaLeaderboardPlayerData()
        {
        }

        public YaLeaderboardPlayerData(string extraData, Player player, string rank, string score)
        {
            this.extraData = extraData;
            this.player = player;
            this.rank = rank;
            this.score = score;
        }
    }
    
    [Serializable]
    public class Player
    {
        [field: Preserve]
        public string publicName;
        [field: Preserve]
        public string uniqueID;
        [field: Preserve]
        public string avatarUrl;

        public Player()
        {
        }

        public Player(string publicName, string uniqueID, string avatarUrl = null)
        {
            this.publicName = publicName;
            this.uniqueID = uniqueID;
            this.avatarUrl = avatarUrl;
        }
    }
}
