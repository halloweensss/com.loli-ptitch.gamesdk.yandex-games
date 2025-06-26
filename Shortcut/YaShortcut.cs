using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Core;
using GameSDK.Plugins.YaGames.Core;
using GameSDK.Shortcut;
using UnityEngine;

namespace GameSDK.Plugins.YaGames.Shortcut
{
    public class YaShortcut : IShortcutApp
    {
        private static readonly YaShortcut Instance = new YaShortcut();
        
        private ShortcutStatus _status;
        public string ServiceId => Service.YaGames;
        public InitializationStatus InitializationStatus => InitializationStatus.Initialized;
        public async Task<bool> Create()
        {
#if !UNITY_EDITOR
            YaGamesCreateShortcut(OnSuccess, OnError);

            _status = ShortcutStatus.Waiting;
            while (_status == ShortcutStatus.Waiting)
                await Task.Yield();
#else
            _status = ShortcutStatus.Waiting;
            OnSuccess();
            await Task.CompletedTask;
#endif
            return _status == ShortcutStatus.Success;
            
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                Instance._status = ShortcutStatus.Success;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Shortcut]: YaShortcut shortcut created!");
                }
            }

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnError(string reason)
            {
                Instance._status = ShortcutStatus.Error;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Shortcut]: YaShortcut an error occurred while creating the shortcut! Reason: {reason}");
                }
            }
        }

        public async Task<bool> CanCreate()
        {
#if !UNITY_EDITOR
            YaGamesCanCreateShortcut(OnSuccess, OnError);

            _status = ShortcutStatus.Waiting;
            while (_status == ShortcutStatus.Waiting)
                await Task.Yield();
#else
            _status = ShortcutStatus.Waiting;
            OnSuccess();
            await Task.CompletedTask;
#endif
            return _status == ShortcutStatus.Success;
            
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                Instance._status = ShortcutStatus.Success;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Shortcut]: YaShortcut shortcut can created!");
                }
            }

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnError(string reason)
            {
                Instance._status = ShortcutStatus.Error;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Shortcut]: YaShortcut unable to create an icon! Reason: {reason}");
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            GameSDK.Shortcut.Shortcut.Register(Instance);
        }
        
        [DllImport("__Internal")]
        private static extern void YaGamesCreateShortcut(Action onSuccess, Action<string> onError);
        
        [DllImport("__Internal")]
        private static extern void YaGamesCanCreateShortcut(Action onSuccess, Action<string> onError);
    }
}