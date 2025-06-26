using System;
using UnityEngine.Scripting;

namespace GameSDK.Plugins.YaGames.Purchases
{
    [Serializable]
    public class YaPurchasesCatalog
    {
        [field: Preserve] 
        public YaProductPurchase[] purchases;
    }
}