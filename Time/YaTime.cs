using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Core;
using GameSDK.Plugins.YaGames.Core;
using GameSDK.Time;
using UnityEngine;

namespace GameSDK.Plugins.YaGames.Time
{
    public class YaTime : ITimeApp
    {
        private static readonly YaTime Instance = new();
        public string ServiceId => Service.YaGames;
        public InitializationStatus InitializationStatus => InitializationStatus.Initialized;
        
        private bool _processing = false;
        private long _lastTimestamp = 0;
        
        public Task<long> GetTimestamp()
        {
#if !UNITY_EDITOR
            _processing = true;
            
            YaGamesServerTime(Callback);
            
            while(_processing)
                Task.Yield();
            
            _processing = false;
            return Task.FromResult(_lastTimestamp);
#endif
            return Task.FromResult(0L);
            
            [MonoPInvokeCallback(typeof(Action))]
            static void Callback(string result)
            {
                Instance._lastTimestamp = long.Parse(result);
                Instance._processing = false;
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            GameSDK.Time.Time.Register(Instance);
        }
        
        [DllImport("__Internal")]
        private static extern void YaGamesServerTime(Action<string> callback);
    }
}