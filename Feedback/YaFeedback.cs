using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Authentication;
using GameSDK.Core;
using GameSDK.GameFeedback;
using GameSDK.Plugins.YaGames.Core;
using UnityEngine;

namespace GameSDK.Plugins.YaGames.Feedback
{
    public class YaFeedback : IFeedbackApp
    {
        private static readonly YaFeedback Instance = new();

        private YaFailReviewReason _failReviewReason;
        private bool _result;
        private ReviewStatus _status;
        public static ReviewStatus ReviewStatus => Instance._status;
        public string ServiceId => Service.YaGames;
        public InitializationStatus InitializationStatus => InitializationStatus.Initialized;

        public async Task<(bool, FailReviewReason)> CanReview()
        {
            _status = ReviewStatus.Waiting;
            YaGamesCanReview(OnSuccess, OnError);

            while (_status == ReviewStatus.Waiting)
                await Task.Yield();

            return (_result, GetReason(_failReviewReason));

            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                Instance._result = true;
                Instance._failReviewReason = YaFailReviewReason.Unknown;
                Instance._status = ReviewStatus.Success;

                if (GameApp.IsDebugMode)
                    Debug.Log("[GameSDK.Feedback]: YaFeedback review possible!");
            }

            [MonoPInvokeCallback(typeof(Action<int>))]
            static void OnError(int reason)
            {
                Instance._result = false;
                Instance._failReviewReason = (YaFailReviewReason)reason;
                Instance._status = ReviewStatus.Error;

                if (GameApp.IsDebugMode)
                    Debug.Log("[GameSDK.Feedback]: YaFeedback review impossible!");
            }
        }

        public async Task<(bool, FailReviewReason)> RequestReview()
        {
            var canReview = await CanReview();

            if (canReview.Item1 == false)
            {
                if (canReview.Item2 == FailReviewReason.NoAuth)
                {
                    await Auth.SignIn();

                    if (Auth.IsAuthorized)
                    {
                        canReview = await CanReview();

                        if (canReview.Item1 == false)
                            return canReview;
                    }
                    else
                    {
                        if (GameApp.IsDebugMode)
                            Debug.Log("[GameSDK.Feedback]: Before leaving a review, log in YaFeedback!");

                        return canReview;
                    }
                }
                else
                {
                    return canReview;
                }
            }

            _status = ReviewStatus.Waiting;
            YaGamesRequestReview(OnSuccess, OnError);

            while (_status == ReviewStatus.Waiting)
                await Task.Yield();

            return (_result, GetReason(_failReviewReason));

            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                Instance._result = true;
                Instance._failReviewReason = YaFailReviewReason.Unknown;
                Instance._status = ReviewStatus.Success;

                if (GameApp.IsDebugMode)
                    Debug.Log("[GameSDK.Feedback]: YaFeedback review has been delivered!");
            }

            [MonoPInvokeCallback(typeof(Action<int>))]
            static void OnError(int reason)
            {
                Instance._result = false;
                Instance._failReviewReason = (YaFailReviewReason)reason;
                Instance._status = ReviewStatus.Error;

                if (GameApp.IsDebugMode)
                    Debug.Log("[GameSDK.Feedback]: YaFeedback review was not submitted!");
            }
        }

        private FailReviewReason GetReason(YaFailReviewReason reason)
        {
            return reason switch
            {
                YaFailReviewReason.Unknown => FailReviewReason.Unknown,
                YaFailReviewReason.NoAuth => FailReviewReason.NoAuth,
                YaFailReviewReason.GameRated => FailReviewReason.GameRated,
                YaFailReviewReason.ReviewAlreadyRequested => FailReviewReason.Unknown,
                YaFailReviewReason.ReviewWasRequested => FailReviewReason.Unknown,
                YaFailReviewReason.Canceled => FailReviewReason.Canceled,
                _ => FailReviewReason.Unknown
            };
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            GameFeedback.Feedback.Register(Instance);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void YaGamesCanReview(Action onSuccess, Action<int> onError);

        [DllImport("__Internal")]
        private static extern void YaGamesRequestReview(Action onSuccess, Action<int> onError);
#else
        private static void YaGamesCanReview(Action onSuccess, Action<int> onError) => onSuccess?.Invoke();

        private static void YaGamesRequestReview(Action onSuccess, Action<int> onError) => onSuccess.Invoke();
#endif
    }
}