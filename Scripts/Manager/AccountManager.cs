using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using DTR.Shared;
using DTR.Protocol;


public class AccountManager : MonoSingleton<AccountManager>
{
    private Int64 pid = 0;
    public Int64 PID { get { return pid; } }

    private string nickname = string.Empty;
    public string Nickname { get { return nickname; } }

    private string accessKey = string.Empty;
    public string AccessKey { get { return accessKey; } }

    private readonly string pid_playerprefs = "pid_playerprefs";
    private readonly string accessKey_playerprefs = "accessKey_playerprefs";

    public bool IsRecommendUpdate { get; set; }

    private long coin = 0;
    public long Coin { get { return coin; } }

    private long freeGem = 0;
    public long FreeGEM { get { return freeGem; } }

    private long paidGem = 0;
    public long PaidGem { get { return paidGem; } }

    List<CCarInfo> MyCarInfoList = new List<CCarInfo>();
    List<Int32> MyCharacterInfoList = new List<Int32>();
    public int Equipped_CarID;

    public CCarInfo Equipped_CarInfo { get { if (MyCarInfoList != null) return MyCarInfoList.Find(x => x.CarID.Equals(Equipped_CarID)); else return null; } }
    public int Equipped_CharacterID;

    private int racingPoint = 0;
    public int RacingPoint { get { return racingPoint; } }

    private int streak = 0;
    public int Streak { get { return streak; } }

    private int firstPlaceCnt = 0;
    public int FirstPlaceCnt { get { return firstPlaceCnt; } }
    private int secondPlaceCnt = 0;
    public int SecondPlaceCnt { get { return secondPlaceCnt; } }
    private int thirdPlaceCnt = 0;
    public int ThirdPlaceCnt { get { return thirdPlaceCnt; } }
    private int fourthPlaceCnt = 0;
    public int FourthPlaceCnt { get { return fourthPlaceCnt; } }
    private int retireCnt = 0;
    public int RetireCnt { get { return retireCnt; } }

    public void Initialize()
    {
        if (PlayerPrefs.HasKey(pid_playerprefs))
        {
            Int64.TryParse(PlayerPrefs.GetString(pid_playerprefs), out pid);
        }

        if (PlayerPrefs.HasKey(accessKey_playerprefs))
        {
            accessKey = PlayerPrefs.GetString(accessKey_playerprefs);
        }
    }

    //계정 정보 셋팅
    public void SetAccountInfo(DTR.Protocol.GS2CProtocolData.CLoginAck ack)
    {
        if (ack == null)
            return;

        Debug.Log("<color=cyan>Account PID: " + ack.PID + " AccessKey:  " + ack.AccessKey + "</color>");

        pid = ack.PID;
        accessKey = ack.AccessKey;

        PlayerPrefs.SetString(pid_playerprefs, pid.ToString());
        PlayerPrefs.SetString(accessKey_playerprefs, accessKey);
    }

    public void SetProfile(GS2CProtocolData.CNotifyProfile ack)
    {
        nickname = ack.NickName;
    }


    //보유한 돈 세팅
    public void SetMoney(DTR.Protocol.GS2CProtocolData.CNotifyMoney ack)
    {
        if (ack == null)
            return;

        coin = ack.Coin;
        freeGem = ack.FreeGem;
        paidGem = ack.PaidGem;
    }

    //보유한 차 세팅
    public void SetCarList(GS2CProtocolData.CNotifyCars ack)
    {
        if (ack == null)
            return;

        MyCarInfoList = ack.CarInfos;
        Equipped_CarID = ack.AttachedCarID;
    }

    public void SetCharacterList(GS2CProtocolData.CNotifyCharacters ack)
    {
        if (ack == null)
            return;

        MyCharacterInfoList = ack.CharacterIDs;
        Equipped_CharacterID = ack.AttachedCharacterID;
    }

    public void SetRacingRecord(GS2CProtocolData.CNotifyRacingRecord ack)
    {
        if (ack == null)
            return;

        racingPoint = ack.RacingPoint;
        streak = ack.Streak;
        firstPlaceCnt = ack.FirstPlaceCnt;
        secondPlaceCnt = ack.SecondPlaceCnt;
        thirdPlaceCnt = ack.ThirdPlaceCnt;
        fourthPlaceCnt = ack.FourthPlaceCnt;
        retireCnt = ack.RetireCnt;
    }


    public int TEMP_AI_TYPE = 1;
}
