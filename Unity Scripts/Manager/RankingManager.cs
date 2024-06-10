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
using System.Linq;


public partial class RankingManager : MonoSingleton<RankingManager>
{
    public const string MOBILE_REF_DATA_FILE_PATH = "ranking_data.text";
    public const string PC_REF_DATA_FILE_PATH = "/6.Data/ranking_data.json";

    public const string FIREBASE_RP_RANK = "TotalRP_RankingUserList";
    public const string FIREBASE_CHILD_RANKINGPOINT = "RankingPoint";
    public const string FIREBASE_CHILD_RANKINGPOINTNEEDTOBECALCULATED = "RankPointNeedToBeCalculated";
    public const string FIREBASE_CHILD_PLAYMODE = "ingameSettings_ingamePlayMode";

    public int MAX_RANK_LIST_COUNT = 10;

    [ReadOnly] public RankData rankingData = new RankData();

    [ReadOnly] public bool IsRankingDataLoaded;

    public RankData RankingData
    {
        get
        {
            if (rankingData != null)
                return rankingData;
            else
            {
                UtilityCommon.ColorLog("ERROR...! rankingData is null!", UtilityCommon.DebugColor.Red);
                return null;
            }
        }
    }

    #region Firebase Data Related
    public void InitializeFirebaseCallback()
    {
        //최초로 딱 1번만 호출시키자...
        FirebaseDatabase.DefaultInstance
        .GetReference(FirebaseManager.FIREBASE_DB_RANKING)
        .ValueChanged += OnValueChanged_RankingData;
    }

    public void LoadData_FirebaseServer(Action<bool> callback = null) 
    {
        if (FirebaseManager.Instance.IsFirebaseSetup == false)
        {
            Debug.Log("<color=red>Error!!! Firebase Loading is not ready...</color>");
            return;
        }

        FirebaseDatabase.DefaultInstance
        .GetReference(FirebaseManager.FIREBASE_DB_RANKING)
        .Child(FIREBASE_RP_RANK)
        .OrderByChild(FIREBASE_CHILD_RANKINGPOINT)
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
                    Debug.Log("<color=yellow>[FIREBASE] Ranking Data Loaded!</color>");
#endif
                    string json = snapshot.GetRawJsonValue();

                    rankingData.TotalRP_RankingUserList.Clear();
                    var ordedChildren = snapshot.Children.OrderByDescending(child => child.Child(FIREBASE_CHILD_RANKINGPOINT).Value);
                    foreach (var child in ordedChildren)
                    { 
                        var data = JsonUtility.FromJson<RankData.RankingUserInfo>(child.GetRawJsonValue());
                        rankingData.TotalRP_RankingUserList.Add(data);
                    }
                    rankingData.TotalRP_RankingUserList.Sort((x, y) => y.RankingPoint.CompareTo(x.RankingPoint));

                    callback?.Invoke(true);
                }
                else
                {
                    //데이터가 아예 없는 경우...

                    callback?.Invoke(false);
                }

                IsRankingDataLoaded = true;
            }
        });
    }

    // 새 점수 추가 또는 업데이트
    private void SaveData_FirebaseServer_Add(RankData.RankingUserInfo info, bool isPromoted, bool isDemoted, Action<bool, bool, bool> callback = null)
    {
        if (info.RankingPoint <= 0)
        {
#if UNITY_EDITOR || SERVERTYPE_DEV
            Debug.Log("<color=red>RankingPoint 0 will not be saved...</color>");
#endif
            return;
        }

        var list = RankingManager.Instance.RankingData.TotalRP_RankingUserList;
        if (list != null && list.Count >= MAX_RANK_LIST_COUNT) 
        {
            var minRP = int.MaxValue;
            foreach (var i in list)
            {
                if (i.RankingPoint < minRP)
                    minRP = i.RankingPoint;
            }

            //현재 가지고 있는 랭킹 정보에서 순위 안에 안들면 갱신x
            if (info.RankingPoint < minRP)
            {
                return;
            }
        }

        string json = JsonUtility.ToJson(info);

        FirebaseDatabase.DefaultInstance
       .GetReference(FirebaseManager.FIREBASE_DB_RANKING)
       .Child(FIREBASE_RP_RANK)
       .Child(info.PID)
       .SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
       {
           if (task.IsFaulted)
           {
               callback?.Invoke(false, isPromoted, isDemoted);
           }
           else if (task.IsCompleted)
           {
               callback?.Invoke(true, isPromoted, isDemoted);
           }
       });

#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=yellow>[FIREBASE] Ranking Data Saved!</color>");
#endif
    }

    // 상위 x명 유지
    private void SaveData_FirebaseServer_TrimRank(Action<bool> callback = null)
    {
        FirebaseDatabase.DefaultInstance
        .GetReference(FirebaseManager.FIREBASE_DB_RANKING)
        .Child(FIREBASE_RP_RANK)
        .OrderByChild(FIREBASE_CHILD_RANKINGPOINT)
        .GetValueAsync().ContinueWithOnMainThread(task =>
        {
           if (task.IsFaulted)
           {
               callback?.Invoke(false);
           }
           else if (task.IsCompleted)
           {
                DataSnapshot snapshot = task.Result;
                if (snapshot.ChildrenCount > MAX_RANK_LIST_COUNT)
                {
                    var children = snapshot.Children.OrderByDescending(child => child.Child(FIREBASE_CHILD_RANKINGPOINT).Value);
                    var childrenToKeep = children.Take(MAX_RANK_LIST_COUNT).Select(child => child.Key).ToList();
                    foreach (var child in snapshot.Children)
                    {
                        if (!childrenToKeep.Contains(child.Key))
                        {
                            FirebaseDatabase.DefaultInstance.GetReference(FirebaseManager.FIREBASE_DB_RANKING)
                            .Child(FIREBASE_RP_RANK).Child(child.Key).RemoveValueAsync();
                        }
                    }
                }

#if UNITY_EDITOR || SERVERTYPE_DEV
                Debug.Log("<color=yellow>[FIREBASE] Rank List Successfully Trimmed</color>");
#endif
                callback?.Invoke(true);
           }
       });
    }

    private void OnValueChanged_RankingData(object sender, ValueChangedEventArgs args)
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
            Debug.Log("<color=yellow>[FIREBASE] OnValueChanged ===> RankingData</color>");
#endif

            rankingData = JsonUtility.FromJson<RankData>(json);
        }
    }
    #endregion
}
