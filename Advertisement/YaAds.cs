using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Advertisement;
using GameSDK.Core;
using GameSDK.Plugins.YaGames.Core;
using UnityEngine;

namespace GameSDK.Plugins.YaGames.Advertisement
{
    public class YaAds : IAdsApp, IInterstitialAds, IRewardedAds, IBannerAds
    {
        private static readonly YaAds Instance = new();

        public string ServiceId => Service.YaGames;
        public InitializationStatus InitializationStatus { get; private set; } = InitializationStatus.None;

        public Task Initialize()
        {
            InitializationStatus = InitializationStatus.Initialized;
            return Task.CompletedTask;
        }

        public Task ShowInterstitial()
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

        public Task ShowRewarded()
        {
#if !UNITY_EDITOR
            YaGamesShowRewarded(OnOpen, OnClose, OnError, OnRewarded);
#else
            OnOpen();
            OnRewarded();
            OnClose();
#endif
            return Task.CompletedTask;

            [MonoPInvokeCallback(typeof(Action))]
            static void OnOpen()
            {
                Instance.OnShownRewarded?.Invoke(Instance);

                if (GameApp.IsDebugMode)
                    Debug.Log("[GameSDK.Advertisement]: YaAdvertisement rewarded opened!");
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnClose()
            {
                Instance.OnClosedRewarded?.Invoke(Instance);

                if (GameApp.IsDebugMode)
                    Debug.Log("[GameSDK.Advertisement]: YaAdvertisement rewarded closed!");
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError(string error)
            {
                Instance.OnErrorRewarded?.Invoke(Instance);

                if (GameApp.IsDebugMode)
                    Debug.Log(
                        $"[GameSDK.Advertisement]: YaAdvertisement an error occurred while displaying an rewarded ad with error {error}!");
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnRewarded()
            {
                Instance.OnRewardedRewarded?.Invoke(Instance);

                if (GameApp.IsDebugMode)
                    Debug.Log("[GameSDK.Advertisement]: YaAdvertisement you can get a reward for watching a video!");
            }
        }

        public Task ShowBanner()
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

        public event Action<IBannerAds> OnShownBanner;
        public event Action<IBannerAds> OnHiddenBanner;
        public event Action<IBannerAds> OnErrorBanner;

        public event Action<IInterstitialAds> OnShownInterstitial;
        public event Action<IInterstitialAds> OnClosedInterstitial;
        public event Action<IInterstitialAds> OnShownFailedInterstitial;
        public event Action<IInterstitialAds> OnErrorInterstitial;
        public event Action<IInterstitialAds> OnClickedInterstitial;

        public event Action<IRewardedAds> OnShownRewarded;
        public event Action<IRewardedAds> OnClosedRewarded;
        public event Action<IRewardedAds> OnShownFailedRewarded;
        public event Action<IRewardedAds> OnErrorRewarded;
        public event Action<IRewardedAds> OnClickedRewarded;
        public event Action<IRewardedAds> OnRewardedRewarded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            Ads.Register(Instance);
            Ads.Interstitial.Register(Instance);
            Ads.Rewarded.Register(Instance);
            Ads.Banner.Register(Instance);
        }

        [DllImport("__Internal")]
        private static extern void YaGamesShowInterstitial(Action onOpen, Action<bool> onClose, Action<string> onError,
            Action onOffline);

        [DllImport("__Internal")]
        private static extern void YaGamesShowRewarded(Action onOpen, Action onClose, Action<string> onError,
            Action onRewarded);

        [DllImport("__Internal")]
        private static extern void YaGamesShowBanner(Action onOpen, Action<int> onError);

        [DllImport("__Internal")]
        private static extern void YaGamesHideBanner(Action onHided, Action<int> onError);
    }
}