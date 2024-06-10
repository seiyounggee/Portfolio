using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Crashlytics;

public class FirebaseManager : MonoSingleton<FirebaseManager>
{
    private FirebaseApp firebaseApp = null;
    public FirebaseApp FirebaseApp { get { return firebaseApp; } }
    private FirebaseAuth firebaseAuth = null;
    public bool IsFirebaseSetup { get; set; } = false;
    public bool IsFirebaseLogin { get; set; } = false;

    public const string FIREBASE_DB_REFERENCE = "Reference_Data";
    public const string FIREBASE_DB_ACCOUNT = "Account_Data";
    public const string FIREBASE_DB_RANKING = "Ranking_Data";

    public string firebase_email = "";
    public string firebase_passward = "";
    public string firebase_userId = "";

    private string PLAYERPREFS_FIREBASE_EMAIL = "PLAYERPREFS_FIREBASE_EMAIL";
    private string PLAYERPREFS_FIREBASE_PASSWARD = "PLAYERPREFS_FIREBASE_PASSWARD";
    private string PLAYERPREFS_USER_ID = "PLAYERPREFS_USER_ID";

    //계정 데이터 Windows의 경우 C:\Users\<사용자 이름>\AppData\LocalLow\<CompanyName>\<ProductName>\에 위치

    public void Init()
    {
        IsFirebaseLogin = false;
        IsFirebaseSetup = false;

        firebase_email = string.Empty;
        firebase_passward = string.Empty;
        firebase_userId = string.Empty;
    }

    public void SetupFirebase()
    {
#if UNITY_EDITOR
        string targetPath = "Assets/google-services.json";
        string sourcePath = "";

        // 여기서 조건을 정의하여 필요한 google-services.json을 선택
    #if SERVERTYPE_DEV || UNITY_EDITOR
        sourcePath = "Assets/Resources/FirebaseConfigs/google-services_dev.json";
    #elif SERVERTYPE_RELEASE
        sourcePath = "Assets/Resources/FirebaseConfigs/google-services_live.json";
    #endif

        // 파일 복사
        System.IO.File.Copy(sourcePath, targetPath, true);
        UnityEditor.AssetDatabase.Refresh();
#endif

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.

                firebaseAuth = FirebaseAuth.DefaultInstance;

                firebaseApp = FirebaseApp.Create(new AppOptions()
                {
#if SERVERTYPE_RELEASE
                    ProjectId = "live-blader",
                    DatabaseUrl = new System.Uri("https://live-blader-default-rtdb.firebaseio.com"),
#elif SERVERTYPE_DEV || UNITY_EDITOR
                    ProjectId = "project-sd-8ed54",
                    DatabaseUrl = new System.Uri("https://project-sd-8ed54-default-rtdb.firebaseio.com"),
#endif
                });

                // Set the recommended Crashlytics uncaught exception behavior.
                Crashlytics.ReportUncaughtExceptionsAsFatal = true;

                IsFirebaseSetup = true;

                Debug.Log("<color=yellow>[FIREBASE] Setup Success!</color>");
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    public void LoginToFirebaseAuthentication()
    {
        if (firebaseAuth == null)
        {
            Debug.LogError("<color=red>firebaseAuth == null</color>");
            return;
        }

        if (PlayerPrefs.HasKey(PLAYERPREFS_FIREBASE_EMAIL) && PlayerPrefs.HasKey(PLAYERPREFS_FIREBASE_PASSWARD))
        {
            firebase_email = PlayerPrefs.GetString(PLAYERPREFS_FIREBASE_EMAIL);
            firebase_passward = PlayerPrefs.GetString(PLAYERPREFS_FIREBASE_PASSWARD);
        }

        if (string.IsNullOrEmpty(firebase_email) == false && string.IsNullOrEmpty(firebase_passward) == false)
        {
            //이메일 로그인
            firebaseAuth.SignInWithEmailAndPasswordAsync(firebase_email, firebase_passward).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("<color=red>SignInWithEmailAndPasswordAsync was canceled.</color>");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("<color=red>SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception + "</color>");
                    return;
                }

                Firebase.Auth.AuthResult result = task.Result;

                PlayerPrefs.SetString(PLAYERPREFS_FIREBASE_EMAIL, firebase_email);
                PlayerPrefs.SetString(PLAYERPREFS_FIREBASE_PASSWARD, firebase_passward);
                PlayerPrefs.SetString(PLAYERPREFS_USER_ID, result.User.UserId);
                firebase_userId = result.User.UserId;
                IsFirebaseLogin = true;

                Debug.LogFormat("<color=yellow>[FIREBASE][Email] User signed in successfully: {0} ({1}!</color>", result.User.DisplayName, result.User.UserId);
            });
        }
        else
        {
            //익명 로그인
            //저장된 이메일,비번이 없으면 익명으로 sign in 시키고 로그인 하자
            firebaseAuth.SignInAnonymouslyAsync().ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInAnonymouslyAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                    return;
                }

                Firebase.Auth.AuthResult result = task.Result;
                firebase_userId = result.User.UserId;
                IsFirebaseLogin = true;

                Debug.LogFormat("<color=yellow>[FIREBASE][Anonymous] User signed in successfully: {0} ({1}!</color>", result.User.DisplayName, result.User.UserId);
            });
        }
    }

    public void LoadFirebaseServerTime()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase with {task.Exception}");
                return;
            }

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(".info/serverTimeOffset");
            reference.GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error getting server time offset: " + task.Exception);
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    long serverTimeOffset = (long)snapshot.Value;
                    System.DateTime serverTime = System.DateTime.UtcNow.AddMilliseconds(serverTimeOffset);
                    TimeSpan offset = serverTime.Subtract(DateTime.UtcNow);
                    NetworkManager_Client.Instance.SetServerTime(offset);

#if UNITY_EDITOR || SERVERTYPE_DEV
                    Debug.Log("<color=white>Server UTC Time: " + serverTime.ToString() + "</color>");
#endif
                }
            });
        });
    }
}
