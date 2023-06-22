using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PanelIngame_Nickname : MonoBehaviour
{
    [SerializeField] UILabel nicknameTxt = null;

    [HideInInspector] public InGameManager.PlayerInfo playerInfo = null;

    public void SetData(InGameManager.PlayerInfo info)
    {
        playerInfo = info;

        if (info != null && info.data != null)
        {
            gameObject.name = "Nickname - " + info.data.ownerNickName;
            nicknameTxt.SafeSetText(info.data.ownerNickName);
        }
        else
            nicknameTxt.SafeSetText("???");
    }

    public float dissst = 3.2f;
    private void FixedUpdate()
    {
        if (playerInfo != null && playerInfo.pm != null && playerInfo.go != null && InGameManager.Instance.myPlayer != null)
        {
            var pos = GetNicknameLocalPosition(playerInfo.go.transform.position + Vector3.up * dissst);
            transform.localPosition = pos;
            transform.localScale = Vector3.one;
        }
        else
        {
            gameObject.SafeSetActive(false);
        }
    }

    private Vector3 GetNicknameLocalPosition(Vector3 targetPos)
    {
        var ingameCam = Camera_Base.Instance.mainCam;
        var uiCam = UIRoot_Base.Instance.uiCam;

        if (ingameCam == null || uiCam == null)
            return Vector3.zero;


        var pos = NGUIMath.WorldToLocalPoint(targetPos, ingameCam, uiCam, transform);
        pos.z = 0f;


        return pos;
    }
}
