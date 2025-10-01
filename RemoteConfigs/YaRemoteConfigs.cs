using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AOT;
using GameSDK.Core;
using GameSDK.Core.Tools;
using GameSDK.Plugins.YaGames.Core;
using GameSDK.RemoteConfigs;
using UnityEngine;

namespace GameSDK.Plugins.YaGames.RemoteConfigs
{
    public class YaRemoteConfigs : IRemoteConfigsApp
    {
        private static readonly YaRemoteConfigs Instance = new();

        private readonly Dictionary<string, RemoteConfigValue> _remoteValues = new(16);

        public string ServiceId => Service.YaGames;
        public InitializationStatus InitializationStatus { get; private set; } = InitializationStatus.None;

        public IReadOnlyDictionary<string, RemoteConfigValue> RemoteValues => _remoteValues;

        public async Task Initialize()
        {
            InitializationStatus = InitializationStatus.Waiting;
            
            YaRemoteConfigsInitialize(OnSuccess, OnError);
            
            while (InitializationStatus == InitializationStatus.Waiting)
                await Task.Yield();
            
            return;

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnSuccess(string value)
            {
                Instance.InitializationStatus = InitializationStatus.Initialized;

                var values = JsonUtility.FromJson<SerializableList<KeyValueStruct<string>>>(value);

                foreach (var data in values.data)
                    Instance.TryAddOrReplace(data.key, data.value, ConfigValueSource.RemoteValue);

                if (GameApp.IsDebugMode)
                    Debug.Log("[GameSDK.RemoteConfigs]: YaGamesApp initialized!");
            }

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnError(string value)
            {
                Instance.InitializationStatus = InitializationStatus.Error;
                if (GameApp.IsDebugMode)
                    Debug.Log("[GameSDK.RemoteConfigs]: An error occurred while initializing the YaGamesApp!");
            }
        }

        public async Task InitializeWithUserParameters(params KeyValuePair<string, string>[] parameters)
        {
            InitializationStatus = InitializationStatus.Waiting;
            
            var data = new List<KeyValueStruct<string>>(parameters.Length);

            foreach (var parameter in parameters)
            {
                data.Add(new KeyValueStruct<string>(parameter.Key, parameter.Value));
            }

            var serializableList = new SerializableList<KeyValueStruct<string>>
            {
                data = data
            };
            
            var json = JsonUtility.ToJson(serializableList);
            YaRemoteConfigsInitializeWithClientParameters(json, OnSuccess, OnError);
            
            while (InitializationStatus == InitializationStatus.Waiting)
                await Task.Yield();
            
            return;
            
            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnSuccess(string value)
            {
                Instance.InitializationStatus = InitializationStatus.Initialized;

                var values = JsonUtility.FromJson<SerializableList<KeyValueStruct<string>>>(value);

                foreach (var data in values.data)
                    Instance.TryAddOrReplace(data.key, data.value, ConfigValueSource.RemoteValue);

                if (GameApp.IsDebugMode)
                    Debug.Log("[GameSDK.RemoteConfigs]: YaGamesApp initialized!");
            }

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnError(string value)
            {
                Instance.InitializationStatus = InitializationStatus.Error;
                if (GameApp.IsDebugMode)
                    Debug.Log("[GameSDK.RemoteConfigs]: An error occurred while initializing the YaGamesApp!");
            }
        }

        private void TryAddOrReplace(string key, string value, ConfigValueSource source)
        {
            if (_remoteValues.ContainsKey(key))
                _remoteValues[key] = new RemoteConfigValue(Encoding.UTF8.GetBytes(value), source);
            else
                _remoteValues.Add(key, new RemoteConfigValue
                (
                    Encoding.UTF8.GetBytes(value),
                    source
                ));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            GameSDK.RemoteConfigs.RemoteConfigs.Register(Instance);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void YaRemoteConfigsInitialize(Action<string> onSuccess, Action<string> onError);

        [DllImport("__Internal")]
        private static extern void YaRemoteConfigsInitializeWithClientParameters(string parameters,
            Action<string> onSuccess, Action<string> onError);
#else
        private static void YaRemoteConfigsInitialize(Action<string> onSuccess, Action<string> onError)
        {
            var data = new List<KeyValueStruct<string>>
            {
                new("1", "1"),
                new("2", "2"),
                new("4", "3")
            };

            var serializableList = new SerializableList<KeyValueStruct<string>>
            {
                data = data
            };

            var json = JsonUtility.ToJson(serializableList);

            onSuccess?.Invoke(json);
        }
        
        private static void YaRemoteConfigsInitializeWithClientParameters(string parameters, Action<string> onSuccess, Action<string> onError)
        {
            var data = new List<KeyValueStruct<string>>
            {
                new("1", "4"),
                new("2", "5"),
                new("4", "6")
            };

            var serializableList = new SerializableList<KeyValueStruct<string>>
            {
                data = data
            };

            var json = JsonUtility.ToJson(serializableList);

            onSuccess?.Invoke(json);
        }
#endif
    }
}