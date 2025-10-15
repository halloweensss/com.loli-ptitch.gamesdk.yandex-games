using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Core;
using GameSDK.Plugins.YaGames.Extension;
using UnityEngine;
using DeviceType = GameSDK.Core.DeviceType;

namespace GameSDK.Plugins.YaGames.Core
{
    internal sealed class YaGamesApp : ICoreApp
    {
        private static readonly YaGamesApp Instance = new();

        private InitializationStatus _status = InitializationStatus.None;
        private DeviceType _device = DeviceType.Undefined;
        private YaEnvironment _environment = new();
        private bool _ready;
        private bool _started = false;

        public string ServiceId => Service.YaGames;
        public InitializationStatus InitializationStatus => _status;

        public DeviceType DeviceType
        {
            get
            {
                if (_device == DeviceType.Undefined)
                {
                    InitializeDeviceType();
                }

                return _device;
            }
        }

        public string AppId => _environment.app.id;
        public string Lang => _environment.i18n.lang;
        public string Payload => _environment.payload;
        public bool IsReady => _ready;
        public bool IsStarted => _started;

        public async Task Initialize()
        {
            if (_status == InitializationStatus.Initialized || _status == InitializationStatus.Waiting)
            {
                return;
            }

            await Instance.InitializeInternal();
        }

        public async Task Ready()
        {
            if (_ready)
                return;

            await Instance.GameReadyInternal();
        }

        public async Task Start()
        {
            await Instance.GameStartInternal();
        }

        public async Task Stop()
        {
            await Instance.GameStopInternal();
        }

        private async Task InitializeInternal()
        {
            _status = InitializationStatus.Waiting;
            YaGamesInitialize(OnSuccess, OnError);

            while (_status == InitializationStatus.Waiting)
                await Task.Yield();

            await Task.CompletedTask;
            return;

            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                Instance._status = InitializationStatus.Initialized;

                Instance.InitializeDeviceType();
                Instance.InitializeEnvironment();

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: YaGamesApp initialized!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                Instance._status = InitializationStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: An error occurred while initializing the YaGamesApp!");
                }
            }
        }

        private Task GameReadyInternal()
        {
            YaGamesReady(OnSuccess, OnError);
            return Task.CompletedTask;

            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                Instance._ready = true;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: YaGamesApp ready!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                Instance._ready = false;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: An error occurred while ready the YaGamesApp!");
                }
            }
        }

        private Task GameStartInternal()
        {
            YaGamesStart(OnSuccess, OnError);
            return Task.CompletedTask;

            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                Instance._started = true;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: YaGamesApp started!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: An error occurred while start the YaGamesApp!");
                }
            }
        }

        private Task GameStopInternal()
        {
            YaGamesStop(OnSuccess, OnError);
            return Task.CompletedTask;

            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                Instance._started = false;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: YaGamesApp stopped!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: An error occurred while stop the YaGamesApp!");
                }
            }
        }

        private void InitializeDeviceType()
        {
            if (_status != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: YaGamesApp is not initialized!");
                }

                return;
            }

            _device = (DeviceType)YaGamesGetDeviceType();
        }

        private void InitializeEnvironment()
        {
            if (_status != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: YaGamesApp is not initialized!");
                }

                return;
            }

            InitializeEnvironmentInternal();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            GameApp.Register(Instance);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void YaGamesInitialize(Action onSuccess, Action onError);
        [DllImport("__Internal")] private static extern void YaGamesReady(Action onSuccess, Action onError);
        [DllImport("__Internal")] private static extern void YaGamesStart(Action onSuccess, Action onError);
        [DllImport("__Internal")] private static extern void YaGamesStop(Action onSuccess, Action onError);
        [DllImport("__Internal")] private static extern int YaGamesGetDeviceType();
        [DllImport("__Internal")] private static extern IntPtr YaGamesGetEnvironment();
        
        private static void InitializeEnvironmentInternal()
        {
            Instance._environment = YaInterop.WithPtr(
                YaGamesGetEnvironment,
                json => string.IsNullOrEmpty(json)
                    ? new YaEnvironment()
                    : JsonUtility.FromJson<YaEnvironment>(json)
            );
        }
#else
        private static void YaGamesInitialize(Action onSuccess, Action onError) => onSuccess?.Invoke();
        private static void YaGamesReady(Action onSuccess, Action onError) => onSuccess?.Invoke();
        private static void YaGamesStart(Action onSuccess, Action onError) => onSuccess?.Invoke();
        private static void YaGamesStop(Action onSuccess, Action onError) => onSuccess?.Invoke();

        private static int YaGamesGetDeviceType() =>
            SystemInfo.deviceType switch
            {
                UnityEngine.DeviceType.Unknown => (int)DeviceType.Undefined,
                UnityEngine.DeviceType.Handheld => (int)DeviceType.Mobile,
                UnityEngine.DeviceType.Console => (int)DeviceType.Console,
                UnityEngine.DeviceType.Desktop => (int)DeviceType.Desktop,
                _ => (int)DeviceType.Undefined
            };

        private static IntPtr YaGamesGetEnvironment() => IntPtr.Zero;

        private static void InitializeEnvironmentInternal()
        {
            var sysLang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant();
            Instance._environment = new YaEnvironment
            {
                app = new YaApp
                {
                    id = string.IsNullOrEmpty(Application.identifier) ? "editor" : Application.identifier
                },
                browser = new YaBrowser
                {
                    lang = sysLang
                },
                i18n = new YaI18n
                {
                    lang = sysLang,
                    tld = "com"
                },
                payload = string.Empty
            };
        }
#endif
    }
}
