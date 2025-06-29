using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Advertisement;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Plugins.YaGames.Advertisement
{
    public class YaInterstitial : IInterstitialAds
    {
        private static readonly YaInterstitial Instance = new();

        public string ServiceId => "YaGames_Interstitial";
        public InitializationStatus InitializationStatus => InitializationStatus.Initialized;

        public event Action<IInterstitialAds> OnShownInterstitial;
        public event Action<IInterstitialAds> OnClosedInterstitial;
        public event Action<IInterstitialAds> OnShownFailedInterstitial;
        public event Action<IInterstitialAds> OnErrorInterstitial;
        public event Action<IInterstitialAds> OnClickedInterstitial;
        public event Action<IInterstitialAds> OnLoadedInterstitial;
        public event Action<IInterstitialAds> OnFailedToLoadInterstitial;

        public Task ShowInterstitial(string placement = null)
        {
#if !UNITY_EDITOR
            YaGamesShowInterstitial(OnOpen, OnClose, OnError, OnOffline);
#else
            OnOpen();
            OnClose(true);
#endif
            return Task.CompletedTask;

            [MonoPInvokeCallback(typeof(Action))]
            static void OnOpen()
            {
                Instance.OnShownInterstitial?.Invoke(Instance);

                if (GameApp.IsDebugMode)
                    Debug.Log("[GameSDK.Advertisement]: YaAdvertisement interstitial opened!");
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnClose(bool wasShown)
            {
                if (wasShown)
                {
                    Instance.OnClosedInterstitial?.Invoke(Instance);

                    if (GameApp.IsDebugMode)
                        Debug.Log("[GameSDK.Advertisement]: YaAdvertisement interstitial closed!");
                }
                else
                {
                    Instance.OnShownFailedInterstitial?.Invoke(Instance);

                    if (GameApp.IsDebugMode)
                        Debug.Log("[GameSDK.Advertisement]: YaAdvertisement an error occurred while displaying an ad!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError(string error)
            {
                Instance.OnErrorInterstitial?.Invoke(Instance);

                if (GameApp.IsDebugMode)
                    Debug.Log(
                        $"[GameSDK.Advertisement]: YaAdvertisement an error occurred while displaying an ad with error {error}!");
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnOffline()
            {
                Instance.OnErrorInterstitial?.Invoke(Instance);

                if (GameApp.IsDebugMode)
                    Debug.Log(
                        "[GameSDK.Advertisement]: YaAdvertisement an error occurred while displaying an ad, the player switched to offline mode!");
            }
        }

        public void LoadInterstitial()
        {
        }

        public bool IsLoadedInterstitial(string placement = null)
        {
            return true;
        }

        public double GetInterstitialEcpm(string placement = null)
        {
            return 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            Ads.Interstitial.Register(Instance);
        }

        [DllImport("__Internal")]
        private static extern void YaGamesShowInterstitial(Action onOpen, Action<bool> onClose, Action<string> onError,
            Action onOffline);
    }
}