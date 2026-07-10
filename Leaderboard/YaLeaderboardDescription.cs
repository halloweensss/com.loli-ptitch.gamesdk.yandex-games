using System;
using UnityEngine.Scripting;

namespace GameSDK.Plugins.YaGames.Leaderboard
{
    [Serializable]
    public class YaLeaderboardDescription
    {
        [field: Preserve]
        public string appID;
        [field: Preserve]
        public bool @default;
        [field: Preserve]
        public Description description;
        [field: Preserve]
        public string name;
        [field: Preserve]
        public Title title;
        [field: Preserve]
        public LocalizedTitle[] localizedTitles;

        [Serializable]
        public class Description
        {
            [field: Preserve]
            public bool invert_sort_order;
            [field: Preserve]
            public ScoreFormat score_format;
            [field: Preserve]
            public string sort_order;
        }

        [Serializable]
        public class ScoreFormat
        {
            [field: Preserve]
            public Options options;
            [field: Preserve]
            public string type;
        }

        [Serializable]
        public class Options
        {
            [field: Preserve]
            public int decimal_offset;
        }

        [Serializable]
        public class Title
        {
            [field: Preserve]
            public string en;
            [field: Preserve]
            public string ru;
        }

        [Serializable]
        public class LocalizedTitle
        {
            [field: Preserve]
            public string locale;
            [field: Preserve]
            public string value;
        }
    }
}
