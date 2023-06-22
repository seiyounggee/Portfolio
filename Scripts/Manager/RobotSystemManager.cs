using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DTR.Protocol;
using DTR.Shared;
using System;
using PNIX.ReferenceTable;

public class RobotSystemManager : MonoSingleton<RobotSystemManager>, IEventBus<RobotSystemManager.EventBusKey>
{
    [ReadOnly] public DateTime? rewardTime = null;

    [ReadOnly] public short network_currentEnergyCount = 0;
    [ReadOnly] public short network_currentMaxEnergyCount = 0;
    [ReadOnly] public short network_currentRewaredCount = 0;

    public List<CSearchingRobotRewardInfo> SearchingRobotRewardInfos = null;

    #region eventBus
    public enum EventBusKey
    {
        None,
        OnRefreshSearchingRobot, //현재 에너지, 리워드 수 Event
        OnSearchingRobotRewards, //보상 받을 수 있는 리스트 Event
        OnReceiveSearchingRobotRewards, //보상 Event
    }

    static Dictionary<EventBusKey, List<Action>> events => IEventBus<EventBusKey>.events;

    public void SubscribeEvent(EventBusKey key, Action ac)
    {
        if (events.ContainsKey(key) == false)
        {
            var l = new List<Action>();
            l.Add(ac);
            events.Add(key, l);
        }
        else
        {
            events[key].Add(ac);
        }
    }

    public void UnSubscribeEvent(EventBusKey key, System.Action ac)
    {
        if (events.ContainsKey(key) == true)
        {
            var l = events[key];
            if (l != null)
            {
                foreach (var i in l)
                {
                    if (i.Equals(ac))
                    {
                        l.Remove(i);
                        break;
                    }
                }
            }
        }
    }

    public void UnSubscribeAllEvent(EventBusKey key)
    {
        if (events.ContainsKey(key) == true)
            events.Remove(key);
    }

    public void ExcecuteEvent(EventBusKey key)
    {
        if (events.ContainsKey(key))
        {
            foreach (var i in events[key])
                i?.Invoke();
        }
    }
    #endregion

    public void SendRefreshSearchingRobot()
    {
        PnixNetworkManager.Instance.SendRefreshSearchingRobot();
    }

    public void SendReceiveSearchingRobotRewards()
    {
        PnixNetworkManager.Instance.SendReceiveSearchingRobotRewards();
    }

    public void OnReceiveSearchingRobotRewards(GS2CProtocolData.CReceiveSearchingRobotRewardsAck ack)
    {
        if (ack.ErrorCode == EErrorCode.SUCCESS)
        {
            //Success의 경우 무조건 list를 날리자
            SearchingRobotRewardInfos.Clear();

            ExcecuteEvent(EventBusKey.OnReceiveSearchingRobotRewards);
        }

        //SendRefreshSearchingRobot();
    }

    public void SetSearchingRobot(GS2CProtocolData.CNotifySearchingRobot ack)
    {
        /*
        Debug.Log("EnergyCnt: " + ack.EnergyCnt);
        Debug.Log("MaxEnergyCnt: " + ack.MaxEnergyCnt);
        Debug.Log("RewardedCnt: " + ack.RewardedCnt);
        */

        network_currentEnergyCount = ack.EnergyCnt;
        network_currentMaxEnergyCount = ack.MaxEnergyCnt;
        network_currentRewaredCount = ack.RewardedCnt;

        if (ack.RewardTime.HasValue)
        {
            rewardTime = ack.RewardTime.Value;

            /*
            Debug.Log("ServerTime: " + PnixNetworkManager.Instance.ServerTime);
            Debug.Log("RewardTime: " + ack.RewardTime);
            var ts = RobotSystemManager.Instance.rewardTime.Value - PnixNetworkManager.Instance.ServerTime;
            Debug.Log("ts: " + ts.TotalSeconds);
            */
        }
        else
        {
            //Debug.Log("RewardTime: null");
        }

        ExcecuteEvent(EventBusKey.OnRefreshSearchingRobot);
    }

    public void SetSearchingRobotRewards(GS2CProtocolData.CNotifySearchingRobotRewards ack)
    {

        if (SearchingRobotRewardInfos == null || SearchingRobotRewardInfos.Count == 0)
        {
            SearchingRobotRewardInfos = ack.SearchingRobotRewardInfos;
        }
        else
        {
            foreach (var i in ack.SearchingRobotRewardInfos)
            {
                if (SearchingRobotRewardInfos.Exists(x => x.ItemID.Equals(i.ItemID)))
                {
                    SearchingRobotRewardInfos.Find(x => x.ItemID.Equals(i.ItemID)).ItemID = i.ItemID;
                }
                else
                {
                    SearchingRobotRewardInfos.Add(i);
                }
            }
        }

        ExcecuteEvent(EventBusKey.OnSearchingRobotRewards);
    }

    public void ActivateSearchRobotTimer()
    {
        SendRefreshSearchingRobot();
        UtilityCoroutine.StartCoroutine(ref searchRobotTimer, SearchRobotTimer(), this);
    }

    public void DeactivateSearchRobotTimer()
    {
        rewardTime = null;
        UtilityCoroutine.StopCoroutine(ref searchRobotTimer, this);
    }

    private IEnumerator searchRobotTimer = null;

    private IEnumerator SearchRobotTimer()
    {
        while (true)
        {
            if (rewardTime.HasValue)
            {
                var ts = PnixNetworkManager.Instance.ServerTime - rewardTime.Value;
                if (ts.TotalSeconds >= 0)
                {
                    SendRefreshSearchingRobot();
                    rewardTime = null;
                }
            }

            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Initialize
                || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGame
                || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGameReady)
            {
                DeactivateSearchRobotTimer();
                break;
            }

            yield return null;
        }
    }

    public bool IsRewardValid()
    {
        if (rewardTime.HasValue == false)
            return false;
        else
        {
            if (rewardTime.Value > PnixNetworkManager.Instance.ServerTime)
                return true;
            else
                return false;
        }
    }

    public bool IsNoEnergy()
    {
        if (network_currentEnergyCount <= 0)
            return true;
        else
            return false;
    }

    public bool IsRewardMax()
    {
        if (network_currentRewaredCount >= EnergyPossessionLimitAmount())
            return true;
        else
            return false;
    }

    public int EnergyConsumeSec() //수집로봇 에너지가 소모되는 주기(초)
    {
        return GetSearchingRobotConfig<int>("defaultEnergyConsumeSec");
    }

    public int EnergyPossessionLimitAmount() //에너지 최대 보유 갯수
    {
        return GetSearchingRobotConfig<int>("defaultEnergyPossessionLimitAmount");
    }

    public int EnergyRewardLimitAmount() //에너지 최대 사용 갯수
    {
        return GetSearchingRobotConfig<int>("defaultEnergyRewardLimitAmount");
    }

    public T GetSearchingRobotConfig<T>(string _key)
    {
        var data = CReferenceManager.Instance.FindRefSearchingRobotConfig(_key);
        if (data != null)
        {
            return ToType<T>(data.value);
        }
        else
        {
            Debug.LogError("GetSearchingRobotConfig() _key Error Key=" + _key);
            return default(T);
        }
    }

    public static T ToType<T>(string value)
    {
        object parsedValue = default(T);
        Type type = typeof(T);

        try
        {
            parsedValue = Convert.ChangeType(value, type, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
        }

        return (T)parsedValue;
    }


    public CRefSearchingRobotReward GetSearchingRobotReward(int level)
    {
        var rewardRed = CReferenceManager.Instance.FindRefSearchingRobotReward(level);

        return rewardRed;
    }
}
