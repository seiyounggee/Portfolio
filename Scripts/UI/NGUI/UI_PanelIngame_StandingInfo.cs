using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PanelIngame_StandingInfo : MonoBehaviour
{
    [SerializeField] UILabel standingTxt = null;
    [SerializeField] UILabel nicknameTxt = null;
    [SerializeField] UILabel recordTxt = null;

    [HideInInspector] public int rank = -1;

    public void SetInfo(InGameManager.PlayerInfo info, int index)
    {
        if (info != null)
        {
            rank = index + 1; //�߰��� ������ -1�� ǥ�ð� �Ǵ� ������ �־ index�� ��ŷ ǥ��������...!

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
}
