using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_IngameControl_VirtualPad : MonoBehaviour
{
    [SerializeField] GameObject boosterBtn = null;
    [SerializeField] GameObject leftBtn = null;
    [SerializeField] GameObject rightBtn = null;
    [SerializeField] GameObject decerlerateBtn = null;

    private PhotonNetworkManager photonNetworkManager => PhotonNetworkManager.Instance;
    private NetworkInGameRPCManager myNetworkInGameRPCManager => photonNetworkManager.MyNetworkInGameRPCManager;

    private void Awake()
    {
        boosterBtn.SafeSetPressd(OnPressCallback);
        leftBtn.SafeSetPressd(OnPressCallback);
        rightBtn.SafeSetPressd(OnPressCallback);
        decerlerateBtn.SafeSetPressd(OnPressCallback);
    }

    private void OnPressCallback(GameObject go, bool onPress)
    {
        if (CommonDefine.CurrentControlType() == CommonDefine.ControlType.VirtualPad)
        {
            var player = InGameManager.Instance.myPlayer;
            if (player == null)
                return;

            if (go == boosterBtn)
            {
                if (onPress)
                {
                    var myPlayer = InGameManager.Instance.myPlayer;
                    if (myPlayer != null)
                    {
                        PlayerMovement.CarBoosterLevel boosterLv = myPlayer.GetAvailableInputBooster();
                        if (boosterLv == PlayerMovement.CarBoosterLevel.None)
                            return;

                        myNetworkInGameRPCManager.RPC_BoostPlayer(player.PlayerID, (int)boosterLv);
                    }
                }
            }
            else if (go == leftBtn)
            {
                if (onPress)
                {
                    myNetworkInGameRPCManager.RPC_ChangeLane_Left(player.PlayerID);
                }
            }
            else if (go == rightBtn)
            {
                if (onPress)
                {
                    myNetworkInGameRPCManager.RPC_ChangeLane_Right(player.PlayerID);
                }
            }
            else if (go == decerlerateBtn)
            {
                if (onPress)
                {
                    if (player.isDecelerating == false)
                        myNetworkInGameRPCManager.RPC_Deceleration_Start(player.PlayerID);
                }
                else
                {
                    if (player.isDecelerating == true)
                        myNetworkInGameRPCManager.RPC_Deceleration_End(player.PlayerID);
                }
            }
        }
            
    }


}
