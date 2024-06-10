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
using System.Globalization;

public partial class AccountManager : MonoSingleton<AccountManager>
{
    [ReadOnly] private bool mobilePlatformChecker;
    private string filePath;

    public const string MOBILE_ACCOUNT_DATA_FILE_PATH = "account_data.text";
    public const string PC_ACCOUNT_DATA_FILE_PATH = "/Data/account_data.json";

    AccountData accountData = new AccountData();

    [ReadOnly] public bool IsAccountDataLoaded;

    public Action OnValueChangedAccountData = null;

    private void Start()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            mobilePlatformChecker = true;
            filePath = Application.persistentDataPath + MOBILE_ACCOUNT_DATA_FILE_PATH; //ÈÞ´ëÆù
        }
        else
        {
            mobilePlatformChecker = false;
            filePath = Application.dataPath + PC_ACCOUNT_DATA_FILE_PATH; //PC
        }
    }

    private void SetupInitialData()
    {
        accountData = new AccountData();
#if EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE || UNITY_STANDALONE
        if (FirebaseManager.Instance.firebase_userId != string.Empty)
        {
            accountData.PID = FirebaseManager.Instance.firebase_userId;
        }
        else
        {
            accountData.PID = SystemInfo.deviceUniqueIdentifier;
        }
#endif

        accountData.nickname = GetInitialNickname();

        accountData.firebase_email = (FirebaseManager.Instance.firebase_email != string.Empty) ?FirebaseManager.Instance.firebase_email : string.Empty;
        accountData.firebase_passward = (FirebaseManager.Instance.firebase_passward != string.Empty) ?FirebaseManager.Instance.firebase_passward : string.Empty;

        accountData.isBgmOn = true;
        accountData.isSoundEffectOn = true;

        accountData.characterSkinID = 0;
        accountData.ownedCharacterSkin = new List<int>() { 0 };

        accountData.weaponSkinID = 0;
        accountData.ownedWeaponSkin = new List<int>() { 0 };

        accountData.passiveSkillID = 1;
        accountData.ownedPassiveSkill = new List<int>() { 0, 1 };

        accountData.activeSkillID = 1;
        accountData.ownedActiveSkill = new List<int>() { 0, 1 };

        accountData.ingameSettings_ingamePlayMode = (int)Quantum.InGamePlayMode.SoloMode;
        accountData.ingameSettings_cameramode = (int)CameraManager.InGameCameraMode.SimpleLookAt;
        accountData.ingameSettings_cameraOffsetDistance = 11f;

        accountData.RankPointNeedToBeCalculated = false;
        accountData.RankingPoint = 0;

        accountData.PlayerStats_MaxHealthPoint_Level = 0;
        accountData.PlayerStats_AttackDamage_Level = 0;
        accountData.PlayerStats_AttackRange_Level = 0;
        accountData.PlayerStats_AttackDuration_Level = 0;
        accountData.PlayerStats_MaxSpeed_Level = 0;
        accountData.PlayerStats_AttackCooltime_Level = 0;
        accountData.PlayerStats_JumpCooltime_Level = 0;

        CultureInfo cultureInfo = CultureInfo.CurrentCulture;
        accountData.CountryCode = cultureInfo.Name; //언어코드 + 국가코드
        accountData.LanguageCode = cultureInfo.TwoLetterISOLanguageName; //언어코드


#if UNITY_EDITOR || SERVERTYPE_DEV
        UtilityCommon.ColorLog("Data Initialized!", UtilityCommon.DebugColor.Cyan);
#endif
    }

    #region General methods

    private void Save_AccountData(Action<bool> callback = null)
    {
        //TODO Client -> CloudFunction 으로 바꾸자...
        SaveMyData_FirebaseServer_All(callback);
    }

    #endregion

    #region Firebase Data Related

    private string GetInitialNickname()
    {
        string nickname = "User";

        if (accountData != null)
        {
            int desiredLength = 10;
            if (accountData.PID.Length > desiredLength)
            {
                nickname += accountData.PID.Substring(0, desiredLength);
            }
            else
            {
                nickname += accountData.PID;
            }
        }

        return nickname;
    }

    public void InitializeFirebaseCallback()
    {
        //최초로 딱 1번만 호출시키자...
        FirebaseDatabase.DefaultInstance
        .GetReference(FirebaseManager.FIREBASE_DB_ACCOUNT)
        .Child(FirebaseManager.Instance.firebase_userId)
        .ValueChanged += OnValueChanged_AccountData;
    }

    public void LoadMyData_FirebaseServer(Action<bool> callback = null)
    {
        var userPid = FirebaseManager.Instance.firebase_userId;
        if (string.IsNullOrEmpty(userPid))
        {
            //혹시나 해서... 보통 firebase에서 보내주는 UserId 사용
            userPid = SystemInfo.deviceUniqueIdentifier;
        }

        if (FirebaseManager.Instance.IsFirebaseSetup == false)
        {
            Debug.Log("<color=red>Error!!! Firebase Loading is not ready...</color>");
            return;
        }

        FirebaseDatabase.DefaultInstance
        .GetReference(FirebaseManager.FIREBASE_DB_ACCOUNT)
        .Child(userPid)
        .GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                //Handle Error

                callback?.Invoke(false);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
#if UNITY_EDITOR || SERVERTYPE_DEV
                    Debug.Log("<color=yellow>[FIREBASE] Account Data Loaded!</color>");
#endif

                    string json = snapshot.GetRawJsonValue();
                    accountData = JsonUtility.FromJson<AccountData>(json);

                    callback?.Invoke(true);

                    //CheckLoadedData();
                    //SaveData_Local();
                }
                else
                {
#if UNITY_EDITOR || SERVERTYPE_DEV
                    Debug.Log("<color=yellow>[FIREBASE] No Exsisting account in firebase... assigning new data to firebase</color>");
#endif

                    //No Data...!
                    SetupInitialData();
                    SaveMyData_FirebaseServer_All();

                    callback?.Invoke(false);
                }

                IsAccountDataLoaded = true;
            }
        });
    }

    //다른 사람 AccountData 가져오기...!
    public void LoadOtherData_FirebaseServer(string userPid, Action<bool, AccountData> callback = null)
    {
        if (string.IsNullOrEmpty(userPid))
        {
            Debug.Log("<color=red>Error!!! userPid is empty? ...</color>");
            return;
        }

        if (FirebaseManager.Instance.IsFirebaseSetup == false)
        {
            Debug.Log("<color=red>Error!!! Firebase Loading is not ready...</color>");
            return;
        }

        FirebaseDatabase.DefaultInstance
        .GetReference(FirebaseManager.FIREBASE_DB_ACCOUNT)
        .Child(userPid)
        .GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                //Error
                Debug.Log("<color=red>[FIREBASE] Error...</color>");
                callback?.Invoke(false, null);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
#if UNITY_EDITOR || SERVERTYPE_DEV
                    Debug.Log("<color=yellow>[FIREBASE] Account Data Loaded!</color>");
#endif

                    string json = snapshot.GetRawJsonValue();
                    var accountData_other = JsonUtility.FromJson<AccountData>(json);

                    callback?.Invoke(true, accountData_other);
                }
                else
                {
                    callback?.Invoke(false, null);
                }
            }
        });
    }

    public void SaveMyData_FirebaseServer_All(Action<bool> callback = null)
    {
        string json = JsonUtility.ToJson(accountData);
        FirebaseDatabase.DefaultInstance
       .GetReference(FirebaseManager.FIREBASE_DB_ACCOUNT)
       .Child(accountData.PID)
       .SetRawJsonValueAsync(json)
       .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("<color=red>[FIREBASE] Error...</color>");
                callback?.Invoke(false);
            }
            else if (task.IsCompleted)
            {
                callback?.Invoke(true);
            }
        });

#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=yellow>[FIREBASE] Account Data Saved!</color>");
#endif
    }

    public void SaveMyData_FirebaseServer_IngamePlay(Action<bool> callback = null)
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=white>SaveMyData_FirebaseServer_IngamePlay</color>");
#endif
        //Ingame Play 시점에 저장할 정보들은 저장하자

        //RankPointNeedToBeCalculated -> true로 바꿔주자
        //정산해야되는 걸 알리는중...
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { $"/{accountData.PID}/{RankingManager.FIREBASE_CHILD_PLAYMODE}", accountData.ingameSettings_ingamePlayMode },
            { $"/{accountData.PID}/{RankingManager.FIREBASE_CHILD_RANKINGPOINTNEEDTOBECALCULATED}", true }
        };

        FirebaseDatabase.DefaultInstance
       .GetReference(FirebaseManager.FIREBASE_DB_ACCOUNT)
       .UpdateChildrenAsync(updates)
       .ContinueWithOnMainThread(task =>
       {
           if (task.IsFaulted)
           {
               Debug.Log("<color=red>[FIREBASE] Error...</color>");
               callback?.Invoke(false);
           }
           else if (task.IsCompleted)
           {
               callback?.Invoke(true);
               Debug.Log("<color=yellow>[FIREBASE] RankPointNeedToBeCalculated : true" + "</color>");
           }
       });
    }

    public void SaveMyData_FirebaseServer_Ranking(int rankingPoint, Action<bool> callback = null)
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=white>SaveMyData_FirebaseServer_Ranking</color>");
#endif

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { $"/{accountData.PID}/{RankingManager.FIREBASE_CHILD_RANKINGPOINT}", rankingPoint },
            { $"/{accountData.PID}/{RankingManager.FIREBASE_CHILD_RANKINGPOINTNEEDTOBECALCULATED}", false }
        };

        FirebaseDatabase.DefaultInstance
       .GetReference(FirebaseManager.FIREBASE_DB_ACCOUNT)
       .UpdateChildrenAsync(updates)
       .ContinueWithOnMainThread(task =>
       {
           if (task.IsFaulted)
           {
               Debug.Log("<color=red>[FIREBASE] Error...</color>");
               callback?.Invoke(false);
           }
           else if (task.IsCompleted)
           {
               callback?.Invoke(true);

#if UNITY_EDITOR || SERVERTYPE_DEV
               Debug.Log("<color=yellow>[FIREBASE] Ranking Point Saved PID : " + accountData.PID + "  RP : " + accountData.RankingPoint + "</color>");
#endif
           }
       });
    }


    private void OnValueChanged_AccountData(object sender, ValueChangedEventArgs args)
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
            Debug.Log("<color=yellow>[FIREBASE] OnValueChanged ===> AccountData </color>");
#endif

            accountData = JsonUtility.FromJson<AccountData>(json);

            OnValueChangedAccountData?.Invoke();
        }
    }
    #endregion

    #region Local Device Data Related
    public void InitializeData_Local()
    {
        if (string.IsNullOrEmpty(filePath) == false)
        {
            if (File.Exists(filePath))
            {
                LoadData_Local();
            }
            else
            {
                SetupInitialData();
                SaveData_Local();
            }
        }
    }

    public void DeleteData_Local()
    {
#if UNITY_EDITOR
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            filePath = Application.persistentDataPath + MOBILE_ACCOUNT_DATA_FILE_PATH; //Mobile
        }
        else
        {
            filePath = Application.dataPath + PC_ACCOUNT_DATA_FILE_PATH; //PC
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

    private void LoadData_Local()
    {
        if (accountData == null)
        {
            UtilityCommon.ColorLog("ERROR...! accountData is null!", UtilityCommon.DebugColor.Red);
            return;
        }

        string dataString = "";
        if (mobilePlatformChecker)
            dataString = File.ReadAllText(Application.persistentDataPath + MOBILE_ACCOUNT_DATA_FILE_PATH);
        else
            dataString = File.ReadAllText(Application.dataPath + PC_ACCOUNT_DATA_FILE_PATH);

        if (string.IsNullOrEmpty(dataString) == false)
            accountData = JsonUtility.FromJson<AccountData>(dataString);

        Debug.Log("<color=cyan>[LOCAL] Account Data </color> <color=white>Loaded!</color>");

    }

    private void SaveData_Local()
    {
        if (accountData == null)
        {
            UtilityCommon.ColorLog("ERROR...! accountData is null!", UtilityCommon.DebugColor.Red);
            return;
        }

        string dataString = JsonUtility.ToJson(accountData);

        if (mobilePlatformChecker)
            File.WriteAllText(Application.persistentDataPath + MOBILE_ACCOUNT_DATA_FILE_PATH, dataString);
        else
            File.WriteAllText(Application.dataPath + PC_ACCOUNT_DATA_FILE_PATH, dataString);

        Debug.Log("<color=cyan>[LOCAL] Account Data </color> <color=white>Saved!</color>");
    }
    #endregion
}
