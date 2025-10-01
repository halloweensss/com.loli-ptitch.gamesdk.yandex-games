using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Authentication;
using GameSDK.Core;
using GameSDK.GameStorage;
using GameSDK.Plugins.YaGames.Core;
using GameSDK.Plugins.YaGames.Extension;
using UnityEngine;

namespace GameSDK.Plugins.YaGames.PlayerData
{
    public class YaPlayerData : IAuthApp, IStorageApp
    {
        private static readonly YaPlayerData Instance = new();
        private InitializationStatus _status = InitializationStatus.None;
        private StorageStatus _lastStorageStatus = StorageStatus.None;
        private string _lastStorageData = string.Empty;
        private SignInType _signInType = SignInType.None;
        private string _id = string.Empty;
        private string _name = string.Empty;
        private PayingStatusType _payingStatus = PayingStatusType.None;
        private Coroutine _coroutineDelayedSave = null;
        private readonly Dictionary<AvatarSizeType, string> _avatars = new(4);

        private static readonly Dictionary<string, PayingStatusType> PayingStatuses = new()
        {
            { "unknown", PayingStatusType.Unknown },
            { "not_paying", PayingStatusType.Paying },
            { "partially_paying", PayingStatusType.PartiallyPaying },
            { "paying", PayingStatusType.Paying }
        };

        private static readonly Dictionary<AvatarSizeType, string> AvatarSizes = new()
        {
            { AvatarSizeType.Small, "small" },
            { AvatarSizeType.Medium, "medium" },
            { AvatarSizeType.Large, "large" }
        };

        public string ServiceId => Service.YaGames;
        public InitializationStatus InitializationStatus => _status;

        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                {
                    InitializeId();
                }

                return _id;
            }
        }

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    InitializeName();
                }

                return _name;
            }
        }

        public SignInType SignInType
        {
            get
            {
                if (_signInType == SignInType.None && _status == InitializationStatus.Initialized)
                {
                    InitializeMode();
                }

                return _signInType;
            }
        }

        public PayingStatusType PayingStatus
        {
            get
            {
                if (_payingStatus == PayingStatusType.None && _status == InitializationStatus.Initialized)
                {
                    InitializePayingStatus();
                }

                return _payingStatus;
            }
        }

        public async Task<string> GetAvatar(AvatarSizeType size)
        {
            if (_status != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Authentication]: YaPlayerData is not initialized!");
                }

                return string.Empty;
            }

            if (_avatars.TryGetValue(size, out var avatar))
                return avatar;

            var avatarUrl = YaGamesGetPhoto(AvatarSizes.GetValueOrDefault(size, "small")).ConsumeUtf8();

            if (string.IsNullOrEmpty(avatarUrl) == false)
                _avatars.TryAdd(size, avatarUrl);

            return avatarUrl;
        }

        public async Task SignIn()
        {
            _status = InitializationStatus.Waiting;
            YaGamesGetPlayer(true, OnSuccess, OnError);
            
            while (_status == InitializationStatus.Waiting)
                await Task.Yield();

            if (_status == InitializationStatus.Error)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Authentication]: Attempt to get a local id!");
                }
                
                _status = InitializationStatus.Waiting;
                YaGamesGetPlayer(false, OnSuccess, OnError);
            }
            else
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Authentication]: You are logged in as a {_signInType}!");
                }
                return;
            }
            
            while (_status == InitializationStatus.Waiting)
                await Task.Yield();

            if (_status == InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Authentication]: You are logged in as a {_signInType}!");
                }
            }
            
            return;
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                Instance._status = InitializationStatus.Initialized;
                
                Instance.InitializeId();
                Instance.InitializeName();
                Instance.InitializeMode();
                Instance.InitializePayingStatus();

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Authentication]: YaPlayerData initialized!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                Instance._status = InitializationStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Authentication]: An error occurred while sign in the YaPlayerData!");
                }
            }
        }

        public async Task SignInAsGuest()
        {
            _status = InitializationStatus.Waiting;
            YaGamesGetPlayer(false, OnSuccess, OnError);

            while (_status == InitializationStatus.Waiting)
                await Task.Yield();

            if (_status == InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Authentication]: You are logged in as a {_signInType}!");
                }
            }
            
            return;
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                Instance._status = InitializationStatus.Initialized;
                
                Instance.InitializeId();
                Instance.InitializeName();
                Instance.InitializeMode();
                Instance.InitializePayingStatus();

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Authentication]: YaPlayerData initialized!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                Instance._status = InitializationStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Authentication]: An error occurred while sign in the YaPlayerData!");
                }
            }
        }

        private void InitializeId()
        {
            if (_status != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Authentication]: YaPlayerData is not initialized!");
                }

                return;
            }
            
            _id = YaGamesGetId().ConsumeUtf8();
        }

        private void InitializePayingStatus()
        {
            if (_status != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Authentication]: YaPlayerData is not initialized!");
                }

                return;
            }

            var payingStatus = YaGamesGetPayingStatus().ConsumeUtf8();
            _payingStatus = PayingStatuses.GetValueOrDefault(payingStatus, PayingStatusType.None);
        }

        private void InitializeName()
        {
            if (_status != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Authentication]: YaPlayerData is not initialized!");
                }

                return;
            }

            _name = YaGamesGetName().ConsumeUtf8();
        }

        private void InitializeMode()
        {
            if (_status != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Authentication]: YaPlayerData is not initialized!");
                }

                return;
            }

            var result = YaGamesGetMode();
            
            _signInType = result switch
            {
                1 => SignInType.Account,
                0 => SignInType.Guest,
                _ => SignInType.None
            };

            if (GameApp.IsDebugMode)
            {
                Debug.Log($"[GameSDK.Authentication]: Result auth: {result}");
            }
        }

        public async Task<StorageStatus> Save(string key, string value)
        {
            if (InitializationStatus != InitializationStatus.Initialized)
            {
                await Auth.SignInAsGuest();
            }

            if (InitializationStatus != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Storage]: YaPlayerData is not initialized!");
                }

                return StorageStatus.Error;
            }

            _lastStorageStatus = StorageStatus.Waiting;
            YaGamesSaveData(key, value, OnSuccess, OnError);

            while (_lastStorageStatus == StorageStatus.Waiting)
                await Task.Yield();

            return _lastStorageStatus;

            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                Instance._lastStorageStatus = StorageStatus.Success;

                var runner = GameApp.Runner;

                if (runner != null)
                {
                    Instance._coroutineDelayedSave ??= runner.StartCoroutine(DelayedSave());
                    return;
                }

                YaGamesSaveDataAll(OnSuccessAll, OnErrorAll);
                return;
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                Instance._lastStorageStatus = StorageStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Storage]: Failed to save data in the YaPlayerData!");
                }
            }

            static IEnumerator DelayedSave()
            {
                yield return new WaitForSeconds(1f);

                YaGamesSaveDataAll(OnSuccessAll, OnErrorAll);
                Instance._coroutineDelayedSave = null;
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccessAll()
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Storage]: Data saved all in the YaPlayerData!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnErrorAll()
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Storage]: Failed to save data all in the YaPlayerData!");
                }
            }
        }

        public async Task<(StorageStatus, string)> Load(string key)
        {
            if (InitializationStatus != InitializationStatus.Initialized)
            {
                await Auth.SignInAsGuest();
            }

            if (InitializationStatus != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Storage]: YaPlayerData is not initialized!");
                }

                return (StorageStatus.Error, string.Empty);
            }

            _lastStorageStatus = StorageStatus.Waiting;
            YaGamesLoadData(key, OnSuccess, OnError);

            while (_lastStorageStatus == StorageStatus.Waiting)
                await Task.Yield();

            if (_lastStorageStatus == StorageStatus.Success)
            {
                return (_lastStorageStatus, _lastStorageData);
            }

            return (_lastStorageStatus, string.Empty);

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnSuccess(string data)
            {
                Instance._lastStorageStatus = StorageStatus.Success;
                Instance._lastStorageData = data;
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Storage]: Data loaded from the YaPlayerData!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                Instance._lastStorageStatus = StorageStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Storage]: Failed to load data from the YaPlayerData!");
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            Auth.Register(Instance);
            Storage.Register(Instance);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void YaGamesGetPlayer(bool isSigned, Action onSuccess, Action onError);
        
        [DllImport("__Internal")]
        private static extern IntPtr YaGamesGetId();
        [DllImport("__Internal")]
        private static extern IntPtr YaGamesGetPayingStatus();
        [DllImport("__Internal")]
        private static extern IntPtr YaGamesGetPhoto(string size);
        
        [DllImport("__Internal")]
        private static extern IntPtr YaGamesGetName();
        
        [DllImport("__Internal")]
        private static extern int YaGamesGetMode();
        
        [DllImport("__Internal")]
        private static extern void YaGamesSaveData(string key, string value, Action onSuccess, Action onError);
        [DllImport("__Internal")]
        private static extern void YaGamesSaveDataAll(Action onSuccess, Action onError);
        
        [DllImport("__Internal")]
        private static extern void YaGamesLoadData(string key, Action<string> onSuccess, Action onError);
#else
        private static void YaGamesGetPlayer(bool isSigned, Action onSuccess, Action onError) => onSuccess?.Invoke();

        private static string YaGamesGetId() => "test_user_id";

        private static string YaGamesGetPayingStatus() => "unknown";

        private static string YaGamesGetPhoto(string size) => $"default_{size}";

        private static string YaGamesGetName() => "test_user_name";

        private static int YaGamesGetMode() => 1;

        private static void YaGamesSaveData(string key, string value, Action onSuccess, Action onError)
        {
            PlayerPrefs.SetString(key, value);
            onSuccess?.Invoke();
        }
        
        private static void YaGamesSaveDataAll(Action onSuccess, Action onError) => onSuccess?.Invoke();

        private static void YaGamesLoadData(string key, Action<string> onSuccess, Action onError)
        {
            if (PlayerPrefs.HasKey(key))
                onSuccess?.Invoke(PlayerPrefs.GetString(key));
            else
                onError?.Invoke();
        }
#endif
    }
}