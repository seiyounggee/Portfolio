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
                        PlayerMovement.CarBoosterType boosterLv = myPlayer.GetAvailableInputBooster();
                        if (boosterLv != PlayerMovement.CarBoosterType.None)
                            myNetworkInGameRPCManager.RPC_BoostPlayer(player.networkPlayerID, (int)boosterLv);
                        else
                        {
                            if (myPlayer.IsTimingBoosterReady)
                                myPlayer.ResetTimingBooster();
                        }
                    }
                }
            }
            else if (go == leftBtn)
            {
                if (onPress)
                {
                    myNetworkInGameRPCManager.RPC_ChangeLane_Left(player.networkPlayerID);
                }
            }
            else if (go == rightBtn)
            {
                if (onPress)
                {
                    myNetworkInGameRPCManager.RPC_ChangeLane_Right(player.networkPlayerID);
                }
            }
            else if (go == decerlerateBtn)
            {
                if (onPress)
                {
                    if (player.isShield == false)
                        myNetworkInGameRPCManager.RPC_Shield(player.networkPlayerID);
                }
                else
                {
                    /*
                    if (player.isDecelerating == true)
                        myNetworkInGameRPCManager.RPC_Deceleration_End(player.networkPlayerID);
                    */
                }
            }
        }
            
    }


}
