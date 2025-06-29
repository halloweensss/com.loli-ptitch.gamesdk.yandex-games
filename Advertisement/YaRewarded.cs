using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Advertisement;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Plugins.YaGames.Advertisement
{
    public class YaRewarded : IRewardedAds
    {
        private static readonly YaRewarded Instance = new();
        public string ServiceId => "YaGames_Rewarded";
        public InitializationStatus InitializationStatus => InitializationStatus.Initialized;

        public event Action<IRewardedAds> OnShownRewarded;
        public event Action<IRewardedAds> OnClosedRewarded;
        public event Action<IRewardedAds> OnShownFailedRewarded;
        public event Action<IRewardedAds> OnErrorRewarded;
        public event Action<IRewardedAds> OnClickedRewarded;
        public event Action<IRewardedAds> OnRewardedRewarded;
        public event Action<IRewardedAds> OnLoadedRewarded;
        public event Action<IRewardedAds> OnFailedToLoadRewarded;

        public Task ShowRewarded(string placement = null)
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

        public void LoadRewarded()
        {
        }

        public bool IsLoadedRewarded(string placement = null)
        {
            return true;
        }

        public double GetRewardedEcpm(string placement = null)
        {
            return 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            Ads.Rewarded.Register(Instance);
        }

        [DllImport("__Internal")]
        private static extern void YaGamesShowRewarded(Action onOpen, Action onClose, Action<string> onError,
            Action onRewarded);
    }
}