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

#if !UNITY_EDITOR
            if (_avatars.TryGetValue(size, out var avatar))
                return avatar;

            var avatarUrl = YaGamesGetPhoto(_avatarSizes.GetValueOrDefault(size, "small"));

            if (string.IsNullOrEmpty(avatarUrl) == false)
                _avatars.TryAdd(size, avatarUrl);

            return avatarUrl;
#else
            return "default";
#endif
        }

        public async Task SignIn()
        {
#if !UNITY_EDITOR
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
#else
            _status = InitializationStatus.Waiting;
            _signInType = SignInType.Account;
            OnSuccess();
            await Task.CompletedTask;
#endif
            
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
#if !UNITY_EDITOR
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
#else
            _status = InitializationStatus.Waiting;
            _signInType = SignInType.Guest;
            OnSuccess();
            await Task.CompletedTask;
#endif
            
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

#if !UNITY_EDITOR
            _id = YaGamesGetId();
#else
            _id = $"Id [{_signInType.ToString()}]";
#endif
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

#if !UNITY_EDITOR
            var payingStatus = YaGamesGetPayingStatus();
            _payingStatus = _payingStatuses.GetValueOrDefault(payingStatus, PayingStatusType.None);
#else
            _payingStatus = PayingStatusType.Unknown;
#endif
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

#if !UNITY_EDITOR
            _name = YaGamesGetName();
#else
            _name = $"Name [{_signInType.ToString()}]";
#endif
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

#if !UNITY_EDITOR
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
#else
            _signInType = SignInType.Account;
#endif
        }

        public async Task<StorageStatus> Save(string key, string value)
        {
#if !UNITY_EDITOR

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
#else
            _lastStorageStatus = StorageStatus.Waiting;
            PlayerPrefs.SetString(key, value);
            OnSuccess();
            await Task.CompletedTask;
            return _lastStorageStatus;
#endif

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


#if !UNITY_EDITOR
                YaGamesSaveDataAll(OnSuccessAll, OnErrorAll);
                return;
#endif
                OnSuccessAll();
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

#if !UNITY_EDITOR
                YaGamesSaveDataAll(OnSuccessAll, OnErrorAll);
                _instance._coroutineDelayedSave = null;
                yield break;
#endif
                OnSuccessAll();
                Instance._coroutineDelayedSave = null;
                yield break;
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
#if !UNITY_EDITOR

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
#else
            _lastStorageStatus = StorageStatus.Waiting;
            if (PlayerPrefs.HasKey(key) == false)
            {
                OnError();
            }
            else
            {
                OnSuccess(PlayerPrefs.GetString(key));
            }
            await Task.CompletedTask;
            return (_lastStorageStatus, _lastStorageData);
#endif
            
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


        [DllImport("__Internal")]
        private static extern void YaGamesGetPlayer(bool isSigned, Action onSuccess, Action onError);
        
        [DllImport("__Internal")]
        private static extern string YaGamesGetId();
        [DllImport("__Internal")]
        private static extern string YaGamesGetPayingStatus();
        [DllImport("__Internal")]
        private static extern string YaGamesGetPhoto(string size);
        
        [DllImport("__Internal")]
        private static extern string YaGamesGetName();
        
        [DllImport("__Internal")]
        private static extern int YaGamesGetMode();
        
        [DllImport("__Internal")]
        private static extern string YaGamesSaveData(string key, string value, Action onSuccess, Action onError);
        [DllImport("__Internal")]
        private static extern string YaGamesSaveDataAll(Action onSuccess, Action onError);
        
        [DllImport("__Internal")]
        private static extern string YaGamesLoadData(string key, Action<string> onSuccess, Action onError);
    }
}