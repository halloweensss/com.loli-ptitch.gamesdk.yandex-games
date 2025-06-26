﻿var YaGames = {

    $yaGames: {
        SDK: undefined,
        Player: undefined,
        Purchases: undefined,
        IsInitialized: false,
        SaveDataObject: {},
        
        Initialize: function (callbackSuccess, callbackError) {
            window['YaGames']
                .init()
                .then(ysdk => {
                    yaGames.SDK = ysdk;
                    dynCall('v', callbackSuccess, []);
                })
                .catch(e => {
                    dynCall('v', callbackError, []);
                });
        },
        
        GameReady: function (callbackSuccess, callbackError) {
            
            if(yaGames.SDK == undefined) {
                dynCall('v', callbackError, []);
                return;
            }
            
            if(yaGames.SDK.features == undefined) {
                dynCall('v', callbackError, []);
                return;
            }
            
            if(yaGames.SDK.features.LoadingAPI == undefined) {
                dynCall('v', callbackError, []);
                return;
            }
            
            yaGames.SDK.features.LoadingAPI.ready();
            dynCall('v', callbackSuccess, []);
        },

        GameStart: function (callbackSuccess, callbackError) {

            if(yaGames.SDK == undefined) {
                dynCall('v', callbackError, []);
                return;
            }

            if(yaGames.SDK.features == undefined) {
                dynCall('v', callbackError, []);
                return;
            }

            if(yaGames.SDK.features.GameplayAPI == undefined) {
                dynCall('v', callbackError, []);
                return;
            }

            yaGames.SDK.features.GameplayAPI.start();
            dynCall('v', callbackSuccess, []);
        },

        GameStop: function (callbackSuccess, callbackError) {

            if(yaGames.SDK == undefined) {
                dynCall('v', callbackError, []);
                return;
            }

            if(yaGames.SDK.features == undefined) {
                dynCall('v', callbackError, []);
                return;
            }

            if(yaGames.SDK.features.GameplayAPI == undefined) {
                dynCall('v', callbackError, []);
                return;
            }

            yaGames.SDK.features.GameplayAPI.stop();
            dynCall('v', callbackSuccess, []);
        },
        
        GetDeviceType: function() {
            const deviceType = yaGames.SDK.deviceInfo.type;
            
            switch (deviceType){
                case 'desktop':
                    return 1;
                case 'mobile':
                    return 2;
                case 'tablet':
                    return 3;
                case 'tv':
                    return 4;
                default:
                    return 0;
            }
        },
        
        GetEnvironment: function() {
            const json = JSON.stringify(yaGames.SDK.environment);
            const jsonUnity = yaGames.GetAllocatedString(json);
            return jsonUnity;
        },
        
        InitPlayer: function (signed) {
            return yaGames.SDK.getPlayer({signed: signed})
                .then(player => {
                    yaGames.Player = player;
                    return player;
                });
        },

        GetPlayer: function (signed, callbackSuccess, callbackError) {
            yaGames.InitPlayer(signed)
                .then(async player => {
                    yaGames.Player = player;

                    await yaGames.LoadAllData();
                    
                    if(player.isAuthorized() === false && signed){
                        console.error("Player is not login to yandex!");
                        yaGames.SDK.auth.openAuthDialog().then(() => {
                            yaGames.InitPlayer(signed)
                                .then(async player => {
                                    yaGames.Player = player;

                                    await yaGames.LoadAllData();
                                    
                                    dynCall('v', callbackSuccess, []);
                                    return;
                                })
                                .catch(e => {
                                console.error(e.toString());
                                dynCall('v', callbackError, []);
                                return;
                            });
                        }).catch(e => {
                            console.error(e.toString());
                            dynCall('v', callbackError, []);
                            return;
                        });
                    }else{
                        dynCall('v', callbackSuccess, []);
                    }
                    return;
                })
                .catch(e => {
                    console.error(e.toString());
                    dynCall('v', callbackError, []);
                    return;
                });
        },
        
        GetId: function() {
            const id = yaGames.GetAllocatedString(yaGames.Player.getUniqueID());
            return id;
        },
        
        GetName: function() {
            const name = yaGames.GetAllocatedString(yaGames.Player.getName());
            return name;
        },
        
        GetPayingStatus: function() {
            const status = yaGames.GetAllocatedString(yaGames.Player.getPayingStatus());
            return status;
        },
        
        GetPhoto: function(size) {
            const photoSrc = yaGames.GetAllocatedString(yaGames.Player.getPhoto(UTF8ToString(size)));
            return photoSrc;
        },

        GetMode: function() {
            const isAuthorized = yaGames.Player.isAuthorized();
            return isAuthorized ? 1 : 0;
        },

        SaveData: function (key, value, callbackSuccess, callbackError) {
            yaGames.SaveDataObject[UTF8ToString(key)] = UTF8ToString(value);
            dynCall('v', callbackSuccess, []);
        },

        SaveDataAll: function (callbackSuccess, callbackError) {
            yaGames.Player.setData(yaGames.SaveDataObject, false).then(() => {
                dynCall('v', callbackSuccess, []);
            }).catch(e => {
                dynCall('v', callbackError, []);
            });
        },
        
        LoadData: function (key, callbackSuccess, callbackError) {
            const keyStr = UTF8ToString(key);

            if(keyStr in yaGames.SaveDataObject) {

                const obj = yaGames.SaveDataObject;
                const result = obj[keyStr];

                if (result !== undefined) {
                    const dataString = yaGames.GetAllocatedString(obj[keyStr]);
                    dynCall('vi', callbackSuccess, [dataString]);
                    _free(dataString);
                    return;
                }
            }

            const array = [keyStr];
            
            yaGames.Player.getData(array).then(data => {
                const obj = data;
                const result = obj[keyStr];

                if(result === undefined){

                    dynCall('v', callbackError, []);
                    return;
                }
                
                yaGames.SaveDataObject[keyStr] = result;
                const dataString = yaGames.GetAllocatedString(result);
                dynCall('vi', callbackSuccess, [dataString]);
                _free(dataString);
            }).catch(e => {
                dynCall('v', callbackError, []);
            });
        },
        
        LoadAllData: function () {
            return yaGames.Player.getData().then(data => {
                yaGames.SaveDataObject = data;
            }).catch(e => {
            });
        },
        
        CanReview: function(callbackSuccess, callbackError) {
            yaGames.SDK.feedback.canReview()
                .then(({ value, reason }) => {
                    if (value) {
                        dynCall('v', callbackSuccess, []);
                    } else {
                        const typeReason = yaGames.GetReviewType(reason);
                        dynCall('vi', callbackError, [typeReason]);
                    }
                }).catch(e => {
                dynCall('vi', callbackError, [0]);
            });
        },
        
        RequestReview: function(callbackSuccess, callbackError) {
            yaGames.SDK.feedback.canReview()
                .then(({ value, reason }) => {
                    if (value) {
                        yaGames.SDK.feedback.requestReview()
                            .then(({ feedbackSent }) => {
                                if(feedbackSent) {
                                    dynCall('v', callbackSuccess, []);
                                }else{
                                    dynCall('vi', callbackError, [5]);
                                }
                            }).catch( e => {
                            dynCall('vi', callbackError, [0]);
                        });
                    } else {
                        const typeReason = yaGames.GetReviewType(reason);
                        dynCall('vi', callbackError, [typeReason]);
                    }
                }).catch(e => {
                dynCall('vi', callbackError, [0]);
            });
        },

        ShowInterstitial: function(callbackOnOpen, callbackOnClose, callbackOnError, callbackOnOffline){
            yaGames.SDK.adv.showFullscreenAdv({
                callbacks: {
                    onOpen: function () {
                        dynCall('v', callbackOnOpen, []);
                    },
                    onClose: function (wasShown) {
                        dynCall('vi', callbackOnClose, [wasShown]);
                    },
                    onError: function (error) {
                        const errorStr = yaGames.GetAllocatedString(error);
                        dynCall('vi', callbackOnError, [errorStr]);
                        _free(errorStr);
                    },
                    onOffline: function () {
                        dynCall('v', callbackOnOffline, []);
                    },
                }
            });
        },
        
        ShowRewarded: function(callbackOnOpen, callbackOnClose, callbackOnError, callbackOnRewarded){
            yaGames.SDK.adv.showRewardedVideo({
                callbacks: {
                    onOpen: function () {
                        dynCall('v', callbackOnOpen, []);
                    },
                    onClose: function (wasShown) {
                        dynCall('v', callbackOnClose, []);
                    },
                    onError: function (error) {
                        const errorStr = yaGames.GetAllocatedString(error);
                        dynCall('vi', callbackOnError, [errorStr]);
                        _free(errorStr);
                    },
                    onRewarded: function () {
                        dynCall('v', callbackOnRewarded, []);
                    },
                }
            });
        },
        
        ShowBanner: function(callbackOnOpen, callbackOnError){
            yaGames.SDK.adv.getBannerAdvStatus().then(({isShowing, reason}) => {
                if(isShowing) {
                    dynCall('vi', callbackOnError, [2]);
                } else if(reason){
                    dynCall('vi', callbackOnError, [yaGames.GetBannerErrorType(reason)]);
                } else {
                    yaGames.SDK.adv.showBannerAdv();
                    dynCall('v', callbackOnOpen, []);
                }
            });
        },
        
        HideBanner: function(callbackOnHided, callbackOnError){
            yaGames.SDK.adv.getBannerAdvStatus().then(({isShowing, reason}) => {
                if(isShowing == false) {
                    dynCall('vi', callbackOnError, [3]);
                } else if(reason){
                    dynCall('vi', callbackOnError, [yaGames.GetBannerErrorType(reason)]);
                } else {
                    yaGames.SDK.adv.hideBannerAdv();
                    dynCall('v', callbackOnHided, []);
                }
            });
        },

        CreateShortcut: function(callbackOnSuccess, callbackOnError){
            yaGames.SDK.shortcut.canShowPrompt().then(prompt => {
                if(prompt.canShow){
                    yaGames.SDK.shortcut.showPrompt().then(result => {
                        if(result.outcome === 'accepted'){
                            dynCall('v', callbackOnSuccess, []);
                        }else{
                            const dataString = yaGames.GetAllocatedString(result.outcome);
                            dynCall('vi', callbackOnError, [dataString]);
                            _free(dataString);
                        }
                    })
                }else{
                    const dataString = yaGames.GetAllocatedString('Cant show');
                    dynCall('vi', callbackOnError, [dataString]);
                    _free(dataString);
                }
            });
        },
        
        CanCreateShortcut: function(callbackOnSuccess, callbackOnError) {
            yaGames.SDK.shortcut.canShowPrompt().then(prompt => {
                if (prompt.canShow) {
                    dynCall('v', callbackOnSuccess, []);
                } else {
                    const dataString = yaGames.GetAllocatedString('Cant show');
                    dynCall('vi', callbackOnError, [dataString]);
                    _free(dataString);
                }
            });
        },

        LeaderboardGetDescription: function(id, callbackOnSuccess, callbackOnError) {
            const idStr = UTF8ToString(id);
            yaGames.SDK.leaderboards.getDescription(idStr)
                .then(res => {
                    const dataString = yaGames.GetAllocatedString(JSON.stringify(res));
                    dynCall('vi', callbackOnSuccess, [dataString]);
                    _free(dataString);
                })
                .catch(e => {
                    dynCall('v', callbackOnError, []);
                });
        },

        LeaderboardSetScore: function(id, score, callbackSuccess, callbackError) {
            const idStr = UTF8ToString(id);
            yaGames.SDK.isAvailableMethod('leaderboards.setScore')
                .then(isAvailable => {
                    if (isAvailable == false) {
                        dynCall('v', callbackError, []);
                        return;
                    }

                    yaGames.SDK.leaderboards.setScore(idStr, score)
                        .then(() => {
                            dynCall('v', callbackSuccess, []);
                            return;
                        })
                        .catch(e => {
                            dynCall('v', callbackError, []);
                            return;
                        });

                }).catch(e => {
                dynCall('v', callbackError, []);
                return;
            });
        },

        LeaderboardGetPlayerData: function(id, callbackOnSuccess, callbackOnError) {
            const idStr = UTF8ToString(id);

            yaGames.SDK.isAvailableMethod('leaderboards.getPlayerEntry')
                .then(isAvailable => {
                    if (isAvailable == false) {
                        dynCall('v', callbackOnError, []);
                        return;
                    }

                    yaGames.SDK.leaderboards.getPlayerEntry(idStr)
                        .then(res => {
                            const dataString = yaGames.GetAllocatedString(JSON.stringify(res));
                            dynCall('vi', callbackOnSuccess, [dataString]);
                            _free(dataString);
                            return;
                        })
                        .catch(e => {
                            dynCall('v', callbackOnError, []);
                            return;
                        });

                })
                .catch(e => {
                    dynCall('v', callbackError, []);
                    return;
                });
        },

        LeaderboardGetEntries: function(id, includeUser, quantityAround, quantityTop, callbackSuccess, callbackError) {
            const idStr = UTF8ToString(id);

            yaGames.SDK.isAvailableMethod('leaderboards.getEntries')
                .then(isAvailable => {
                    if (isAvailable == false) {
                        dynCall('v', callbackError, []);
                        return;
                    }

                    yaGames.SDK.leaderboards.getEntries(idStr, {includeUser: includeUser, quantityTop: quantityTop, quantityAround: quantityAround})
                        .then(res => {
                            const dataString = yaGames.GetAllocatedString(JSON.stringify(res));
                            dynCall('vi', callbackSuccess, [dataString]);
                            _free(dataString);
                            return;
                        })
                        .catch(e => {
                            dynCall('v', callbackError, []);
                            return;
                        });
                })
                .catch(e => {
                    dynCall('v', callbackError, []);
                    return;
                });
        },

        PurchasesInitialize: function(callbackOnSuccess, callbackOnError) {
            yaGames.SDK.getPayments({signed: true})
                .then(payments => {
                    yaGames.Purchases = payments;
                    dynCall('v', callbackOnSuccess, []);
                })
                .catch(e => {
                    dynCall('v', callbackOnError, []);
                });
        },

        PurchasesGetCatalog: function(callbackOnSuccess, callbackOnError) {
            yaGames.Purchases.getCatalog()
                .then(products => {
                    var data = {products: products};
                    const dataString = yaGames.GetAllocatedString(JSON.stringify(data));
                    dynCall('vi', callbackOnSuccess, [dataString]);
                    _free(dataString);
                    return;
                })
                .catch(e => {
                    const dataString = yaGames.GetAllocatedString(e);
                    dynCall('vi', callbackOnError, [e]);
                    _free(dataString);
                    return;
                });
        },

        PurchasesPurchase: function(id, developerPayload, callbackOnSuccess, callbackOnError) {
            const idStr = UTF8ToString(id);
            const developerPayloadStr = UTF8ToString(developerPayload);
            yaGames.Purchases.purchase({id: idStr, developerPayload: developerPayloadStr})
                .then(purchase => {
                    const dataString = yaGames.GetAllocatedString(JSON.stringify(purchase));
                    dynCall('vi', callbackOnSuccess, [dataString]);
                    _free(dataString);
                    return;
                })
                .catch(e => {
                    const dataString = yaGames.GetAllocatedString(e);
                    dynCall('vi', callbackOnError, [e]);
                    _free(dataString);
                    return;
                });
        },

        PurchasesGetPurchases: function(callbackOnSuccess, callbackOnError) {
            yaGames.Purchases.getPurchases()
                .then(purchases => {
                    var data = {purchases: purchases};
                    const dataString = yaGames.GetAllocatedString(JSON.stringify(data));
                    dynCall('vi', callbackOnSuccess, [dataString]);
                    _free(dataString);
                    return;
                })
                .catch(e => {
                    const dataString = yaGames.GetAllocatedString(e);
                    dynCall('vi', callbackOnError, [e]);
                    _free(dataString);
                    return;
                });
        },
        
        PurchasesConsume: function(token, callbackOnSuccess, callbackOnError) {
            const tokenStr = UTF8ToString(token);

            yaGames.Purchases.consumePurchase(tokenStr)
                .then(() => {
                    dynCall('v', callbackOnSuccess, []);
                    return;
                })
                .catch(e => {
                    const dataString = yaGames.GetAllocatedString(e);
                    dynCall('vi', callbackOnError, [e]);
                    _free(dataString);
                    return;
                });
        },

        RemoteConfigsInitialize: function(callbackOnSuccess, callbackOnError) {
            yaGames.SDK.getFlags().then(flags => {
                
                let jsonArray = [];
                
                for (let key in flags) {
                    let value = flags[key];
                    jsonArray.push({ "key": key, "value": value });
                }

                let data = {data : jsonArray};
                
                let jsonResult = JSON.stringify(data);

                const dataString = yaGames.GetAllocatedString(jsonResult);
                
                dynCall('vi', callbackOnSuccess, [dataString]);
                _free(dataString);
                return;
            }).catch(e => {
                    const dataString = yaGames.GetAllocatedString(e);
                    dynCall('vi', callbackOnError, [e]);
                    _free(dataString);
                    return;
            });
        },
        
        RemoteConfigsInitializeWithClientParameters: function(parameters, callbackOnSuccess, callbackOnError) {
            const parametersStr = UTF8ToString(parameters);
            const parametersObj = JSON.parse(parametersStr).data;
            let convertedArray = parametersObj.map(item => {
                return { "name": item.key, "value": item.value };
            });
            
            yaGames.SDK.getFlags({
                defaultFlags: {},
                clientFeatures: convertedArray}).then(flags => {
                let jsonArray = [];

                for (let key in flags) {
                    let value = flags[key];
                    jsonArray.push({ "key": key, "value": value });
                }
                
                let data = {data : jsonArray};

                let jsonResult = JSON.stringify(data);

                const dataString = yaGames.GetAllocatedString(jsonResult);

                dynCall('vi', callbackOnSuccess, [dataString]);
                _free(dataString);
                return;
            }).catch(e => {
                    const dataString = yaGames.GetAllocatedString(e);
                    dynCall('vi', callbackOnError, [e]);
                    _free(dataString);
                    return;
            });
        },
        
        GetBannerErrorType: function (reason){
            switch (reason){
                case 'UNKNOWN':
                    return 0;
                case 'ADV_IS_NOT_CONNECTED':
                    return 1;
                default:
                    return 0;
            }
        },

        GetReviewType: function(reason) {
            switch (reason){
                case 'UNKNOWN':
                    return 0;
                case 'NO_AUTH':
                    return 1;
                case 'GAME_RATED':
                    return 2;
                case 'REVIEW_ALREADY_REQUESTED':
                    return 3;
                case 'REVIEW_WAS_REQUESTED':
                    return 4;
                default:
                    return 0;
            }
        },

        GetAllocatedString: function (string) {
            const stringBufferSize = lengthBytesUTF8(string) + 1;
            const stringBufferPtr = _malloc(stringBufferSize);
            stringToUTF8(string, stringBufferPtr, stringBufferSize);
            return stringBufferPtr;
        },

        ServerTime: function(callback) {
            const serverTime = yaGames.SDK.serverTime();
            const dataString = yaGames.GetAllocatedString(serverTime.toString());
            dynCall('vi', callback, [dataString]);
            _free(dataString);
        },
    },

    YaGamesInitialize: function (callbackSuccess, callbackError){
        yaGames.Initialize(callbackSuccess, callbackError);
    },
    
    YaGamesReady: function (callbackSuccess, callbackError){
        yaGames.GameReady(callbackSuccess, callbackError);
    },

    YaGamesStart: function (callbackSuccess, callbackError){
        yaGames.GameStart(callbackSuccess, callbackError);
    },

    YaGamesStop: function (callbackSuccess, callbackError){
        yaGames.GameStop(callbackSuccess, callbackError);
    },
        
    YaGamesGetDeviceType: function (){
        return yaGames.GetDeviceType();
    },
    
    YaGamesGetEnvironment: function (){
        return yaGames.GetEnvironment();
    },
    
    YaGamesGetPlayer: function (signed, callbackSuccess, callbackError){
        yaGames.GetPlayer(signed, callbackSuccess, callbackError);
    },
    
    YaGamesGetId: function (){
        return yaGames.GetId();
    },
    
    YaGamesGetName: function (){
        return yaGames.GetName();
    },
    
    YaGamesGetMode: function (){
        return yaGames.GetMode();
    },

    YaGamesGetPayingStatus: function (){
        return yaGames.GetPayingStatus();
    },
    
    YaGamesGetPhoto: function (size){
        return yaGames.GetPhoto(size);
    },
    
    YaGamesSaveData: function(key, value, callbackSuccess, callbackError){
        yaGames.SaveData(key, value, callbackSuccess, callbackError);
    },
    
    YaGamesSaveDataAll: function(callbackSuccess, callbackError){
        yaGames.SaveDataAll(callbackSuccess, callbackError);
    },
    
    YaGamesLoadData: function(key, callbackSuccess, callbackError){
        yaGames.LoadData(key, callbackSuccess, callbackError);
    },
    
    YaGamesCanReview: function(callbackSuccess, callbackError){
        yaGames.CanReview(callbackSuccess, callbackError);
    },
    
    YaGamesRequestReview: function(callbackSuccess, callbackError){
        yaGames.RequestReview(callbackSuccess, callbackError);
    },

    YaGamesShowInterstitial: function(callbackOnOpen, callbackOnClose, callbackOnError, callbackOnOffline){
        yaGames.ShowInterstitial(callbackOnOpen, callbackOnClose, callbackOnError, callbackOnOffline);
    },

    YaGamesShowRewarded: function(callbackOnOpen, callbackOnClose, callbackOnError, callbackOnRewarded){
        yaGames.ShowRewarded(callbackOnOpen, callbackOnClose, callbackOnError, callbackOnRewarded);
    },
    
    YaGamesShowBanner: function(callbackOnOpen, callbackOnError){
        yaGames.ShowBanner(callbackOnOpen, callbackOnError);
    },
    
    YaGamesHideBanner: function(callbackOnHided, callbackOnError){
        yaGames.HideBanner(callbackOnHided, callbackOnError);
    },

    YaGamesCreateShortcut: function(callbackOnSuccess, callbackOnError){
        yaGames.CreateShortcut(callbackOnSuccess, callbackOnError);
    },
    
    YaGamesCanCreateShortcut: function(callbackOnSuccess, callbackOnError){
        yaGames.CanCreateShortcut(callbackOnSuccess, callbackOnError);
    },
    
    YaLeaderboardGetDescription: function (id, callbackSuccess, callbackError){
        yaGames.LeaderboardGetDescription(id, callbackSuccess, callbackError);
    },

    YaLeaderboardSetScore: function (id, score, callbackSuccess, callbackError){
        yaGames.LeaderboardSetScore(id, score, callbackSuccess, callbackError);
    },

    YaLeaderboardGetPlayerData: function (id, callbackSuccess, callbackError){
        yaGames.LeaderboardGetPlayerData(id, callbackSuccess, callbackError);
    },
    
    YaLeaderboardGetEntries: function (id, includeUser, quantityAround, quantityTop, callbackSuccess, callbackError){
        yaGames.LeaderboardGetEntries(id, includeUser, quantityAround, quantityTop, callbackSuccess, callbackError);
    },

    YaPurchasesInitialize: function (callbackSuccess, callbackError){
        yaGames.PurchasesInitialize(callbackSuccess, callbackError);
    },

    YaPurchasesGetCatalog: function (callbackSuccess, callbackError){
        yaGames.PurchasesGetCatalog(callbackSuccess, callbackError);
    },

    YaPurchasesPurchase: function (id, developerPayload, callbackSuccess, callbackError){
        yaGames.PurchasesPurchase(id, developerPayload, callbackSuccess, callbackError);
    },

    YaPurchasesGetPurchases: function (callbackSuccess, callbackError){
        yaGames.PurchasesGetPurchases(callbackSuccess, callbackError);
    },
    
    YaPurchasesConsume: function (token, callbackSuccess, callbackError){
        yaGames.PurchasesConsume(token, callbackSuccess, callbackError);
    },

    YaRemoteConfigsInitialize: function (callbackSuccess, callbackError){
        yaGames.RemoteConfigsInitialize(callbackSuccess, callbackError);
    },

    YaRemoteConfigsInitializeWithClientParameters: function (parameters, callbackSuccess, callbackError){
        yaGames.RemoteConfigsInitializeWithClientParameters(parameters, callbackSuccess, callbackError);
    },

    YaGamesServerTime: function (callback){
        yaGames.ServerTime(callback);
    },
}

autoAddDeps(YaGames, '$yaGames');
mergeInto(LibraryManager.library, YaGames);