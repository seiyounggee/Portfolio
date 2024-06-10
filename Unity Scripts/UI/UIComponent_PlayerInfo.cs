using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Quantum;

public class UIComponent_PlayerInfo : MonoBehaviour
{
    public InGame_Quantum.PlayerInfo info = null;

    [SerializeField] RawImage rankImg = null;
    [SerializeField] TextMeshProUGUI nameTxt = null;
    [SerializeField] TextMeshProUGUI teamTxt = null;
    [SerializeField] Slider hpSlider = null;

    public enum UIState { None, Show, Hide }
    public UIState state = UIState.None;

    public void SetData(InGame_Quantum.PlayerInfo _info)
    {
        info = _info;
        if (info != null)
        {
            if (string.IsNullOrEmpty(info.nickname) == false)
                nameTxt.SafeSetText(info.nickname.ToString());
            else
                nameTxt.SafeSetText(info.pid.ToString());
        }

        rankImg.texture = ResourceManager.Instance.GetOutGameRankIconByRP(info.rankingPoint);

        state = UIState.Show;
    }

    private unsafe void LateUpdate()
    {
        var f = NetworkManager_Client.Instance.GetFramePredicted();

        if (f == null)
            return;

        if (info == null)
            return;

        if (state == UIState.Show)
        {
            if (f.Unsafe.TryGetPointer<PlayerRules>(info.entityRef, out PlayerRules* pr)
                && f.Unsafe.TryGetPointerSingleton<GameManager>(out var gm))
            {
                hpSlider.minValue = 0;
                hpSlider.maxValue = pr->maxHealthPoint;
                hpSlider.value = pr->currHealthPoint;

                if (pr->isDead)
                {
                    state = UIState.Hide;
                }

                if (gm->CurrentInGamePlayMode == InGamePlayMode.TeamMode)
                {
                    teamTxt.SafeSetActive(true);
                    teamTxt.SafeLocalizeText(StringManager.GetIngameTeamName(pr->teamID));
                    teamTxt.color = StringManager.GetIngameTeamColor(pr->teamID);
                }
                else
                {
                    teamTxt.SafeSetActive(false);
                }
            }
        }
    }

    public Vector3 PositionData()
    {
        if (info != null && info.enityGameObj != null)
        {
            return info.enityGameObj.transform.position;
        }

        return Vector3.zero;
    }
}
