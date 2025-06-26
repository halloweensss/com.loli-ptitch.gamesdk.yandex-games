using System;
using UnityEngine.Scripting;

namespace GameSDK.Plugins.YaGames.Purchases
{
    [Serializable]
    public class YaProductPurchase
    {
        [field: Preserve] 
        public string productID;
        [field: Preserve] 
        public string purchaseToken;
        [field: Preserve] 
        public string developerPayload;
        [field: Preserve] 
        public string signature;
    }
}