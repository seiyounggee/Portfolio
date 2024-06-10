using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using WebSocketSharp;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public partial class ReferenceManager : MonoSingleton<ReferenceManager>
{
    [ReadOnly] private bool mobilePlatformChecker;
    private string filePath;
    public const string MOBILE_REF_DATA_FILE_PATH = "ref_data.text";
    public const string PC_REF_DATA_FILE_PATH = "/6.Data/ref_data.json";

    RefData referenceData = new RefData();

    [ReadOnly] public bool IsReferenceDataLoaded;

    public RefData ReferenceData
    {
        get
        {
            if (referenceData != null)
                return referenceData;
            else
            {
                UtilityCommon.ColorLog("ERROR...! referenceData is null!", UtilityCommon.DebugColor.Red);
                return null;
            }
        }
    }

    private void Start()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            mobilePlatformChecker = true;
            filePath = Application.persistentDataPath + MOBILE_REF_DATA_FILE_PATH; //휴대폰
        }
        else
        {
            mobilePlatformChecker = false;
            filePath = Application.dataPath + PC_REF_DATA_FILE_PATH; //PC
        }
    }

    public void Save_ReferenceData()
    {
        SaveData_FirebaseServer();
    }

    #region Firebase Data Related
    public void InitializeFirebaseCallback()
    {
        //최초로 딱 1번만 호출시키자...
        FirebaseDatabase.DefaultInstance
        .GetReference(FirebaseManager.FIREBASE_DB_REFERENCE)
        .ValueChanged += OnValueChanged_RefData;
    }

    public void LoadData_FirebaseServer()
    {
        if (FirebaseManager.Instance.IsFirebaseSetup == false)
        {
            Debug.Log("<color=red>Error!!! Firebase Loading is not ready...</color>");
            return;
        }

        FirebaseDatabase.DefaultInstance
        .GetReference(FirebaseManager.FIREBASE_DB_REFERENCE)
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
#if UNITY_EDITOR || SERVERTYPE_DEV
                    Debug.Log("<color=yellow>[FIREBASE] Ref Data Loaded!</color>");
#endif

                    string json = snapshot.GetRawJsonValue();
                    referenceData = JsonUtility.FromJson<RefData>(json);                  
                }
                else
                {
                    //for intialization...!! Currently Firebase Save is not valid~
                    //need to check Firebase Database Rules! write / read
                    //Debug.Log("<color=yellow>[FIREBASE] No Exsisting ref data in firebase... assigning new data to firebase</color>");
                    //SaveData_FirebaseServer();
                }

                IsReferenceDataLoaded = true;
            }
        });
    }

    private void SaveData_FirebaseServer()
    {
        string json = JsonUtility.ToJson(referenceData);
        FirebaseDatabase.DefaultInstance
       .GetReference(FirebaseManager.FIREBASE_DB_REFERENCE)
       .SetRawJsonValueAsync(json);

#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=yellow>[FIREBASE] Ref Data Saved!</color>");
#endif
    }

    private void OnValueChanged_RefData(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        // Do something with the data in args.Snapshot
        string json = args.Snapshot.GetRawJsonValue();
        if (string.IsNullOrEmpty(json) == false)
        {
#if UNITY_EDITOR || SERVERTYPE_DEV
            Debug.Log("<color=yellow>[FIREBASE] OnValueChanged ===> RefData</color>");
#endif

            referenceData = JsonUtility.FromJson<RefData>(json);
        }
    }
    #endregion

    //사용 x.....
    #region Local Device Data Related
    public void InitializeData_Local()
    {
        if (string.IsNullOrEmpty(filePath) == false)
        {
            if (File.Exists(filePath))
            {
                LoadData_Local();
            }
        }
    }

    public void DeleteData_Local()
    {
#if UNITY_EDITOR
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            filePath = Application.persistentDataPath + MOBILE_REF_DATA_FILE_PATH; //휴대폰
        }
        else
        {
            filePath = Application.dataPath + PC_REF_DATA_FILE_PATH; //PC
        }

        if (string.IsNullOrEmpty(filePath) == false)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log("file deleted!: " + filePath);
            }
        }
#endif
    }

    public void LoadData_Local()
    {
        if (referenceData == null)
        {
            UtilityCommon.ColorLog("ERROR...! referenceData is null!", UtilityCommon.DebugColor.Red);
            return;
        }

        string dataString = "";
        if (mobilePlatformChecker)
            dataString = File.ReadAllText(Application.persistentDataPath + MOBILE_REF_DATA_FILE_PATH);
        else
            dataString = File.ReadAllText(Application.dataPath + PC_REF_DATA_FILE_PATH);

        if (string.IsNullOrEmpty(dataString) == false)
            referenceData = JsonUtility.FromJson<RefData>(dataString);

        Debug.Log("<color=cyan>[LOCAL] Ref Data </color> <color=white>Loaded!</color>");

    }

    private void SaveData_Local()
    {
        if (referenceData == null)
        {
            UtilityCommon.ColorLog("ERROR...! referenceData is null!", UtilityCommon.DebugColor.Red);
            return;
        }

        string dataString = JsonUtility.ToJson(referenceData);

        if (mobilePlatformChecker)
            File.WriteAllText(Application.persistentDataPath + MOBILE_REF_DATA_FILE_PATH, dataString);
        else
            File.WriteAllText(Application.dataPath + PC_REF_DATA_FILE_PATH, dataString);

        Debug.Log("<color=cyan>[LOCAL] Ref Data </color> <color=white>Saved!</color>");
    }
    #endregion
}
