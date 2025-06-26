using UnityEngine.Scripting;

namespace GameSDK.Plugins.YaGames.Core
{
    [System.Serializable]
    public class YaEnvironment
    {
        [field: Preserve]
        public YaApp app;
        [field: Preserve]
        public YaBrowser browser;
        [field: Preserve]
        public YaI18n i18n;
        [field: Preserve]
        public string payload;
    }
    
    [System.Serializable]
    public class YaApp
    {
        [field: Preserve]
        public string id;
    }
    
    [System.Serializable]
    public class YaBrowser
    {
        [field: Preserve]
        public string lang;
    }
    
    [System.Serializable]
    public class YaI18n
    {
        [field: Preserve]
        public string lang;
        [field: Preserve]
        public string tld;
    }
}