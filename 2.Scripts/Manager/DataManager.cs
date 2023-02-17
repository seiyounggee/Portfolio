using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class DataManager : MonoSingleton<DataManager>
{
    public Action firebaseUserValueChangedCallback = null;
    public Action firebaseBasicValueChangedCallback = null;
    public Action firebaseCarValueChangedCallback = null;
    public Action firebaseMapValueChangedCallback = null;

    FirebaseApp app;
    [ReadOnly] public bool isBasicDataLoaded = false;
    private bool isBasicData_GameDataLoaded = false;
    private bool isBasicData_CamDataLoaded = false;
    private bool isBasicData_PlayerDataLoaded = false;
    private bool isBasicData_FoodTruckDataLoaded = false;
    private bool isBasicData_ChargePadDataLoaded = false;
    private bool isBasicData_ChargeZoneDataLoaded = false;
    private bool isBasicData_GroundDataLoaded = false;

    [ReadOnly] public bool isUserDataLoaded = false;
    [ReadOnly] public bool isCarDataLoaded = false;
    [ReadOnly] public bool isMapDataLoaded = false;
    [ReadOnly] public bool isFirebaseConnected = false;
    [ReadOnly] private string currentUserId = "";

    public void SetFirebaseConnection()
    {
        if (isFirebaseConnected)
            return;

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.

                FirebaseDatabase.DefaultInstance
                .GetReference("BASIC_VALUES")
                .Child("GameData")
                .ValueChanged += HandleBasicValue_GameDataChanged;
                FirebaseDatabase.DefaultInstance
                .GetReference("BASIC_VALUES")
                .Child("CameraData")
                .ValueChanged += HandleBasicValue_CameraDataChanged;
                FirebaseDatabase.DefaultInstance
                .GetReference("BASIC_VALUES")
                .Child("PlayerData")
                .ValueChanged += HandleBasicValue_PlayerDataChanged;
                FirebaseDatabase.DefaultInstance
                .GetReference("BASIC_VALUES")
                .Child("FoodTruckData")
                .ValueChanged += HandleBasicValue_FoodTruckDataChanged;
                FirebaseDatabase.DefaultInstance
                .GetReference("BASIC_VALUES")
                .Child("ChargePadData")
                .ValueChanged += HandleBasicValue_ChargePadDataChanged;
                 FirebaseDatabase.DefaultInstance
                .GetReference("BASIC_VALUES")
                .Child("ChargeZoneData")
                .ValueChanged += HandleBasicValue_ChargeZoneDataChanged;
                FirebaseDatabase.DefaultInstance
                .GetReference("BASIC_VALUES")
                .Child("GroundData")
                .ValueChanged += HandleBasicValue_GroundDataChanged;

                FirebaseDatabase.DefaultInstance
                .GetReference("CAR_VALUES")
                .ValueChanged += HandleCarValueChanged;

                FirebaseDatabase.DefaultInstance
                .GetReference("MAP_VALUES")
                .ValueChanged += HandleMapValueChanged;

                if (task.IsCompleted)
                {
                    Debug.Log("Firebase is Ready!");
                    isFirebaseConnected = true;
                }
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    public void LoadBasicData(System.Action<bool> callback = null)
    {
        LoadBasicData_GameData(callback);
        LoadBasicData_CameraData(callback);
        LoadBasicData_PlayerData(callback);
        LoadBasicData_FoodTruckData(callback);
        LoadBasicData_ChargePadData(callback);
        LoadBasicData_ChargeZoneData(callback);
        LoadBasicData_GroundData(callback);
    }

    private void LoadBasicData_GameData(System.Action<bool> callback = null)
    {
                FirebaseDatabase.DefaultInstance
        .GetReference("BASIC_VALUES")
        .Child("GameData")
        .GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                // Handle the error...
                Debug.Log("Firebase Database Error......!");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    string json = snapshot.GetRawJsonValue();
                    var data = JsonUtility.FromJson<BASIC_VALUES_FIRBASE.GameData>(json);
                    basicData.gameData = data;

                    isBasicData_GameDataLoaded = true;
                    CheckForBasicGameLoad();

                    callback?.Invoke(true);
                }
            }
        });
    }

    private void LoadBasicData_CameraData(System.Action<bool> callback = null)
    { 
             FirebaseDatabase.DefaultInstance
        .GetReference("BASIC_VALUES")
        .Child("CameraData")
        .GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                // Handle the error...
                Debug.Log("Firebase Database Error......!");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    string json = snapshot.GetRawJsonValue();
                    var data = JsonUtility.FromJson<BASIC_VALUES_FIRBASE.CameraData>(json);
                    basicData.cameraData = data;

                    isBasicData_CamDataLoaded = true;
                    CheckForBasicGameLoad();

                    callback?.Invoke(true);
                }
            }
        });
    }

    private void LoadBasicData_PlayerData(System.Action<bool> callback = null)
    {
         FirebaseDatabase.DefaultInstance
        .GetReference("BASIC_VALUES")
        .Child("PlayerData")
        .GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                // Handle the error...
                Debug.Log("Firebase Database Error......!");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    string json = snapshot.GetRawJsonValue();
                    var data = JsonUtility.FromJson<BASIC_VALUES_FIRBASE.PlayerData>(json);
                    basicData.playerData = data;

                    isBasicData_PlayerDataLoaded = true;
                    CheckForBasicGameLoad();

                    callback?.Invoke(true);
                }
            }
        });
    }

    private void LoadBasicData_FoodTruckData(System.Action<bool> callback = null)
    {
         FirebaseDatabase.DefaultInstance
        .GetReference("BASIC_VALUES")
        .Child("FoodTruckData")
        .GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                // Handle the error...
                Debug.Log("Firebase Database Error......!");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    string json = snapshot.GetRawJsonValue();
                    var data = JsonUtility.FromJson<BASIC_VALUES_FIRBASE.FoodTruckData>(json);
                    basicData.foodTruckData = data;

                    isBasicData_FoodTruckDataLoaded = true;
                    CheckForBasicGameLoad();

                    callback?.Invoke(true);
                }
            }
        });
    }

    private void LoadBasicData_ChargePadData(System.Action<bool> callback = null)
    {
        FirebaseDatabase.DefaultInstance
       .GetReference("BASIC_VALUES")
       .Child("ChargePadData")
       .GetValueAsync().ContinueWithOnMainThread(task => {
           if (task.IsFaulted)
           {
                // Handle the error...
                Debug.Log("Firebase Database Error......!");
           }
           else if (task.IsCompleted)
           {
               DataSnapshot snapshot = task.Result;
               if (snapshot.Exists)
               {
                   string json = snapshot.GetRawJsonValue();
                   var data = JsonUtility.FromJson<BASIC_VALUES_FIRBASE.ChargePadData>(json);
                   basicData.chargePadData = data;

                   isBasicData_ChargePadDataLoaded = true;
                   CheckForBasicGameLoad();
             
                   callback?.Invoke(true);
               }
           }
       });
    }

    private void LoadBasicData_ChargeZoneData(System.Action<bool> callback = null)
    {
        FirebaseDatabase.DefaultInstance
       .GetReference("BASIC_VALUES")
       .Child("ChargeZoneData")
       .GetValueAsync().ContinueWithOnMainThread(task => {
           if (task.IsFaulted)
           {
               // Handle the error...
               Debug.Log("Firebase Database Error......!");
           }
           else if (task.IsCompleted)
           {
               DataSnapshot snapshot = task.Result;
               if (snapshot.Exists)
               {
                   string json = snapshot.GetRawJsonValue();
                   var data = JsonUtility.FromJson<BASIC_VALUES_FIRBASE.ChargeZoneData>(json);
                   basicData.chargeZoneData = data;

                   isBasicData_ChargeZoneDataLoaded = true;
                   CheckForBasicGameLoad();

                   callback?.Invoke(true);
               }
           }
       });
    }

    private void LoadBasicData_GroundData(System.Action<bool> callback = null)
    {
        FirebaseDatabase.DefaultInstance
       .GetReference("BASIC_VALUES")
       .Child("GroundData")
       .GetValueAsync().ContinueWithOnMainThread(task => {
           if (task.IsFaulted)
           {
               // Handle the error...
               Debug.Log("Firebase Database Error......!");
           }
           else if (task.IsCompleted)
           {
               DataSnapshot snapshot = task.Result;
               if (snapshot.Exists)
               {
                   string json = snapshot.GetRawJsonValue();
                   var data = JsonUtility.FromJson<BASIC_VALUES_FIRBASE.GroundData>(json);
                   basicData.groundData = data;

                   isBasicData_GroundDataLoaded = true;
                   CheckForBasicGameLoad();

                   callback?.Invoke(true);
               }
           }
       });
    }

    private void CheckForBasicGameLoad()
    {
        if (isBasicDataLoaded == false)
        {
            if (isBasicData_GameDataLoaded 
                && isBasicData_PlayerDataLoaded
                && isBasicData_CamDataLoaded 
                && isBasicData_ChargePadDataLoaded
                && isBasicData_FoodTruckDataLoaded
                && isBasicData_ChargeZoneDataLoaded
                && isBasicData_GroundDataLoaded)
            {
                UtilityCommon.ColorLog("FIREBASE BASIC DATABASE LOADED!", UtilityCommon.DebugColor.Cyan);
                isBasicDataLoaded = true;
            }
        }
    }


    public void LoadUserData(string userId)
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        FirebaseDatabase.DefaultInstance
            .GetReference("USER_DATA")
            .Child(userId)
            .GetValueAsync().ContinueWithOnMainThread(task => 
            {
                if (task.IsFaulted)       
                {
                    //Handle Error
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        string json = snapshot.GetRawJsonValue();
                        UtilityCommon.ColorLog("FIREBASE USER DATABASE LOADED!", UtilityCommon.DebugColor.Cyan);
                        userData = JsonUtility.FromJson<USER_DATA>(json);
                        currentUserId = userData.UserId;

                        isUserDataLoaded = true;
                    }
                    else
                    {
                        //No Data...!
                        UtilityCommon.ColorLog(">NO EXISTING FIREBASE USER DATA ASSIGINING NEW DATA TO FIREBASE", UtilityCommon.DebugColor.Cyan);
                        SetNewUserData(userId);
                        SaveUserData(userId); //Make New Account!
                    }

                    FirebaseDatabase.DefaultInstance
                    .GetReference("USER_DATA")
                    .Child(currentUserId)
                    .ValueChanged += HandleUserValueChanged;
                }
            });
    }

    public void SetNewUserData(string userId)
    {
        userData = new USER_DATA();

        userData.UserId = userId;

        userData.LoginCount = 0;

        userData.MyCarID = (int)CAR_DATA.CarID.One;
        userData.MyCharacterID = (int)CHARACTER_DATA.CharacterType.One;

        if (userId.Length > 6)
            userData.NickName = "User_" + userId[0] + userId[1] + userId[2] + userId[3] + userId[4] + userId[5];
        else
            userData.NickName = "User";

        userData.IsBanned = false;
        userData.IdCreatedTime = DateTime.UtcNow.ToString();

        isUserDataLoaded = true;
    }

    public void SaveUserData()
    {
        if (string.IsNullOrEmpty(currentUserId) == false)
        {
            UtilityCommon.ColorLog("FIREBASE USER DATABASE SAVED!", UtilityCommon.DebugColor.Yellow);

            StartCoroutine(SaveUserData_Coroutine());
        }
    }

    public void SaveUserData(string userId)
    {
        UtilityCommon.ColorLog("FIREBASE USER DATABASE SAVED!", UtilityCommon.DebugColor.Yellow);

        currentUserId = userId;
        StartCoroutine(SaveUserData_Coroutine());
    }

    private IEnumerator SaveUserData_Coroutine()
    {
        yield return null;

        string json = JsonUtility.ToJson(userData);
        FirebaseDatabase.DefaultInstance
       .GetReference("USER_DATA")
       .Child(currentUserId).SetRawJsonValueAsync(json);
    }



    public void SaveCarData()
    {
        UtilityCommon.ColorLog("FIREBASE CAR DATABASE SAVED!", UtilityCommon.DebugColor.Yellow);

        for (int i = 0; i < 4; i++)
        {
            carData.CarDataList[i].carId = i;
        }

        StartCoroutine(SaveCarData_Coroutine());
    }

    private IEnumerator SaveCarData_Coroutine()
    {
        yield return null;

        string json = JsonUtility.ToJson(carData);
        FirebaseDatabase.DefaultInstance
       .GetReference("CAR_VALUES")
       .SetRawJsonValueAsync(json);

        isCarDataLoaded = true;
    }

    public void LoadCarData()
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        FirebaseDatabase.DefaultInstance
            .GetReference("CAR_VALUES")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    //Handle Error
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        string json = snapshot.GetRawJsonValue();
                        UtilityCommon.ColorLog("FIREBASE CAR DATABASE LOADED!", UtilityCommon.DebugColor.Cyan);
                        carData = JsonUtility.FromJson<CAR_DATA>(json);

                        isCarDataLoaded = true;
                    }
                    else
                    {
                        //No Data...!
                        Debug.Log("<color=cyan>NO EXISTING FIREBASE CAR DATA</color>");

                        SaveCarData();
                    }
                }
            });
    }

    public void SaveMapData()
    {
        UtilityCommon.ColorLog("FIREBASE MAP DATABASE SAVED!", UtilityCommon.DebugColor.Yellow);

        if (mapData == null)
            return;

        for (int i = 0; i < 2; i++)
        {
            mapData.MapDataList[i].id = i;
        }

        StartCoroutine(SaveMapData_Coroutine());
    }

    private IEnumerator SaveMapData_Coroutine()
    {
        yield return null;

        string json = JsonUtility.ToJson(mapData);
        FirebaseDatabase.DefaultInstance
       .GetReference("MAP_VALUES")
       .SetRawJsonValueAsync(json);

        isMapDataLoaded = true;
    }

    public void LoadMapData()
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        FirebaseDatabase.DefaultInstance
            .GetReference("MAP_VALUES")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    //Handle Error
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        string json = snapshot.GetRawJsonValue();
                        UtilityCommon.ColorLog("FIREBASE MAP DATABASE LOADED!", UtilityCommon.DebugColor.Cyan);
                        mapData = JsonUtility.FromJson<MAP_DATA>(json);

                        isMapDataLoaded = true;
                    }
                    else
                    {
                        //No Data...!
                        Debug.Log("<color=cyan>NO EXISTING FIREBASE MAP DATA</color>");

                        SaveMapData();
                    }
                }
            });
    }

    public void SaveMatchRecordData(MATCH_RECORD_DATA.MatchResultInfo data, string matchName)
    {
        UtilityCommon.ColorLog("FIREBASE MATCH RECORD DATABASE SAVED!", UtilityCommon.DebugColor.Yellow);

        if (data == null)
            return;

        StartCoroutine(SaveMatchRecordData_Coroutine(data, matchName));
    }

    private IEnumerator SaveMatchRecordData_Coroutine(MATCH_RECORD_DATA.MatchResultInfo data, string matchName)
    {
        yield return null;

        string json = JsonUtility.ToJson(data);
        FirebaseDatabase.DefaultInstance
       .GetReference("MATCH_DATA")
       .Child(matchName)
       .SetRawJsonValueAsync(json);
    }

    public void LoadMatchRecordData()
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        FirebaseDatabase.DefaultInstance
            .GetReference("MATCH_DATA")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    //Handle Error
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        string json = snapshot.GetRawJsonValue();


                        UtilityCommon.ColorLog("FIREBASE MATCH RECORD DATABASE LOADED!", UtilityCommon.DebugColor.Cyan);

                        foreach (var s in snapshot.Children)
                        {
                            var data = JsonUtility.FromJson<MATCH_RECORD_DATA.MatchResultInfo>(s.GetRawJsonValue());
                            matchRecordData.MatchResultInfoList.Add(data);
                        }
                    }
                    else
                    {
                        //No Data...!
                        UtilityCommon.ColorLog(">NO EXISTING FIREBASE MATCH RECORD DATA", UtilityCommon.DebugColor.Cyan);
                    }
                }
            });
    }







    //값이 바뀌면 이벤트 형식으로 자동 호출됨
    void HandleBasicValue_GameDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot == null)
            return;

        //이거 없으면 data initializing 할떄 이상하게 나옴...! 꼭 포함 시키자
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.None
            || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Initialize)
            return;

        // Do something with the data in args.Snapshot
        string json = args.Snapshot.GetRawJsonValue();
        if (string.IsNullOrEmpty(json) == false)
        {
            UtilityCommon.ColorLog("FIREBASE BASIC GameData DATABASE HAS CHANGED....! OVERRIDING NEW VALUES...", UtilityCommon.DebugColor.Cyan);
            var data = JsonUtility.FromJson<BASIC_VALUES_FIRBASE.GameData>(json);
            basicData.gameData = data;
            firebaseBasicValueChangedCallback?.Invoke();
        }
    }

    void HandleBasicValue_CameraDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot == null)
            return;

        //이거 없으면 data initializing 할떄 이상하게 나옴...! 꼭 포함 시키자
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.None
            || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Initialize)
            return;

        // Do something with the data in args.Snapshot
        string json = args.Snapshot.GetRawJsonValue();
        if (string.IsNullOrEmpty(json) == false)
        {
            UtilityCommon.ColorLog("FIREBASE BASIC CameraData DATABASE HAS CHANGED....! OVERRIDING NEW VALUES...", UtilityCommon.DebugColor.Cyan);
            var data = JsonUtility.FromJson<BASIC_VALUES_FIRBASE.CameraData>(json);
            basicData.cameraData = data;
            firebaseBasicValueChangedCallback?.Invoke();
        }
    }

    void HandleBasicValue_PlayerDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot == null)
            return;

        //이거 없으면 data initializing 할떄 이상하게 나옴...! 꼭 포함 시키자
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.None
            || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Initialize)
            return;

        // Do something with the data in args.Snapshot
        string json = args.Snapshot.GetRawJsonValue();
        if (string.IsNullOrEmpty(json) == false)
        {
            UtilityCommon.ColorLog("FIREBASE BASIC PlayerData DATABASE HAS CHANGED....! OVERRIDING NEW VALUES...", UtilityCommon.DebugColor.Cyan);
            var data = JsonUtility.FromJson<BASIC_VALUES_FIRBASE.PlayerData>(json);
            basicData.playerData = data;
            firebaseBasicValueChangedCallback?.Invoke();
        }
    }

    void HandleBasicValue_FoodTruckDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot == null)
            return;

        //이거 없으면 data initializing 할떄 이상하게 나옴...! 꼭 포함 시키자
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.None
            || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Initialize)
            return;

        // Do something with the data in args.Snapshot
        string json = args.Snapshot.GetRawJsonValue();
        if (string.IsNullOrEmpty(json) == false)
        {
            UtilityCommon.ColorLog("FIREBASE BASIC FoodTruckData DATABASE HAS CHANGED....! OVERRIDING NEW VALUES...", UtilityCommon.DebugColor.Cyan);
            var data = JsonUtility.FromJson<BASIC_VALUES_FIRBASE.FoodTruckData>(json);
            basicData.foodTruckData = data;
            firebaseBasicValueChangedCallback?.Invoke();
        }
    }

    void HandleBasicValue_ChargePadDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot == null)
            return;

        //이거 없으면 data initializing 할떄 이상하게 나옴...! 꼭 포함 시키자
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.None
            || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Initialize)
            return;

        // Do something with the data in args.Snapshot
        string json = args.Snapshot.GetRawJsonValue();
        if (string.IsNullOrEmpty(json) == false)
        {
            UtilityCommon.ColorLog("FIREBASE BASIC ChargePad DATABASE HAS CHANGED....! OVERRIDING NEW VALUES...", UtilityCommon.DebugColor.Cyan);
            var data = JsonUtility.FromJson<BASIC_VALUES_FIRBASE.ChargePadData>(json);
            basicData.chargePadData = data;
            firebaseBasicValueChangedCallback?.Invoke();
        }
    }

    void HandleBasicValue_ChargeZoneDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot == null)
            return;

        //이거 없으면 data initializing 할떄 이상하게 나옴...! 꼭 포함 시키자
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.None
            || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Initialize)
            return;

        // Do something with the data in args.Snapshot
        string json = args.Snapshot.GetRawJsonValue();
        if (string.IsNullOrEmpty(json) == false)
        {
            UtilityCommon.ColorLog("FIREBASE BASIC ChargeZone DATABASE HAS CHANGED....! OVERRIDING NEW VALUES...", UtilityCommon.DebugColor.Cyan);
            var data = JsonUtility.FromJson<BASIC_VALUES_FIRBASE.ChargeZoneData>(json);
            basicData.chargeZoneData = data;
            firebaseBasicValueChangedCallback?.Invoke();
        }
    }

    void HandleBasicValue_GroundDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot == null)
            return;

        //이거 없으면 data initializing 할떄 이상하게 나옴...! 꼭 포함 시키자
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.None
            || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Initialize)
            return;

        // Do something with the data in args.Snapshot
        string json = args.Snapshot.GetRawJsonValue();
        if (string.IsNullOrEmpty(json) == false)
        {
            UtilityCommon.ColorLog("FIREBASE BASIC Ground DATABASE HAS CHANGED....! OVERRIDING NEW VALUES...", UtilityCommon.DebugColor.Cyan);
            var data = JsonUtility.FromJson<BASIC_VALUES_FIRBASE.GroundData>(json);
            basicData.groundData = data;
            firebaseBasicValueChangedCallback?.Invoke();
        }
    }



    void HandleCarValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot == null)
            return;

        //이거 없으면 data initializing 할떄 이상하게 나옴...! 꼭 포함 시키자
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.None
            || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Initialize)
            return;

        // Do something with the data in args.Snapshot
        string json = args.Snapshot.GetRawJsonValue();
        if (string.IsNullOrEmpty(json) == false)
        {
            UtilityCommon.ColorLog("FIREBASE CAR DATABASE HAS CHANGED....! OVERRIDING NEW VALUES...", UtilityCommon.DebugColor.Cyan);
            carData = JsonUtility.FromJson<CAR_DATA>(json);
            firebaseCarValueChangedCallback?.Invoke();
        }
    }

    void HandleUserValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot == null)
            return;

        //이거 없으면 data initializing 할떄 이상하게 나옴...! 꼭 포함 시키자
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.None
            || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Initialize)
            return;

        // Do something with the data in args.Snapshot
        string json = args.Snapshot.GetRawJsonValue();
        if (string.IsNullOrEmpty(json) == false)
        {
            UtilityCommon.ColorLog("FIREBASE USER DATABASE HAS CHANGED....! OVERRIDING NEW VALUES...", UtilityCommon.DebugColor.Cyan);
            userData = JsonUtility.FromJson<USER_DATA>(json);
            firebaseUserValueChangedCallback?.Invoke();
        }
    }

    void HandleMapValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot == null)
            return;

        //이거 없으면 data initializing 할떄 이상하게 나옴...! 꼭 포함 시키자
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.None
            || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Initialize)
            return;

        // Do something with the data in args.Snapshot
        string json = args.Snapshot.GetRawJsonValue();
        if (string.IsNullOrEmpty(json) == false)
        {
            UtilityCommon.ColorLog("FIREBASE MAP DATABASE HAS CHANGED....! OVERRIDING NEW VALUES...", UtilityCommon.DebugColor.Cyan);
            mapData = JsonUtility.FromJson<MAP_DATA>(json);
            firebaseMapValueChangedCallback?.Invoke();
        }
    }


    public BASIC_VALUES_FIRBASE basicData = new BASIC_VALUES_FIRBASE();
    [System.Serializable]
    public class BASIC_VALUES_FIRBASE
    {
        public CameraData cameraData = new CameraData();
        public PlayerData playerData = new PlayerData();
        public GameData gameData = new GameData();
        public FoodTruckData foodTruckData = new FoodTruckData();
        public ChargePadData chargePadData = new ChargePadData();
        public ChargeZoneData chargeZoneData = new ChargeZoneData();
        public GroundData groundData = new GroundData();

        [Serializable]
        public class CameraData
        {
            public float CAMERA_HEIGHT = 12f; //카메라 높이 조절
            public float CAMERA_DIST = 7f; //카메라 거리 조절
            public float CAMERA_POSITION_SLERP_SPEED = 3.5f; //카메라 Position 따라 붙는 속도(?)
            public float CAMERA_ROTATION_SLERP_SPEED = 17f; //카메라 Rotation 따라 붙는 속도(?)
            public float CAMERA_POSITION_SLERP_SPEED_CHANGINGLINE = 3.5f;
            public float CAMERA_ROTATION_SLERP_SPEED_CHANGINGLINE = 12f;
            public float CAMERA_TARGET_Y_OFFSET = 4f; //카메라의 타켓 offset
            public int CAMERA_FIELD_OF_VIEW = 65; //카메라 원근감(?) field of view
        }

        [Serializable]
        public class PlayerData
        {
            public float PLAYER_REDUCE_STRENGTH_AFTER_FINISHLINE = 10f; //Finish Line 통과 후 속도 줄이는 속도
            public float PLAYER_MOVE_INDEX_CHECK_MIN_RANGE = 15f; //next index 찾을때 찾을 범위
            public int PLAYER_MOVE_INDEX_CHECK_NUMBER = 7; //next index 몇개까지 찾을지
            public float PLAYER_MOVE_SPEED_BUFF = 0.05f; //뒤쪽에 플레이어에 스피드 버프 = 순위X값

            public float PLAYER_LAG_MAX_DISTANCE = 2f; //Lag Max
            public float PLAYER_LAG_MIN_DISTANCE = 0.25f; //Lag로 판단하는 dist
            public float PLAYER_LAG_MAX_RATE = 0.6f; //Lag를 어느정도 수치로 조절할지 ... 이때 MAX 값
            public float PLAYER_LAG_MIN_RATE = 0.06f; //Lag를 어느정도 수치로 조절할지 ... 이때 MIN 값
            public float PLAYER_LAG_TELEPORT_TIME = 1.5f; //일정 시간 이상 lag 할 경우 teleport 시켜버리기
            public float PLAYER_LAG_TELEPORT_DIST = 15f; //일정 거리 이상 lag 할 경우 teleport 시켜버리기

            public float PLAYER_LAG_PING_ADJUSTMENT_RATE = 1f;

            public float PLAYER_DIRECTION_LERP_MAX_SPEED = 15f;
            public float PLAYER_DIRECTION_LERP_MIN_SPEED = 10f;
            public float PLAYER_DIRECTION_LERP_ADDITIONAL_ANGLE_MIN_SPEED = 5f;
            public float PLAYER_DIRECTION_LERP_ADDITIONAL_ANGLE_MAX_SPEED = 29f;

            public bool PLAYER_RIGIDBODY_USE_GRAVITY = true;

            public bool PLAYER_OUT_OF_BOUNDARY_USE = false; //isOutOfBoundary 사용여부
            public float PLAYER_OUT_OF_BOUNDARY_TIME = 3f; //isOutOfBoundary 이후 몇 초후 정상 궤도로 돌릴지
            public float PLAYER_COLLISION_DELAY_COOLTIME = 0.1f; //충돌 후 collision 쿨 타임.... (충돌된 당사자 한해)

            public float PLAYER_INDICATOR_CHECK_BEHIND_DIST = 30f; //어느정리 거리 체크하는지 (인디케이터 용)

            public float PLAYER_VALID_REACH_DEST = 1.5f;

            public int PLAYER_SHOW_MOVE_SPEED_MULTIPYER = 1;
        }

        [Serializable]
        public class GameData
        {
            public int GAME_END_COUNTDOWN_TIME = 10; //1등 통과 후 게임 종료 카운트 다운
            public int GAME_START_COUNTDOWN_TIME = 5; //게임 시작 카운트 다운
            public float GAME_TIME_WAITING_BEFORE_START = 10; //Start Race 전 대기 시간... 대기 시간 이후 플레이가 없을시 solo mode
            public float GAME_INPUT_SWIPE_MIN_LENGHT = 0.05f; //Ingame input 좌우 부스터 사용하기 위한 스와이프 최소 거리
            public float GAME_INPUT_DELAY = 0.2f; //Ingame input 다음 input까지 딜레이 (연속 input 막아주는..)
            public bool GAME_INPUT_USE_NETWORK_RESPONSE = false; //네트워크 응답받고 인풋 결과를 받을지..
            public bool GAME_FOODTRUCK_ACTIVATE = true; //푸드트럭 노출 여부
            public bool GAME_MINIMAP_ACTIVATE = true; //미니맵 사용 여부
            public bool GAME_SPEEDBUFF_ACTIVATE = true; //뒤쪽 유저 스피드 버프 사용여부
            public int GAME_MAP_ID = 0; //-1일 경우 랜덤
            public float GAME_AUTO_LEAVE_ROOM_TIME = 30f; //게임 종료 후 x초 후 Room 강제로 나가는 시간
        }

        [Serializable]
        public class FoodTruckData
        {
            public float FOODTRUCK_DEFAULT_MOVE_SPEED = 15f;
            public float FOODTRUCK_DEFAULT_MOVE_SPEED_CHANGINGLANE = 25f;
            public float FOODTRUCK_DEFAULT_ROTATION_SPEED = 15f;
            public float FOODTRUCK_DIRECTION_LERP_SPEED = 7f;
            public float FOODTRUCK_CHANGELANE_MIN_TIME = 10f;
            public float FOODTRUCK_CHANGELANE_MAX_TIME = 30f;
            public float FOODTRUCK_LAG_DIST = 10f;
            public float FOODTRUCK_HIT_COOLTIME = 2f;
            public int FOODTRUCK_MOVEINDEX_CHECK_NUMBER = 5;
            public float FOODTRUCK_MOVEINDEX_CHECK_MIN_RANGE = 15f;
            public float FOODTRUCK_NETWORK_UPDATE_RATE = 0.2f;
            public float FOODTRUCK_BOOSTER_MAX_SPEED = 10f;
            public float FOODTRUCK_BOOSTER_DECREASE_SPEED = 5f;
            public float FOODTRUCK_BOOSTER_MAXSPEED_DURATION_TIME = 0.5f;
        }

        [Serializable]
        public class ChargePadData
        {
            public float CHARGEPAD_LV2_CHARGE_RESET_TIME = 5f;
            public float CHARGEPAD_LV3_CHARGE_RESET_TIME = 3f;

            public int CHARGEPAD_LV1_CHARGE_AMOUNT = 20; //Lv1 패드 충전 양
            public int CHARGEPAD_LV2_CHARGE_AMOUNT = 30; //Lv2 패드 충전 양
            public int CHARGEPAD_LV3_CHARGE_AMOUNT = 45; //Lv3 패드 충전 양

            public float CHARGEPAD_LV1_MAX_SPEED = 7f; //Lv1 패드 부스터 속도
            public float CHARGEPAD_LV1_DECREASE_SPEED = 4f; //얼마나 빠르게 소모가 될지
            public float CHARGEPAD_LV2_MAX_SPEED = 8f; //Lv2 패드 부스터 속도
            public float CHARGEPAD_LV2_DECREASE_SPEED = 4f; //얼마나 빠르게 소모가 될지
            public float CHARGEPAD_LV3_MAX_SPEED = 9f; //Lv3 패드 부스터 속도
            public float CHARGEPAD_LV3_DECREASE_SPEED = 4f; //얼마나 빠르게 소모가 될지

            public float CHARGEPAD_LV1_ANIM_SPEED = 1f;
            public float CHARGEPAD_LV2_ANIM_SPEED = 1.2f;
            public float CHARGEPAD_LV3_ANIM_SPEED = 1.5f;
        }

        [Serializable]
        public class ChargeZoneData
        {
            public float CHARGEZONE_CHARGE_COOLTIME = 5f;
            public int CHARGEZONE_CHARGE_AMOUNT = 3;
        }

        [Serializable]
        public class GroundData
        {
            public float GROUND_MUD_DECREASE_SPEED = 10f;
        }
    }


    [SerializeField] public USER_DATA userData = new USER_DATA();
    [System.Serializable]
    public class USER_DATA
    {
        public string UserId;
        public string NickName;

        public long LoginCount;

        public int MyCarID;
        public int MyCharacterID;

        public bool IsBanned = false;
        public string IdCreatedTime;

        public int IngameControlType = 0;

        #region Ingame Releated Stats

        public long GameStats_GamePlayCount;

        public long GameStats_FirstPlaceCount;
        public long GameStats_SecondPlaceCount;
        public long GameStats_ThirdPlaceCount;
        public long GameStats_FourthPlaceCount;
        public long GameStats_FivthPlaceCount;
        public long GameStats_RetireCount;

        #endregion
    }

    [SerializeField] public CAR_DATA carData = new CAR_DATA();
    [System.Serializable]
    public class CAR_DATA
    {
        public enum CarID { One, Two, Three, Four}
        [SerializeField] public List<CarInfo> CarDataList = new List<CarInfo>();

        [System.Serializable]
        public class CarInfo
        {
            public int carId = 0;

            public float PLAYER_MOVE_SPEED = 20; //기본 이동 속도
            public float PLAYER_MAX_MOVE_SPEED = 100; //최대 이동 속도
            public float PLAYER_ROTATION_SPEED = 17; //기본 회전 속도
            public int PLAYER_BATTERY_ATTACK_SUCCESS = 30; //공격 성공시 충전되는 바태리 양
            public int PLAYER_BATTERY_DEFENCE_SUCCESS = 20; //수비 성공시 충전되는 바태리 양
            public int PLAYER_BATTERY_START_AMOUNT = 25; //게임 시작시 시작 바태리 양

            public int PLAYER_BATTERY_MAX = 120; // battery 총량
            public float PLAYER_BATTERY_AUTOCHARGE_SPEED = 0.9f; //자동 충전 속도
            public int PLAYER_BATTERY_CARBOOSTER1_COST = 20; //Lv 1 부스터 비용
            public int PLAYER_BATTERY_CARBOOSTER2_COST = 33; //Lv 2 부스터 비용
            public int PLAYER_BATTERY_CARBOOSTER3_COST = 50; //Lv 3 부스터 비용

            public float PLAYER_RIGIDBODY_MASS = 10; //질량 (Gravity체크가 되어 있어야함)
            public float PLAYER_RIGIDBODY_DRAG = 1; //Drag  (Gravity체크가 되어 있어야함)
            public float PLAYER_RIGIDBODY_ANGULAR_DRAG = 0; //Angular Drag (Gravity체크가 되어 있어야함)

            public float PLAYER_BOOSTER_CAR_LV1_MAX_SPEED = 10f; //Lv1 부스터 속도
            public float PLAYER_BOOSTER_CAR_LV1_DECREASE_SPEED = 5f; //얼마나 빠르게 소모가 될지
            public float PLAYER_BOOSTER_CAR_LV2_MAX_SPEED = 14f; //Lv2 부스터 속도
            public float PLAYER_BOOSTER_CAR_LV2_DECREASE_SPEED = 5f; //얼마나 빠르게 소모가 될지
            public float PLAYER_BOOSTER_CAR_LV3_MAX_SPEED = 17f; //Lv3 부스터 속도
            public float PLAYER_BOOSTER_CAR_LV3_DECREASE_SPEED = 5f; //얼마나 빠르게 소모가 될지

            public float PLAYER_BOOSTER_CAR_LV1_MAXSPEED_DURATION_TIME = 0.5f; //최고 속도 유지시간
            public float PLAYER_BOOSTER_CAR_LV2_MAXSPEED_DURATION_TIME = 1f; //최고 속도 유지시간
            public float PLAYER_BOOSTER_CAR_LV3_MAXSPEED_DURATION_TIME = 1.5f; //최고 속도 유지시간

            public float PLAYER_DECELERATION_DECREASE_SPEED = 7f; //어느정도의 속도로 감속할지
            public float PLAYER_DECELERATION_MAX_SPEED = 15f; //최대 몇속도를 감속 시킬지
            public float PLAYER_DECELERATION_RECOVERY_SPEED = 14f; //감속 이후 정상 속도로 얼마나 빨리 복귀 시킬지

            public float PLAYER_PARRYING_TIME = 2.5f; //얼마나 parrying을 지속할지
            public float PLAYER_PARRYING_COOLTIME = 5f; //parrying 쿨타임
            public float PLAYER_STUN_TIME = 1.5f; //스턴 상태 지속 시간

            public float PLAYER_FLIP_UP_TIME = 1.5f; //Flip 시 높이 상승 시간
            public float PLAYER_FLIP_UP_SPEED = 18f; //Flip 시 높이 상승 속도
            public float PLAYER_FLIP_DOWN_SPEED = 19f; //Flip 시 상승 후 하강 속도
            public float PLAYER_FLIP_GROUND_DELAY_TIME = 0.4f; //지면 도착 후 딜레이


            public string LOBBY_STATS_CARNAME = "None";
            public int LOBBY_STATS_01 = 0;
            public int LOBBY_STATS_02 = 0;
            public int LOBBY_STATS_03 = 0;
            public int LOBBY_STATS_04 = 0;
        }
    }

    [SerializeField] public MAP_DATA mapData = new MAP_DATA();
    [System.Serializable]
    public class MAP_DATA
    {
        [SerializeField] public List<MapInfo> MapDataList = new List<MapInfo>();

        [System.Serializable]
        public class MapInfo
        {
            public int id;
            public string SCENE_NAME = "";

            public int MAX_PLAYER = 0;
            public int MIN_PLAYER = 0;
            public int LAP_COUNT = 0;

            public int FOOD_TRUCK_NUMBER_TO_SPAWN = 0;
        }
    }

    [System.Serializable]
    public class CHARACTER_DATA
    {
        public enum CharacterType { One, Two, Three, Four }

        [System.Serializable]
        public class CharacterInfo
        {
            public CharacterType type;
            public int id;
        }
    }




    [SerializeField] public MATCH_RECORD_DATA matchRecordData = new MATCH_RECORD_DATA();
    [System.Serializable]
    public class MATCH_RECORD_DATA
    {
        [SerializeField] public List<MatchResultInfo> MatchResultInfoList = new List<MatchResultInfo>();



        [System.Serializable]
        public class MatchResultInfo
        {
            [System.Serializable]
            public class PlayerResultInfo
            {
                public int rank;
                public double totalGameTime;
                public string playerId;
                public int playerCarId;
                public int playerCharacterId;

                //TODO....
                public int attackCount;
                public int defenceCount;
            }

            [SerializeField] public List<PlayerResultInfo> PlayerResultInfoList = new List<PlayerResultInfo>();

            public int MapId;
            public int TotalPlayers;
        }
    }
}
