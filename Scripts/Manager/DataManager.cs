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







    //���� �ٲ�� �̺�Ʈ �������� �ڵ� ȣ���
    void HandleBasicValue_GameDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot == null)
            return;

        //�̰� ������ data initializing �ҋ� �̻��ϰ� ����...! �� ���� ��Ű��
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

        //�̰� ������ data initializing �ҋ� �̻��ϰ� ����...! �� ���� ��Ű��
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

        //�̰� ������ data initializing �ҋ� �̻��ϰ� ����...! �� ���� ��Ű��
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

        //�̰� ������ data initializing �ҋ� �̻��ϰ� ����...! �� ���� ��Ű��
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

        //�̰� ������ data initializing �ҋ� �̻��ϰ� ����...! �� ���� ��Ű��
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

        //�̰� ������ data initializing �ҋ� �̻��ϰ� ����...! �� ���� ��Ű��
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

        //�̰� ������ data initializing �ҋ� �̻��ϰ� ����...! �� ���� ��Ű��
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

        //�̰� ������ data initializing �ҋ� �̻��ϰ� ����...! �� ���� ��Ű��
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

        //�̰� ������ data initializing �ҋ� �̻��ϰ� ����...! �� ���� ��Ű��
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

        //�̰� ������ data initializing �ҋ� �̻��ϰ� ����...! �� ���� ��Ű��
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
            public float CAMERA_HEIGHT = 12f; //ī�޶� ���� ����
            public float CAMERA_DIST = 7f; //ī�޶� �Ÿ� ����
            public float CAMERA_POSITION_SLERP_SPEED = 3.5f; //ī�޶� Position ���� �ٴ� �ӵ�(?)
            public float CAMERA_ROTATION_SLERP_SPEED = 17f; //ī�޶� Rotation ���� �ٴ� �ӵ�(?)
            public float CAMERA_POSITION_SLERP_SPEED_CHANGINGLINE = 3.5f;
            public float CAMERA_ROTATION_SLERP_SPEED_CHANGINGLINE = 12f;
            public float CAMERA_TARGET_Y_OFFSET = 4f; //ī�޶��� Ÿ�� offset
            public int CAMERA_FIELD_OF_VIEW = 65; //ī�޶� ���ٰ�(?) field of view
        }

        [Serializable]
        public class PlayerData
        {
            public float PLAYER_REDUCE_STRENGTH_AFTER_FINISHLINE = 10f; //Finish Line ��� �� �ӵ� ���̴� �ӵ�
            public float PLAYER_MOVE_INDEX_CHECK_MIN_RANGE = 15f; //next index ã���� ã�� ����
            public int PLAYER_MOVE_INDEX_CHECK_NUMBER = 7; //next index ����� ã����
            public float PLAYER_MOVE_SPEED_BUFF = 0.05f; //���ʿ� �÷��̾ ���ǵ� ���� = ����X��

            public float PLAYER_LAG_MAX_DISTANCE = 2f; //Lag Max
            public float PLAYER_LAG_MIN_DISTANCE = 0.25f; //Lag�� �Ǵ��ϴ� dist
            public float PLAYER_LAG_MAX_RATE = 0.6f; //Lag�� ������� ��ġ�� �������� ... �̶� MAX ��
            public float PLAYER_LAG_MIN_RATE = 0.06f; //Lag�� ������� ��ġ�� �������� ... �̶� MIN ��
            public float PLAYER_LAG_TELEPORT_TIME = 1.5f; //���� �ð� �̻� lag �� ��� teleport ���ѹ�����
            public float PLAYER_LAG_TELEPORT_DIST = 15f; //���� �Ÿ� �̻� lag �� ��� teleport ���ѹ�����

            public float PLAYER_LAG_PING_ADJUSTMENT_RATE = 1f;

            public float PLAYER_DIRECTION_LERP_MAX_SPEED = 15f;
            public float PLAYER_DIRECTION_LERP_MIN_SPEED = 10f;
            public float PLAYER_DIRECTION_LERP_ADDITIONAL_ANGLE_MIN_SPEED = 5f;
            public float PLAYER_DIRECTION_LERP_ADDITIONAL_ANGLE_MAX_SPEED = 29f;

            public bool PLAYER_RIGIDBODY_USE_GRAVITY = true;

            public bool PLAYER_OUT_OF_BOUNDARY_USE = false; //isOutOfBoundary ��뿩��
            public float PLAYER_OUT_OF_BOUNDARY_TIME = 3f; //isOutOfBoundary ���� �� ���� ���� �˵��� ������
            public float PLAYER_COLLISION_DELAY_COOLTIME = 0.1f; //�浹 �� collision �� Ÿ��.... (�浹�� ����� ����)

            public float PLAYER_INDICATOR_CHECK_BEHIND_DIST = 30f; //������� �Ÿ� üũ�ϴ��� (�ε������� ��)

            public float PLAYER_VALID_REACH_DEST = 1.5f;

            public int PLAYER_SHOW_MOVE_SPEED_MULTIPYER = 1;
        }

        [Serializable]
        public class GameData
        {
            public int GAME_END_COUNTDOWN_TIME = 10; //1�� ��� �� ���� ���� ī��Ʈ �ٿ�
            public int GAME_START_COUNTDOWN_TIME = 5; //���� ���� ī��Ʈ �ٿ�
            public float GAME_TIME_WAITING_BEFORE_START = 10; //Start Race �� ��� �ð�... ��� �ð� ���� �÷��̰� ������ solo mode
            public float GAME_INPUT_SWIPE_MIN_LENGHT = 0.05f; //Ingame input �¿� �ν��� ����ϱ� ���� �������� �ּ� �Ÿ�
            public float GAME_INPUT_DELAY = 0.2f; //Ingame input ���� input���� ������ (���� input �����ִ�..)
            public bool GAME_INPUT_USE_NETWORK_RESPONSE = false; //��Ʈ��ũ ����ް� ��ǲ ����� ������..
            public bool GAME_FOODTRUCK_ACTIVATE = true; //Ǫ��Ʈ�� ���� ����
            public bool GAME_MINIMAP_ACTIVATE = true; //�̴ϸ� ��� ����
            public bool GAME_SPEEDBUFF_ACTIVATE = true; //���� ���� ���ǵ� ���� ��뿩��
            public int GAME_MAP_ID = 0; //-1�� ��� ����
            public float GAME_AUTO_LEAVE_ROOM_TIME = 30f; //���� ���� �� x�� �� Room ������ ������ �ð�
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

            public int CHARGEPAD_LV1_CHARGE_AMOUNT = 20; //Lv1 �е� ���� ��
            public int CHARGEPAD_LV2_CHARGE_AMOUNT = 30; //Lv2 �е� ���� ��
            public int CHARGEPAD_LV3_CHARGE_AMOUNT = 45; //Lv3 �е� ���� ��

            public float CHARGEPAD_LV1_MAX_SPEED = 7f; //Lv1 �е� �ν��� �ӵ�
            public float CHARGEPAD_LV1_DECREASE_SPEED = 4f; //�󸶳� ������ �Ҹ� ����
            public float CHARGEPAD_LV2_MAX_SPEED = 8f; //Lv2 �е� �ν��� �ӵ�
            public float CHARGEPAD_LV2_DECREASE_SPEED = 4f; //�󸶳� ������ �Ҹ� ����
            public float CHARGEPAD_LV3_MAX_SPEED = 9f; //Lv3 �е� �ν��� �ӵ�
            public float CHARGEPAD_LV3_DECREASE_SPEED = 4f; //�󸶳� ������ �Ҹ� ����

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

            public float PLAYER_MOVE_SPEED = 20; //�⺻ �̵� �ӵ�
            public float PLAYER_MAX_MOVE_SPEED = 100; //�ִ� �̵� �ӵ�
            public float PLAYER_ROTATION_SPEED = 17; //�⺻ ȸ�� �ӵ�
            public int PLAYER_BATTERY_ATTACK_SUCCESS = 30; //���� ������ �����Ǵ� ���¸� ��
            public int PLAYER_BATTERY_DEFENCE_SUCCESS = 20; //���� ������ �����Ǵ� ���¸� ��
            public int PLAYER_BATTERY_START_AMOUNT = 25; //���� ���۽� ���� ���¸� ��

            public int PLAYER_BATTERY_MAX = 120; // battery �ѷ�
            public float PLAYER_BATTERY_AUTOCHARGE_SPEED = 0.9f; //�ڵ� ���� �ӵ�
            public int PLAYER_BATTERY_CARBOOSTER1_COST = 20; //Lv 1 �ν��� ���
            public int PLAYER_BATTERY_CARBOOSTER2_COST = 33; //Lv 2 �ν��� ���
            public int PLAYER_BATTERY_CARBOOSTER3_COST = 50; //Lv 3 �ν��� ���

            public float PLAYER_RIGIDBODY_MASS = 10; //���� (Gravityüũ�� �Ǿ� �־����)
            public float PLAYER_RIGIDBODY_DRAG = 1; //Drag  (Gravityüũ�� �Ǿ� �־����)
            public float PLAYER_RIGIDBODY_ANGULAR_DRAG = 0; //Angular Drag (Gravityüũ�� �Ǿ� �־����)

            public float PLAYER_BOOSTER_CAR_LV1_MAX_SPEED = 10f; //Lv1 �ν��� �ӵ�
            public float PLAYER_BOOSTER_CAR_LV1_DECREASE_SPEED = 5f; //�󸶳� ������ �Ҹ� ����
            public float PLAYER_BOOSTER_CAR_LV2_MAX_SPEED = 14f; //Lv2 �ν��� �ӵ�
            public float PLAYER_BOOSTER_CAR_LV2_DECREASE_SPEED = 5f; //�󸶳� ������ �Ҹ� ����
            public float PLAYER_BOOSTER_CAR_LV3_MAX_SPEED = 17f; //Lv3 �ν��� �ӵ�
            public float PLAYER_BOOSTER_CAR_LV3_DECREASE_SPEED = 5f; //�󸶳� ������ �Ҹ� ����

            public float PLAYER_BOOSTER_CAR_LV1_MAXSPEED_DURATION_TIME = 0.5f; //�ְ� �ӵ� �����ð�
            public float PLAYER_BOOSTER_CAR_LV2_MAXSPEED_DURATION_TIME = 1f; //�ְ� �ӵ� �����ð�
            public float PLAYER_BOOSTER_CAR_LV3_MAXSPEED_DURATION_TIME = 1.5f; //�ְ� �ӵ� �����ð�

            public float PLAYER_DECELERATION_DECREASE_SPEED = 7f; //��������� �ӵ��� ��������
            public float PLAYER_DECELERATION_MAX_SPEED = 15f; //�ִ� ��ӵ��� ���� ��ų��
            public float PLAYER_DECELERATION_RECOVERY_SPEED = 14f; //���� ���� ���� �ӵ��� �󸶳� ���� ���� ��ų��

            public float PLAYER_PARRYING_TIME = 2.5f; //�󸶳� parrying�� ��������
            public float PLAYER_PARRYING_COOLTIME = 5f; //parrying ��Ÿ��
            public float PLAYER_STUN_TIME = 1.5f; //���� ���� ���� �ð�

            public float PLAYER_FLIP_UP_TIME = 1.5f; //Flip �� ���� ��� �ð�
            public float PLAYER_FLIP_UP_SPEED = 18f; //Flip �� ���� ��� �ӵ�
            public float PLAYER_FLIP_DOWN_SPEED = 19f; //Flip �� ��� �� �ϰ� �ӵ�
            public float PLAYER_FLIP_GROUND_DELAY_TIME = 0.4f; //���� ���� �� ������


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
