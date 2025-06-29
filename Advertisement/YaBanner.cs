using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Advertisement;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Plugins.YaGames.Advertisement
{
    public class YaBanner : IBannerAds
    {
        private static readonly YaBanner Instance = new();

        public string ServiceId => "YaGames_Banner";
        public InitializationStatus InitializationStatus => InitializationStatus.Initialized;
        public event Action<IBannerAds> OnShownBanner;
        public event Action<IBannerAds> OnHiddenBanner;
        public event Action<IBannerAds> OnErrorBanner;

        public Task ShowBanner(BannerPosition position = BannerPosition.None, string placement = null)
        {
#if !UNITY_EDITOR
            YaGamesShowBanner(OnOpen, OnError);
#else
            OnOpen();
#endif
            return Task.CompletedTask;

            [MonoPInvokeCallback(typeof(Action))]
            static void OnOpen()
            {
                Instance.OnShownBanner?.Invoke(Instance);

                if (GameApp.IsDebugMode)
                    Debug.Log("[GameSDK.Advertisement]: YaAdvertisement banner opened!");
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError(int error)
            {
                Instance.OnErrorBanner?.Invoke(Instance);

                var type = (BannerErrors)error;

                if (GameApp.IsDebugMode)
                    Debug.Log(
                        $"[GameSDK.Advertisement]: YaAdvertisement an error occurred while displaying an banner ad with error {type}!");
            }
        }

        public Task HideBanner()
        {
#if !UNITY_EDITOR
            YaGamesHideBanner(OnHided, OnError);
#else
            OnHided();
#endif
            return Task.CompletedTask;

            [MonoPInvokeCallback(typeof(Action))]
            static void OnHided()
            {
                Instance.OnHiddenBanner?.Invoke(Instance);

                if (GameApp.IsDebugMode)
                    Debug.Log("[GameSDK.Advertisement]: YaAdvertisement banner is hidden!");
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError(int error)
            {
                Instance.OnErrorBanner?.Invoke(Instance);

                var type = (BannerErrors)error;

                if (GameApp.IsDebugMode)
                    Debug.Log(
                        $"[GameSDK.Advertisement]: YaAdvertisement an error occurred while hidden an banner ad with error {type}!");
            }
        }

        public void LoadBanner()
        {
        }

        public bool IsLoadedBanner(string placement = null)
        {
            return true;
        }

        public double GetBannerEcpm(string placement = null)
        {
            return 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            Ads.Banner.Register(Instance);
        }

        [DllImport("__Internal")]
        private static extern void YaGamesShowBanner(Action onOpen, Action<int> onError);

        [DllImport("__Internal")]
        private static extern void YaGamesHideBanner(Action onHided, Action<int> onError);
    }
}