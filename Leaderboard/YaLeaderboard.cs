using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Core;
using GameSDK.Leaderboard;
using GameSDK.Plugins.YaGames.Core;
using UnityEngine;

namespace GameSDK.Plugins.YaGames.Leaderboard
{
    public class YaLeaderboard : ILeaderboardApp
    {
        private static readonly YaLeaderboard Instance = new YaLeaderboard();

        private InitializationStatus _status = InitializationStatus.None;
        private LeaderboardStatus _statusResponse = LeaderboardStatus.None;
        private LeaderboardDescription _descriptionResponse = new LeaderboardDescription();
        private LeaderboardPlayerData _playerDataResponse = new LeaderboardPlayerData();
        private LeaderboardEntries _entriesResponse = new LeaderboardEntries();
        public string ServiceId => Service.YaGames;
        public InitializationStatus InitializationStatus => _status;

        public async Task Initialize()
        {
            _status = InitializationStatus.Waiting;
            OnSuccess();
            await Task.CompletedTask;
            return;
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                Instance._status = InitializationStatus.Initialized;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Leaderboard]: YaGamesApp initialized!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                Instance._status = InitializationStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Leaderboard]: An error occurred while initializing the YaGamesApp!");
                }
            }
        }

        public async Task<LeaderboardDescription> GetDescription(string id)
        {
            _statusResponse = LeaderboardStatus.Waiting;
            
            YaLeaderboardGetDescription(id, OnSuccess, OnError);

            while (_statusResponse == LeaderboardStatus.Waiting)
                await Task.Yield();

            return _statusResponse == LeaderboardStatus.Success ? _descriptionResponse : null;

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnSuccess(string data)
            {
                var yaDescription = JsonUtility.FromJson<YaLeaderboardDescription>(data);
                Instance._descriptionResponse = ToLeaderboardDescription(yaDescription);

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Leaderboard]: YaGamesApp description received!");
                }

                Instance._statusResponse = LeaderboardStatus.Success;
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                Instance._statusResponse = LeaderboardStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Leaderboard]: YaGamesApp description not received!");
                }
            }
        }

        public async Task<LeaderboardStatus> SetScore(string id, long score, string extraData)
        {
            _statusResponse = LeaderboardStatus.Waiting;
            
            YaLeaderboardSetScore(id, score.ToString(CultureInfo.InvariantCulture), extraData, OnSuccess, OnError);

            while (_statusResponse == LeaderboardStatus.Waiting)
                await Task.Yield();

            return _statusResponse;

            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Leaderboard]: YaGamesApp the data is recorded in the leaderboard!");
                }

                Instance._statusResponse = LeaderboardStatus.Success;
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                Instance._statusResponse = LeaderboardStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Leaderboard]: YaGamesApp the data is not entered in the leaderboard!");
                }
            }
        }

        public async Task<(LeaderboardStatus, LeaderboardPlayerData)> GetPlayerData(string id)
        {
            _statusResponse = LeaderboardStatus.Waiting;
            
            YaLeaderboardGetPlayerData(id, OnSuccess, OnError);

            while (_statusResponse == LeaderboardStatus.Waiting)
                await Task.Yield();

            return (_statusResponse, _playerDataResponse);

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnSuccess(string data)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log(
                        $"[GameSDK.Leaderboard]: YaGamesApp the data is recorded in the leaderboard!\nData:{data}");
                }

                var yaPlayerData = JsonUtility.FromJson<YaLeaderboardPlayerData>(data);
                Instance._playerDataResponse = ToLeaderboardPlayerData(yaPlayerData);

                Instance._statusResponse = LeaderboardStatus.Success;
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                Instance._playerDataResponse = null;
                Instance._statusResponse = LeaderboardStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Leaderboard]: YaGamesApp the data is not entered in the leaderboard!");
                }
            }
        }

        public async Task<(LeaderboardStatus, LeaderboardEntries)> GetEntries(LeaderboardParameters parameters)
        {
            _statusResponse = LeaderboardStatus.Waiting;
            
            YaLeaderboardGetEntries(parameters.id, parameters.includeUser, parameters.quantityAround, parameters.quantityTop, OnSuccess, OnError);

            while (_statusResponse == LeaderboardStatus.Waiting)
                await Task.Yield();

            return (_statusResponse, _entriesResponse);

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnSuccess(string data)
            {
                var yaEntries = JsonUtility.FromJson<YaLeaderboardEntries>(data);

                var dataEntries = new LeaderboardEntries();
                dataEntries.Leaderboard = ToLeaderboardDescription(yaEntries.leaderboard);
                dataEntries.Ranges = new LeaderboardRange[yaEntries.ranges.Length];
                for (int i = 0; i < yaEntries.ranges.Length; i++)
                {
                    var range = yaEntries.ranges[i];
                    dataEntries.Ranges[i] = new LeaderboardRange()
                    {
                        Start = ToInt64(range.start),
                        Size = ToInt64(range.size)
                    };
                }

                dataEntries.Entries = new LeaderboardPlayerData[yaEntries.entries.Length];
                for (int i = 0; i < yaEntries.entries.Length; i++)
                {
                    var playerData = yaEntries.entries[i];
                    dataEntries.Entries[i] = ToLeaderboardPlayerData(playerData);
                }

                dataEntries.UserRank = ToInt64(yaEntries.userRank);
                dataEntries.HasUserRank = dataEntries.UserRank > 0;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Leaderboard]: YaGamesApp the entries is recorded in the leaderboard!");
                }

                Instance._entriesResponse = dataEntries;
                Instance._statusResponse = LeaderboardStatus.Success;
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                Instance._entriesResponse = null;
                Instance._statusResponse = LeaderboardStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Leaderboard]: YaGamesApp the entries is not entered in the leaderboard!");
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            GameSDK.Leaderboard.Leaderboard.Register(Instance);
        }

        private static LeaderboardPlayerData ToLeaderboardPlayerData(YaLeaderboardPlayerData playerData)
        {
            return new LeaderboardPlayerData()
            {
                PlayerId = playerData.player.uniqueID,
                Name = playerData.player.publicName,
                AvatarUrl = playerData.player.avatarUrl,
                Rank = ToInt64(playerData.rank),
                Score = ToInt64(playerData.score),
                ExtraData = playerData.extraData
            };
        }

        private static LeaderboardDescription ToLeaderboardDescription(YaLeaderboardDescription description)
        {
            var providerDescription = description.description;
            var scoreFormat = providerDescription?.score_format;

            return new LeaderboardDescription()
            {
                AppId = description.appID,
                Name = description.name,
                Title = new Title()
                {
                    EN = description.title?.en,
                    RU = description.title?.ru
                },
                LocalizedTitles = ToLocalizedTitles(description.localizedTitles),
                IsDefault = description.@default,
                SortOrder = providerDescription?.sort_order,
                IsInvertedSortOrder = providerDescription?.invert_sort_order ?? false,
                ScoreFormat = new LeaderboardScoreFormat()
                {
                    Type = scoreFormat?.type,
                    DecimalOffset = scoreFormat?.options?.decimal_offset ?? 0
                }
            };
        }

        private static LeaderboardLocalizedTitle[] ToLocalizedTitles(YaLeaderboardDescription.LocalizedTitle[] titles)
        {
            if (titles == null)
                return Array.Empty<LeaderboardLocalizedTitle>();

            var localizedTitles = new LeaderboardLocalizedTitle[titles.Length];
            for (var i = 0; i < titles.Length; i++)
            {
                localizedTitles[i] = new LeaderboardLocalizedTitle()
                {
                    Locale = titles[i].locale,
                    Value = titles[i].value
                };
            }

            return localizedTitles;
        }

        private static long ToInt64(string value)
        {
            return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
                ? result
                : 0;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void YaLeaderboardGetDescription(string id, Action<string> onSuccess, Action onError);
        [DllImport("__Internal")]
        private static extern void YaLeaderboardSetScore(string id, string score, string extraData, Action onSuccess, Action onError);
        [DllImport("__Internal")]
        private static extern void YaLeaderboardGetPlayerData(string id, Action<string> onSuccess, Action onError);
        [DllImport("__Internal")]
        private static extern void YaLeaderboardGetEntries(string id, bool includeUser, int quantityAround, int quantityTop, Action<string> onSuccess, Action onError);
#else
        private static void YaLeaderboardGetDescription(string id, Action<string> onSuccess, Action onError)
        {
            onSuccess?.Invoke(JsonUtility.ToJson(new YaLeaderboardDescription()
            {
                appID = Application.productName,
                @default = true,
                description = CreateDefaultDescription(),
                localizedTitles = CreateDefaultLocalizedTitles(),
                title = new YaLeaderboardDescription.Title
                {
                    en = "Title",
                    ru = "Заголовок"
                },
                name = id
            }));
        }

        private static void YaLeaderboardSetScore(string id, string score, string extraData, Action onSuccess, Action onError) => onSuccess?.Invoke();

        private static void YaLeaderboardGetPlayerData(string id, Action<string> onSuccess, Action onError)
        {
            onSuccess?.Invoke(JsonUtility.ToJson(new YaLeaderboardPlayerData()
            {
                extraData = string.Empty,
                player = new Player()
                {
                    publicName = "test",
                    uniqueID = "-1",
                    avatarUrl = "https://example.com/avatar-medium.png"
                },
                rank = "1",
                score = "0"
            }));
        }

        private static void YaLeaderboardGetEntries(string id, bool includeUser, int quantityAround, int quantityTop, Action<string> onSuccess, Action onError)
        {
            onSuccess?.Invoke(JsonUtility.ToJson(new YaLeaderboardEntries()
            {
                leaderboard = new YaLeaderboardDescription()
                {
                    appID = "-1",
                    @default = true,
                    description = CreateDefaultDescription(),
                    localizedTitles = CreateDefaultLocalizedTitles(),
                    title = new YaLeaderboardDescription.Title()
                    {
                        en = "Title",
                        ru = "Заголовок"
                    },
                    name = id
                },
                ranges = new[]
                {
                    new YaLeaderboardRanges(size: "1", start: "0")
                },
                entries = new[]
                {
                    new YaLeaderboardPlayerData(extraData: string.Empty,
                        player: new Player(publicName: "test", uniqueID: "-1",
                            avatarUrl: "https://example.com/avatar-medium.png"), rank: "1", score: "0")
                },
                userRank = "1"
            }));
        }

        private static YaLeaderboardDescription.Description CreateDefaultDescription()
        {
            return new YaLeaderboardDescription.Description()
            {
                invert_sort_order = false,
                sort_order = "DESC",
                score_format = new YaLeaderboardDescription.ScoreFormat()
                {
                    type = "numeric",
                    options = new YaLeaderboardDescription.Options()
                    {
                        decimal_offset = 0
                    }
                }
            };
        }

        private static YaLeaderboardDescription.LocalizedTitle[] CreateDefaultLocalizedTitles()
        {
            return new[]
            {
                new YaLeaderboardDescription.LocalizedTitle() { locale = "en", value = "Title" },
                new YaLeaderboardDescription.LocalizedTitle() { locale = "ru", value = "Заголовок" }
            };
        }
#endif
    }
}
