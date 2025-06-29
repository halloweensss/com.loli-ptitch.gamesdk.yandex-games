using System.Threading.Tasks;
using GameSDK.Advertisement;
using GameSDK.Core;
using GameSDK.Plugins.YaGames.Core;
using UnityEngine;

namespace GameSDK.Plugins.YaGames.Advertisement
{
    public class YaAds : IAdsApp
    {
        private static readonly YaAds Instance = new();
        public string ServiceId => Service.YaGames;
        public InitializationStatus InitializationStatus { get; private set; } = InitializationStatus.None;

        public Task Initialize()
        {
            InitializationStatus = InitializationStatus.Initialized;
            return Task.CompletedTask;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            Ads.Register(Instance);
        }
    }
}