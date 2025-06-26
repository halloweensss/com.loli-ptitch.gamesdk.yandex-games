using System;
using UnityEngine.Scripting;

namespace GameSDK.Plugins.YaGames.Leaderboard
{
    [Serializable]
    public class YaLeaderboardPlayerData
    {
        [field: Preserve]
        public int score;
        [field: Preserve]
        public string extraData;
        [field: Preserve]
        public int rank;
        [field: Preserve]
        public Player player;
        [field: Preserve]
        public string formattedScore;

        public YaLeaderboardPlayerData()
        {
        }

        public YaLeaderboardPlayerData(string extraData, string formattedScore, Player player, int rank, int score)
        {
            this.extraData = extraData;
            this.formattedScore = formattedScore;
            this.player = player;
            this.rank = rank;
            this.score = score;
        }
    }
    
    [Serializable]
    public class Player
    {
        [field: Preserve]
        public string lang;
        [field: Preserve]
        public string publicName;
        [field: Preserve]
        public ScopePermissions scopePermissions;
        [field: Preserve]
        public string uniqueID;

        public Player()
        {
        }

        public Player(string lang, string publicName, ScopePermissions scopePermissions, string uniqueID)
        {
            this.lang = lang;
            this.publicName = publicName;
            this.scopePermissions = scopePermissions;
            this.uniqueID = uniqueID;
        }
    }

    [Serializable]
    public class ScopePermissions
    {
        [field: Preserve]
        public string avatar;
        [field: Preserve]
        public string public_name;

        public ScopePermissions()
        {
        }

        public ScopePermissions(string avatar, string publicName)
        {
            this.avatar = avatar;
            public_name = publicName;
        }
    }
}