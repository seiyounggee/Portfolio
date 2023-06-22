using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObjectFunction_SetActiveTrueFalse : MonoBehaviour
{
    public enum TurnOnOffType
    { 
        On,
        Off,
    }

    public TurnOnOffType onOffType;

    public enum EventTimingType
    { 
        None,
        StartCountDown,
        RaceStart,
        PassedLastLap,
        EndCountDown,
        EndRace,
    }

    public EventTimingType eventTime;
    public float delayTime = 0f;

    IEnumerator Start()
    {
        while (true)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGame
                && InGameManager.Instance.gameState == InGameManager.GameState.IsGameReady)
                break;

            yield return null;
        }
            
        Initialize();
    }

    private void Initialize()
    {
        if (onOffType == TurnOnOffType.On)
            gameObject.SafeSetActive(false);
        else
            gameObject.SafeSetActive(true);

        var rpc = PhotonNetworkManager.Instance.MyNetworkInGameRPCManager;

        if (rpc == null || InGameManager.Instance == null)
            return;

        switch (eventTime)
        {
            case EventTimingType.None:
                break;
            case EventTimingType.StartCountDown:
                InGameManager.Instance.SubscribeEvent(InGameManager.GameState.StartCountDown, GameState_Callback);
                break;
            case EventTimingType.RaceStart:
                InGameManager.Instance.SubscribeEvent(InGameManager.GameState.PlayGame, GameState_Callback);
                break;
            case EventTimingType.PassedLastLap:
                rpc.AttachObserver(this, NetworkInGameRPCManager.ObserverKey.RPC_PlayerCompletedLap, RPC_Callback_PassLastLap);
                break;
            case EventTimingType.EndCountDown:
                InGameManager.Instance.SubscribeEvent(InGameManager.GameState.EndCountDown, GameState_Callback);
                break;
            case EventTimingType.EndRace:
                InGameManager.Instance.SubscribeEvent(InGameManager.GameState.EndGame, GameState_Callback);
                break;
        }
    }

    private void OnDestroy()
    {
        var rpc = PhotonNetworkManager.Instance.MyNetworkInGameRPCManager;

        if (rpc == null || InGameManager.Instance == null)
            return;

        switch (eventTime)
        {
            case EventTimingType.None:
                break;
            case EventTimingType.StartCountDown:
                InGameManager.Instance.UnSubscribeEvent(InGameManager.GameState.StartCountDown, GameState_Callback);
                break;
            case EventTimingType.RaceStart:
                InGameManager.Instance.UnSubscribeEvent(InGameManager.GameState.PlayGame, GameState_Callback);
                break;
            case EventTimingType.PassedLastLap:
                rpc.DetachObserver(this);
                break;
            case EventTimingType.EndCountDown:
                InGameManager.Instance.UnSubscribeEvent(InGameManager.GameState.EndCountDown, GameState_Callback);
                break;
            case EventTimingType.EndRace:
                InGameManager.Instance.UnSubscribeEvent(InGameManager.GameState.EndGame, GameState_Callback);
                break;
        }
    }

    private void GameState_Callback()
    {
        if(onOffType == TurnOnOffType.On)
            Invoke("ActivateObj", delayTime);
        else
            Invoke("DeactivateObj", delayTime);
    }

    private void RPC_Callback_PassLastLap(Fusion.NetworkId? id)
    {
        if (InGameManager.Instance.myPlayer != null && id.HasValue)
        {
            if (id.Equals(InGameManager.Instance.myPlayer.networkPlayerID))
            {
                if (InGameManager.Instance.myPlayer.IsCurrentLastLap())
                {
                    if (onOffType == TurnOnOffType.On)
                        Invoke("ActivateObj", delayTime);
                    else
                        Invoke("DeactivateObj", delayTime);
                }
            }
        }
    }

    public void ActivateObj()
    {
        gameObject.SafeSetActive(true);
    }

    public void DeactivateObj()
    {
        gameObject.SafeSetActive(false);
    }
}

