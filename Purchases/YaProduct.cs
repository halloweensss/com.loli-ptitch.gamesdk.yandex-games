using System;
using UnityEngine.Scripting;

namespace GameSDK.Plugins.YaGames.Purchases
{
    [Serializable]
    public class YaProduct
    {
        [field: Preserve] 
        public string id;
        [field: Preserve] 
        public string title;
        [field: Preserve] 
        public string description;
        [field: Preserve] 
        public string price;
        [field: Preserve] 
        public string priceValue;
        [field: Preserve] 
        public string priceCurrencyCode;
    }
}