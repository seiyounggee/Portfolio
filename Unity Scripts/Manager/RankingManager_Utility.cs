using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class RankingManager
{
    private int dataSetCounter = 0;

    //로그인 시 누락된 랭킹 포인트 적용
    public void SetAndSaveMyRankingData_Login()
    {
        //로그인 했을 때 정상적으로 정산되지 않은 상태인 경우 강제로 정산시켜버리자
        //정산 시킬 필요가 없으면 넘어가자
        if (AccountManager.Instance.AccountData.RankPointNeedToBeCalculated)
        {
#if UNITY_EDITOR
            Debug.Log("<color=red>RankPointNeedToBeCalculated!!</color>");
#endif

            //꼴등 rp
            (var rp, var isPromoted, var isDemoted) = GetRankingPoint_Login(AccountManager.Instance.PID, AccountManager.Instance.RankingPoint, (Quantum.InGamePlayMode)AccountManager.Instance.CurrentPlayMode);

            var info = new RankData.RankingUserInfo();
            info.PID = AccountManager.Instance.PID;
            info.Nickname = AccountManager.Instance.Nickname;
            info.CountryCode = AccountManager.Instance.CountryCode;
            info.PassiveSkillID = AccountManager.Instance.AccountData.passiveSkillID;
            info.ActiveSkillID = AccountManager.Instance.AccountData.activeSkillID;
            info.RankingPoint = rp;
            info.TotalMatchCount = AccountManager.Instance.AccountData.MatchCount_SoloMode + AccountManager.Instance.AccountData.MatchCount_TeamMode; //TODO..
            info.MatchDateTime = "unknown";

            //Account Data에도 저장
            AccountManager.Instance.SaveMyData_FirebaseServer_Ranking(rp);

            //랭킹 DB에 추가 저장
            SaveData_FirebaseServer_Add(info, isPromoted, isDemoted, OnSaveData_FirebaseServer_Add);
        }

    }

    //랭킹 포인트 적용
    public void SetAndSaveMyRankingData_Ingame()
    {
        var gm = InGame_Quantum.Instance;
        if (gm == null || gm.listOfPlayers == null)
            return;

        //PracticeMode는 랭킹 집계 x
        if (NetworkManager_Client.Instance != null &&
            NetworkManager_Client.Instance.CurrentPlayMode == Quantum.InGamePlayMode.PracticeMode)
            return;

        //혹시 몰라서... 다른 PlayMode 생기면 Ranking 데이터 건들일까봐 넣어둠...
        if (NetworkManager_Client.Instance.CurrentPlayMode != Quantum.InGamePlayMode.SoloMode
            && NetworkManager_Client.Instance.CurrentPlayMode != Quantum.InGamePlayMode.TeamMode)
            return;

#if UNITY_EDITOR
        Debug.Log("<color=white>SetAndSaveRankingData_Me</color>");
#endif

        dataSetCounter = 1; //나 한명 세팅 하면됨

        //내 AccountData 로드 후  세팅해주자
        AccountManager.Instance.LoadOtherData_FirebaseServer(AccountManager.Instance.PID, OnLoadOtherData_FirebaseServer);

    }

    private void OnLoadOtherData_FirebaseServer(bool isComplete, AccountData data)
    {
        if (data == null)
            Debug.Log("<color=red>Error!!! AccountData is null</color>");

        if (isComplete && data != null)
        {
            (var rp, var isPromoted, var isDemoted) = GetRankingPoint_Ingame(data.PID, data.RankingPoint);

            var info = new RankData.RankingUserInfo();
            info.PID = data.PID;
            info.Nickname = data.nickname;
            info.CountryCode = data.CountryCode;
            info.PassiveSkillID = data.passiveSkillID;
            info.ActiveSkillID = data.activeSkillID;
            info.RankingPoint = rp;
            info.TotalMatchCount = data.MatchCount_SoloMode + data.MatchCount_TeamMode; //TODO..
            info.MatchDateTime = NetworkManager_Client.Instance.ServerTime_UTC_String;

            //Account Data에도 저장
            AccountManager.Instance.SaveMyData_FirebaseServer_Ranking(rp);

            //랭킹 DB에 추가 저장
            SaveData_FirebaseServer_Add(info, isPromoted, isDemoted, OnSaveData_FirebaseServer_Add);

        }
    }

    private void OnSaveData_FirebaseServer_Add(bool isComplete, bool isPromoted, bool isDemoted)
    {
        --dataSetCounter;

        if (isComplete && dataSetCounter == 0) //딱 1번만 호출되도록...
        {
            //랭킹 DB에 Add 후 상위 x명 랭킹만 유지하도록 수정
            SaveData_FirebaseServer_TrimRank();
        }

        if (isComplete)
        {
            //강등, 승급 팝업 예약
            if (isPromoted || isDemoted)
            {
                if (isPromoted)
                    PrefabManager.Instance.UI_RankChangePopup.Setup(true);
                else if (isDemoted)
                    PrefabManager.Instance.UI_RankChangePopup.Setup(false);
            }
        }
    }


    private (int, bool, bool) GetRankingPoint_Ingame(string pid, int currentRP)
    {
        var gm = InGame_Quantum.Instance;
        if (gm == null || gm.listOfPlayers == null)
            return (0, false, false);

        bool isPromoted = false;
        bool isDemoted = false;
        int finalRP = 0;
        var currentRank = -1;
        Quantum.InGamePlayMode currMode = Quantum.InGamePlayMode.SoloMode;
        foreach (var i in gm.listOfPlayers)
        {
            if (i.pid.Equals(pid))
            {
                currMode = i.inGamePlayMode;

                if (i.inGamePlayMode == Quantum.InGamePlayMode.SoloMode)
                    currentRank = i.rank_solo;
                else if (i.inGamePlayMode == Quantum.InGamePlayMode.TeamMode)
                    currentRank = i.rank_team;

                break;
            }
        }

        if (currentRank != -1 && ReferenceManager.Instance.RankingData != null) 
        {
            var rankingDataList = ReferenceManager.Instance.RankingData.RankingTierList;
            var data = rankingDataList.Find(x => x.TierRange_Min <= currentRP && x.TierRange_Max >= currentRP);
            if (data != null)
            {
                var currentTierRP_min = data.TierRange_Min;
                var currentTierRP_max = data.TierRange_Max;

                if (currMode == Quantum.InGamePlayMode.SoloMode)
                {
                    switch (currentRank)
                    {
                        case 1:
                            finalRP = currentRP + data.RankingPoints_SoloMode_1st;
                            break;
                        case 2:
                            finalRP = currentRP + data.RankingPoints_SoloMode_2nd;
                            break;
                        case 3:
                            finalRP = currentRP + data.RankingPoints_SoloMode_3rd;
                            break;
                        case 4:
                            finalRP = currentRP + data.RankingPoints_SoloMode_4th;
                            break;
                        case 5:
                            finalRP = currentRP + data.RankingPoints_SoloMode_5th;
                            break;
                        case 6:
                            finalRP = currentRP + data.RankingPoints_SoloMode_6th;
                            break;
                    }
                }
                else if (currMode == Quantum.InGamePlayMode.TeamMode)
                {
                    switch (currentRank)
                    {
                        case 1:
                            finalRP = currentRP + data.RankingPoints_TeamMode_1st;
                            break;
                        case 2:
                            finalRP = currentRP + data.RankingPoints_TeamMode_2nd;
                            break;
                    }
                }

                isPromoted = (finalRP > currentTierRP_max && data.TierType != (short)CommonDefine.RankTierType.Champion); //승급여부
                isDemoted = (finalRP < currentTierRP_min && (data.TierType == (short)CommonDefine.RankTierType.Champion || data.TierType == (short)CommonDefine.RankTierType.Diamond)); //강등여부

                if (isPromoted)
                {
                    //승급했을경우 다음 티어 RP +100
                    finalRP = currentTierRP_max + 101;
                }
                else if (isDemoted)
                {
                    //챔피온,다이아몬드 티어는 강등 가능!!
                    finalRP = Mathf.Clamp(finalRP, 0, currentTierRP_max);
                }
                else //승급,강등 아닌 나머지의 경우
                {
                    finalRP = Mathf.Clamp(finalRP, currentTierRP_min, int.MaxValue); //해당 티어에서 강등 불가 rp_min에 고정
                }
            }
        }

        return (finalRP, isPromoted, isDemoted);
    }

    private (int, bool, bool) GetRankingPoint_Login(string pid, int currentRP, Quantum.InGamePlayMode playMode)
    {
        bool isPromoted = false;
        bool isDemoted = false;

        int finalRP = 0;
        //중간에 나갔다가 들어온 경우 강제로 꼴등 보상을 주자...
        if (ReferenceManager.Instance.RankingData != null)
        {
            var rankingDataList = ReferenceManager.Instance.RankingData.RankingTierList;
            var data = rankingDataList.Find(x => x.TierRange_Min <= currentRP && x.TierRange_Max >= currentRP);
            if (data != null)
            {
                var currentTierRP_min = data.TierRange_Min;
                var currentTierRP_max = data.TierRange_Max;

                if (playMode == Quantum.InGamePlayMode.SoloMode)
                {
                    //중간에 나갔다가 들어온 경우 강제로 꼴등 보상을 주자...
                    finalRP = currentRP + data.RankingPoints_SoloMode_6th;
                }
                else if (playMode == Quantum.InGamePlayMode.TeamMode)
                {
                    //중간에 나갔다가 들어온 경우 강제로 꼴등 보상을 주자...
                    finalRP = currentRP + data.RankingPoints_TeamMode_2nd;
                }

                isPromoted = (finalRP > currentTierRP_max && data.TierType != (short)CommonDefine.RankTierType.Champion); //승급여부
                isDemoted = (finalRP < currentTierRP_min && (data.TierType == (short)CommonDefine.RankTierType.Champion || data.TierType == (short)CommonDefine.RankTierType.Diamond)); //강등여부

                if (isPromoted)
                {
                    //승급했을경우 다음 티어 RP +100
                    finalRP = currentTierRP_max + 101;
                }
                else if (isDemoted)
                {
                    //챔피온,다이아몬드 티어는 강등 가능!!
                    finalRP = Mathf.Clamp(finalRP, 0, currentTierRP_max);
                }
                else //승급,강등 아닌 나머지의 경우
                {
                    finalRP = Mathf.Clamp(finalRP, currentTierRP_min, int.MaxValue); //해당 티어에서 강등 불가 rp_min에 고정
                }
            }
        }

        return (finalRP, isPromoted, isDemoted);
    }
}
