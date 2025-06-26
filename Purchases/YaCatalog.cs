using System;
using UnityEngine.Scripting;

namespace GameSDK.Plugins.YaGames.Purchases
{
    [Serializable]
    public class YaCatalog
    {
        [field: Preserve] 
        public YaProduct[] products;
    }
}