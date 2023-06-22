using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PanelIngame_StandingInfo : MonoBehaviour
{
    [SerializeField] UILabel standingTxt = null;
    [SerializeField] UILabel nicknameTxt = null;
    [SerializeField] UILabel recordTxt = null;

    [HideInInspector] public int rank = -1;

    public InGameManager.PlayerInfo playerInfo = null;

    [HideInInspector] public bool isActive = false;

    public bool IsShow
    {
        get 
        {
            if (InGameManager.Instance.isGameEnded == true)
                return true;

            if (InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
                return true;

            if (playerInfo != null && playerInfo.isEnterFinishLine_Network)
                return true;

            return false;
        }
    }

    public void ClearInfo()
    {
        standingTxt.SafeSetText("");
        nicknameTxt.SafeSetText("");
        recordTxt.SafeSetText("");
        rank = -1;
        playerInfo = null;
        isActive = false;
    }

    public void Activate()
    {
        isActive = true;
        if (gameObject.activeSelf == false)
            gameObject.SafeSetActive(true);
    }

    public void SetInfo(InGameManager.PlayerInfo info) //최초 세팅
    {
        if (info != null)
        {
            playerInfo = info;

            nicknameTxt.SafeSetText(info.data.ownerNickName.ToString());
            if (info.isEnterFinishLine_Network)
            {
                standingTxt.SafeSetText(rank.ToString());
                recordTxt.SafeSetText(UtilityCommon.GetTimeString_TYPE_2(info.enterFinishLineTime_Network));
            }
            else
            {
                standingTxt.SafeSetText("X");
                recordTxt.SafeSetText("RETIRE");
            }
        }
    }

    public void UpdateInfo(InGameManager.PlayerInfo info) //갱신
    {
        if (info != null)
        {
            playerInfo = info;

            if (info.isEnterFinishLine_Network)
            {
                standingTxt.SafeSetText(rank.ToString());
                recordTxt.SafeSetText(UtilityCommon.GetTimeString_TYPE_2(info.enterFinishLineTime_Network));
            }
            else
            {
                standingTxt.SafeSetText("X");
                recordTxt.SafeSetText("RETIRE");
            }
        }
    }

    public void SetRank(int rank)
    {
        this.rank = rank;

        if (playerInfo != null && playerInfo.isEnterFinishLine_Network)
        {
            standingTxt.SafeSetText(rank.ToString());
        }
        else
        {
            standingTxt.SafeSetText("X");
            recordTxt.SafeSetText("RETIRE");
        }
    }
}
