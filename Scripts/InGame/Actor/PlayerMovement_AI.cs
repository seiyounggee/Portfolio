using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Fusion;
using Fusion.Sockets;
using UnityEngine.Events;

public partial class PlayerMovement
{
    //AI�� ��� AI�� ��Ʈ���ϰ� �ִ� RoomMasterClient�� �߰��� ���� ��� StateAuthority�� �ٲ�� isMine�� �� �� ����
    //�̶� ���ο� RoomMasterClient�� �� ��� �Ʒ� ����(FixedUpdate_AI)�� �̾ �ްԵ�

    private float ai_leftRightCooltimeCounter = 0f;
    private float ai_leftRightCooltime = 3f;
    private float ai_ref_leftRightCooltime_min = 0.5f;
    private float ai_ref_leftRightCooltime_max = 1.5f;


    private float ai_autoDodgeRoadCooltimeCounter = 0f;
    private float ai_autoDodgeRoadCooltime = 2f;
    private float ai_ref_autoDodgeRoadCooltime_min = 2.0f;
    private float ai_ref_autoDodgeRoadCooltime_max = 2.5f;

    private float ai_autoDodgePlayerActivationDist = 7f;
    private float ai_autoDodgePlayerCooltimeCounter = 0f;
    private float ai_autoDodgePlayerCooltime = 2f;
    private float ai_ref_autoDodgePlayerCooltime_min = 2.0f;
    private float ai_ref_autoDodgePlayerCooltime_max = 2.5f;
    private float ai_ref_autoDodgePlayerProbability = 3500f;

    private float ai_autoDodgeObstacleActivationDist = 10f;
    private float ai_autoDodgeObstacleCooltimeCounter = 0f;
    private float ai_autoDodgeObstacleCooltime = 2f;
    private float ai_ref_autoDodgeObstacleCooltime_min = 2.0f;
    private float ai_ref_autoDodgeObstacleCooltime_max = 2.5f;
    private float ai_ref_autoDodgeObstacleProbability = 7000f;

    private float ai_boosterInputCooltimeCounter = 0f;
    private float ai_boosterInputCooltime = 3.5f;
    private float ai_ref_boosterInputCooltime_min = 1.5f;
    private float ai_ref_boosterInputCooltime_max = 3.0f;

    private float ai_decelerationActivationDist = 7f;
    private float ai_decelerationStartCooltimeCounter = 0f;
    private float ai_decelerationEndCooltimeCounter = 0f;
    private float ai_decelerationStartCooltime = 3f;
    private float ai_decelerationEndCooltime = 1.5f;
    private bool ai_successInBlockingOtherPlayer = false;
    private float ai_ref_decelerationStartCooltime_min = 2.5f;
    private float ai_ref_decelerationStartCooltime_max = 3.0f;
    private float ai_ref_decelerationEndCooltime_min = 0.8f;
    private float ai_ref_decelerationEndCooltime_max = 1.5f;
    private float ai_ref_decelerationStartSameLaneProbability = 7500f;
    private float ai_ref_decelerationStartDiffLaneProbability = 3500f;

    private bool ai_timingBoosterAvailable = false;
    private float ai_timingBoosterCounter = 0f;
    private float ai_timingBoosterCooltime = 1f;
    private float ai_ref_timingBoosterInputCooltime_min = 0.8f;
    private float ai_ref_timingBoosterInputCooltime_max = 1.3f;
    private float ai_ref_timingBoosterSuccessProbability = 8000f;

    private bool ai_isCheckingMapObjects = false;
    private IEnumerator ai_checkLoop = null;
    private float ai_nearCheckChargePadDist = 20f;
    private float ai_nearCheckPlayerDist = 30f;

    [SerializeField] private List<MapObject_ChargePad> ai_nearChargePadList = new List<MapObject_ChargePad>();
    [SerializeField] private List<InGameManager.PlayerInfo> ai_nearPlayerList = new List<InGameManager.PlayerInfo>();

    [SerializeField] private List<AI_Skills> ai_skillsUsedList = new List<AI_Skills>();

    public enum AI_Action
    {
        None,
        ChangeLane_Left,
        ChangeLane_Right,
        Booster,
        Shield,

        Max,
    }

    public enum AI_Skills
    { 
        None,
        
        MoveLeftRightRandom, //�¿� �̵�
        DodgeRoad, //������������ ���ϱ�
        DodgePlayer, //���� ���ϱ�
        DodgeObstacles, //��ֹ� ���ϱ�
        MoveTowardsChargePad, //�����е�� �ڵ� �̵�
        UseDeceleration, //����&���� ��뿩��
        EndDecelerationAfterBlocking, //���� �������� �ν��� ��뿩��
        UseBoosterWhenBoosterThreeIsAvailable, //�ν���3�ܰ� ��밡���ҽ� �����ؼ� ��뿩��
        Booster,
        TimingBooster, //Ÿ�̹� �ν���
    }

    public void Initialize_AI()
    {
        ai_nearChargePadList = new List<MapObject_ChargePad>();
        ai_nearPlayerList = new List<InGameManager.PlayerInfo>();

        ai_isCheckingMapObjects = false;
        ai_boosterInputCooltimeCounter = ai_boosterInputCooltime;
        ai_successInBlockingOtherPlayer = false;

        ai_skillsUsedList.Clear();

        ai_skillsUsedList = new List<AI_Skills>
            {
                AI_Skills.MoveLeftRightRandom,
                AI_Skills.DodgeRoad,
                AI_Skills.DodgePlayer,
                AI_Skills.DodgeObstacles,
                AI_Skills.MoveTowardsChargePad,
                AI_Skills.UseDeceleration,
                AI_Skills.EndDecelerationAfterBlocking,
                AI_Skills.UseBoosterWhenBoosterThreeIsAvailable,
                AI_Skills.Booster,
                AI_Skills.TimingBooster,
            };

        isStartingBoosterActiavated = true;
    }

    public void SetAI(int id)
    {
        var refAI = DataManager.Instance.GetAIType(id);

        if (refAI != null)
        {
            ai_skillsUsedList = new List<AI_Skills>();

            if (refAI.moveLeftRightRandomOnOff == 1)
                ai_skillsUsedList.Add(AI_Skills.MoveLeftRightRandom);

            if (refAI.DodgeRoadOnOff == 1)
                ai_skillsUsedList.Add(AI_Skills.DodgeRoad);

            if (refAI.DodgePlayerOnOff == 1)
                ai_skillsUsedList.Add(AI_Skills.DodgePlayer);

            if (refAI.DodgeObstaclesOnOff == 1)
                ai_skillsUsedList.Add(AI_Skills.DodgeObstacles);

            if (refAI.TimingBoosterOnOff == 1)
                ai_skillsUsedList.Add(AI_Skills.TimingBooster);

            if (refAI.DecelerationOnOff == 1)
                ai_skillsUsedList.Add(AI_Skills.UseDeceleration);

            if (refAI.EndDecelerationAfterBlockingOnOff == 1)
                ai_skillsUsedList.Add(AI_Skills.EndDecelerationAfterBlocking);

            if (refAI.UseBoosterWhenBoosterThreeIsAvailableOnOff == 1)
                ai_skillsUsedList.Add(AI_Skills.UseBoosterWhenBoosterThreeIsAvailable);

            if (refAI.MoveTowardsChargePadOnOff == 1)
                ai_skillsUsedList.Add(AI_Skills.MoveTowardsChargePad);

            if (refAI.BoosterOnOff == 1)
                ai_skillsUsedList.Add(AI_Skills.Booster);

            ai_ref_leftRightCooltime_min = refAI.moveLeftRightRandomCooltimeMin;
            ai_ref_leftRightCooltime_max = refAI.moveLeftRightRandomCooltimeMax;

            ai_ref_autoDodgeRoadCooltime_min = refAI.DodgeRoadCooltimeMin;
            ai_ref_autoDodgeRoadCooltime_max = refAI.DodgeRoadCooltimeMax;

            ai_ref_autoDodgePlayerCooltime_min = refAI.DodgePlayerCooltimeMin;
            ai_ref_autoDodgePlayerCooltime_max = refAI.DodgePlayerCooltimeMax;
            ai_ref_autoDodgePlayerProbability = refAI.DodgePlayerActivationProb;

            ai_ref_autoDodgeObstacleCooltime_min = refAI.DodgeObstaclesCooltimeMin;
            ai_ref_autoDodgeObstacleCooltime_max = refAI.DodgeObstaclesCooltimeMax;
            ai_ref_autoDodgeObstacleProbability = refAI.DodgeObstaclesActivationProb;

            ai_ref_boosterInputCooltime_min = refAI.BoosterCooltimeMin;
            ai_ref_boosterInputCooltime_max = refAI.BoosterCooltimeMax;

            ai_ref_decelerationStartCooltime_min = refAI.DecelerationStartCooltimeMin;
            ai_ref_decelerationStartCooltime_max = refAI.DecelerationStartCooltimeMax;
            ai_ref_decelerationEndCooltime_min = refAI.DecelerationEndCooltimeMin;
            ai_ref_decelerationEndCooltime_max = refAI.DecelerationEndCooltimeMax;
            ai_ref_decelerationStartSameLaneProbability = refAI.DecelerationStartProbSameLane;
            ai_ref_decelerationStartDiffLaneProbability = refAI.DecelerationStartProbDiffLane;

            ai_ref_timingBoosterInputCooltime_min = refAI.TimingBoosterCooltimeMin;
            ai_ref_timingBoosterInputCooltime_max = refAI.TimingBoosterCooltimeMax;
            ai_ref_timingBoosterSuccessProbability = refAI.TimingBoosterProb;
        }
        else
        {
            Debug.Log("No such AI id: " + id);
        }
    }

    private void FixedUpdate_AI()
    {
        if (IsWayPointSet() == false)
            return;

        if (IsAI == true)
        {
            if (IsMineAndAI)
            {
                if (network_isMoving && !isFlipped && !isOutOfBoundary)
                {
                    MoveLeftRight_AI();

                    DodgeRoad_AI();

                    DodgePlayer_AI();

                    DodgeObstacles_AI();

                    Booster_AI();

                    TimingBooster_AI();

                    Sheild_AI();

                    if (!ai_isCheckingMapObjects)
                        UtilityCoroutine.StartCoroutine(ref ai_checkLoop, CheckMapObjects(), this);
                }
            }
        }
        else
        {
            //���̽� ���� �� �ڵ� ����
            if (network_isEnteredTheFinishLine && IsAutoMovement)
            {
                if (IsMineAndNotAI && client_currentMoveSpeed > 0f)
                {
                    MoveLeftRight_AI();
                    DodgeRoad_AI();
                    DodgeObstacles_AI();

                    if (!ai_isCheckingMapObjects)
                        UtilityCoroutine.StartCoroutine(ref ai_checkLoop, CheckMapObjects(), this);
                }
            }
        }

    }

    //���� ������ �̵�
    private void MoveLeftRight_AI()
    {
        if (ai_skillsUsedList.Contains(AI_Skills.MoveLeftRightRandom) == false)
            return;

        if (isStunned)
            return;

        if (isShield)
            return;

        //����� ����ϸ� �������� �¿� �̵� �����ִ� �κ� ��Ÿ���� ���� �ø���...
        float minCoolTime = 0f;
        if (network_isEnteredTheFinishLine)
        {
            minCoolTime = 3f;
        }

        if (ai_leftRightCooltimeCounter < float.MaxValue)
            ai_leftRightCooltimeCounter += Runner.DeltaTime;

        if (ai_leftRightCooltimeCounter > ai_leftRightCooltime)
        {
            if (ai_skillsUsedList.Contains(AI_Skills.MoveTowardsChargePad) == true)
            {
                //��ó �����е� ���� ��� �������� �̵�
                if (ai_nearChargePadList == null || ai_nearChargePadList.Count <= 0)
                {
                    MoveRandomRightOrLeft_AI();
                }
                else //�����е�� �̵�
                {
                    MoveToChargePad_AI();
                }
            }
            else
            {
                MoveRandomRightOrLeft_AI();
            }

            ai_leftRightCooltimeCounter = 0f;
            ai_leftRightCooltime = minCoolTime + UnityEngine.Random.Range(ai_ref_leftRightCooltime_min, ai_ref_leftRightCooltime_max);
        }
    }

    //���������� ���� �ڵ����� ���ϱ�...!
    private void DodgeRoad_AI()
    {
        if (isStunned)
            return;

        if (ai_skillsUsedList.Contains(AI_Skills.DodgeRoad) == false)
            return;

        if (ai_autoDodgeRoadCooltimeCounter < float.MaxValue)
            ai_autoDodgeRoadCooltimeCounter += Runner.DeltaTime;

        //checkIndexNumber = �󸶳� ���� index�� üũ����
        int checkIndexNumber = 4;

        var nextIndex = client_currentMoveIndex;
        for (int i = 0; i < checkIndexNumber; i++)
        {
            nextIndex = GetNextMoveIndex(client_currentLaneType, nextIndex);
        }

        if (IsFrontValidLane_AI(client_currentLaneType, nextIndex) == false
            && ai_autoDodgeRoadCooltimeCounter > ai_autoDodgeRoadCooltime)
        {
            //�������� ���� ���������� ������ ����....
            int random = UnityEngine.Random.Range(0, 100);
            if (random < 50)
            {
                if (IsLeftValidLane_AI(client_currentLaneType, nextIndex))
                    ActivateAction(AI_Action.ChangeLane_Left);
                else if (IsRightValidLane_AI(client_currentLaneType, nextIndex))
                    ActivateAction(AI_Action.ChangeLane_Right);
            }
            else
            {
                if (IsRightValidLane_AI(client_currentLaneType, nextIndex))
                    ActivateAction(AI_Action.ChangeLane_Right);
                else if (IsLeftValidLane_AI(client_currentLaneType, nextIndex))
                    ActivateAction(AI_Action.ChangeLane_Left);

            }

            ai_autoDodgeRoadCooltime = UnityEngine.Random.Range(ai_ref_autoDodgeRoadCooltime_min, ai_ref_autoDodgeRoadCooltime_max);
            ai_autoDodgeRoadCooltimeCounter = 0f;
        }
    }

    //�ٸ� �÷��̾� ���ϱ�
    private void DodgePlayer_AI()
    {
        if (ai_skillsUsedList.Contains(AI_Skills.DodgePlayer) == false)
            return;

        if (ai_autoDodgePlayerCooltimeCounter < float.MaxValue)
            ai_autoDodgePlayerCooltimeCounter += Runner.DeltaTime;
        if (IsFrontPlayerExsisting() == true
            && ai_autoDodgePlayerCooltimeCounter > ai_autoDodgePlayerCooltime)
        {
            //�ٸ� �÷��̾� ���ϱ⸦ �ߵ����� ����
            int random1 = UnityEngine.Random.Range(0, 100);
            if (random1 < (int)(ai_ref_autoDodgePlayerProbability / 100)) //x%�� Ȯ���� �ߵ� 
            {
                //�������� ���� ���������� ������ ����....
                int random2 = UnityEngine.Random.Range(0, 100);
                if (random2 < 50)
                {
                    if (IsLeftValidLane_AI(client_currentLaneType, client_currentMoveIndex))
                        ActivateAction(AI_Action.ChangeLane_Left);
                    else if (IsRightValidLane_AI(client_currentLaneType, client_currentMoveIndex))
                        ActivateAction(AI_Action.ChangeLane_Right);
                }
                else
                {
                    if (IsRightValidLane_AI(client_currentLaneType, client_currentMoveIndex))
                        ActivateAction(AI_Action.ChangeLane_Right);
                    else if (IsLeftValidLane_AI(client_currentLaneType, client_currentMoveIndex))
                        ActivateAction(AI_Action.ChangeLane_Left);

                }

                ai_leftRightCooltimeCounter = 0f; //�������� �¿� �̵��ϴ� Ÿ�̸ӵ� �ʱ�ȭ��������
            }

            ai_autoDodgePlayerCooltime = UnityEngine.Random.Range(ai_ref_autoDodgePlayerCooltime_min, ai_ref_autoDodgePlayerCooltime_max);
            ai_autoDodgePlayerCooltimeCounter = 0f;
        }
    }

    //��ֹ� ���ϱ�
    private void DodgeObstacles_AI()
    {
        if (ai_skillsUsedList.Contains(AI_Skills.DodgeObstacles) == false)
            return;

        if (ai_autoDodgeObstacleCooltimeCounter < float.MaxValue)
            ai_autoDodgeObstacleCooltimeCounter += Runner.DeltaTime;
        if (IsFrontObstacleExsisting() == true
            && ai_autoDodgeObstacleCooltimeCounter > ai_autoDodgeObstacleCooltime)
        {
            //Obstacle ���ϱ⸦ �ߵ����� ����
            int random1 = UnityEngine.Random.Range(0, 100);
            if (network_isEnteredTheFinishLine)
                random1 = 0; //����� ���

            if (random1 < (int)(ai_ref_autoDodgeObstacleProbability / 100)) //x%�� Ȯ���� �ߵ� 
            {
                //�������� ���� ���������� ������ ����....
                int random2 = UnityEngine.Random.Range(0, 100);
                if (random2 < 50)
                {
                    if (IsLeftValidLane_AI(client_currentLaneType, client_currentMoveIndex))
                        ActivateAction(AI_Action.ChangeLane_Left);
                    else if (IsRightValidLane_AI(client_currentLaneType, client_currentMoveIndex)) //������ ���������� ���������� ��������
                        ActivateAction(AI_Action.ChangeLane_Right);
                }
                else
                {
                    if (IsRightValidLane_AI(client_currentLaneType, client_currentMoveIndex))
                        ActivateAction(AI_Action.ChangeLane_Right);
                    else if (IsLeftValidLane_AI(client_currentLaneType, client_currentMoveIndex)) //�������� ���������� �������� ��������
                        ActivateAction(AI_Action.ChangeLane_Left);

                }
            }

            ai_autoDodgeObstacleCooltime = UnityEngine.Random.Range(ai_ref_autoDodgeObstacleCooltime_min, ai_ref_autoDodgeObstacleCooltime_max);
            ai_autoDodgeObstacleCooltimeCounter = 0f;
        }
    }

    //�ν��� ���
    private void Booster_AI()
    {
        if (ai_skillsUsedList.Contains(AI_Skills.Booster) == false)
            return;

        if (network_isEnteredTheFinishLine)
            return;

        if (isShield == false)
        {
            CarBoosterType boosterLv = GetAvailableInputBooster();

            //3�ܰ� �ߵ� �Ҽ� ������ ���� �ٷ� ���� �ְԲ� ó��...!!
            if (ai_skillsUsedList.Contains(AI_Skills.UseBoosterWhenBoosterThreeIsAvailable) == true)
            {
                if (isBoosting == false && boosterLv == CarBoosterType.CarBooster_LevelThree)
                    ai_boosterInputCooltimeCounter = ai_boosterInputCooltime * 0.65f;
            }

            if (ai_boosterInputCooltimeCounter < float.MaxValue)
                ai_boosterInputCooltimeCounter += Runner.DeltaTime;
            if (ai_boosterInputCooltime <= ai_boosterInputCooltimeCounter)
            {
                if (boosterLv == CarBoosterType.CarBooster_LevelThree)
                {
                    ActivateAction(AI_Action.Booster);
                    ai_boosterInputCooltimeCounter = 0f;
                    ai_boosterInputCooltime = UnityEngine.Random.Range(ai_ref_boosterInputCooltime_min, ai_ref_boosterInputCooltime_max);
                }
                else if (boosterLv == CarBoosterType.CarBooster_LevelTwo)
                {
                    ActivateAction(AI_Action.Booster);
                    ai_boosterInputCooltimeCounter = 0f;
                    ai_boosterInputCooltime = UnityEngine.Random.Range(ai_ref_boosterInputCooltime_min, ai_ref_boosterInputCooltime_max);
                }
            }
        }
    }

    //Ÿ�̹� �ν��� ���
    private void TimingBooster_AI()
    {
        if (ai_skillsUsedList.Contains(AI_Skills.TimingBooster) == false)
            return;

        if (network_isEnteredTheFinishLine)
            return;

        //Timing Booster..

        if (ai_timingBoosterAvailable)
        {
            if (ai_timingBoosterCounter < float.MaxValue)
                ai_timingBoosterCounter += Runner.DeltaTime;

            if (ai_timingBoosterCounter >= ai_timingBoosterCooltime
                && isShield == false)
            {
                int random = UnityEngine.Random.Range(0, 100);
                if (random <= (int)(ai_ref_timingBoosterSuccessProbability / 100)) //Success
                {
                    isTimingBoosterActive = true; //�ߵ�~
                    CarBoosterType boosterLv = GetAvailableInputBooster();
                    if (boosterLv == CarBoosterType.CarBooster_LevelFour_Timing)
                    {
                        int random2 = UnityEngine.Random.Range(0, 100);
                        if (random2 < 50)
                            currentTimingBoosterSuccessType = TimingBoosterSuccessType.Great;
                        else
                            currentTimingBoosterSuccessType = TimingBoosterSuccessType.Perfect;

                        ActivateAction(AI_Action.Booster);
                    }
                }

                ResetTimingBooster_AI();
            }
        }
    }

    public void SetTimingBoosterAvailable_AI()
    {
        if (ai_timingBoosterAvailable == false)
        {
            ai_timingBoosterAvailable = true;
        }
    }

    private void ResetTimingBooster_AI()
    {
        ai_timingBoosterCounter = 0;
        ai_timingBoosterAvailable = false;
        ai_timingBoosterCooltime = UnityEngine.Random.Range(ai_ref_timingBoosterInputCooltime_min, ai_ref_timingBoosterInputCooltime_max);
    }

    //����,���� ���
    private void Sheild_AI()
    {
        if (ai_skillsUsedList.Contains(AI_Skills.UseDeceleration) == false)
            return;

        if (network_isEnteredTheFinishLine)
            return;

        bool activate = false;
        if (ai_decelerationStartCooltimeCounter < float.MaxValue)
            ai_decelerationStartCooltimeCounter += Runner.DeltaTime;

        if (inGameManager.totalTimeGameElapsed < 10) //�ʹ� 10�ʴ� ���x!
            return;

        foreach (var i in inGameManager.ListOfPlayers)
        {
            if (isShield == true)
                break;

            if (isShieldCooltime == true) //TODO...!
                break;

            if (i == null || i.pm == null || i.pm.Equals(this))
                continue;

            if (i.pm.isFlipped || i.pm.isOutOfBoundary) //������ ��볪 �ܰ��� �ִ� ����� ��� �ߵ�x
                continue;

            if (Vector3.Distance(this.transform.position, i.pm.transform.position) < ai_decelerationActivationDist
                && UtilityCommon.IsFront(transform.forward, transform.position, i.pm.transform.position) == false
                && ai_decelerationStartCooltimeCounter > ai_decelerationStartCooltime
                && Mathf.Abs((int)i.pm.client_currentLaneType - (int)client_currentLaneType) <= 1)
            {
                //���� Ȯ���� �ߵ� ��Ű��
                int random = UnityEngine.Random.Range(0, 100);

                int diff = Mathf.Abs((int)i.pm.client_currentLaneType - (int)client_currentLaneType);

                float aiNurff = 0;
                if (i.pm.IsAI) //AI�� ��� �ߵ� Ȯ�� ������...
                    aiNurff = 0.8f;

                if (diff == 0) //���� ������ ���
                {
                    if (random < ((int)(ai_ref_decelerationStartSameLaneProbability / 100)))
                        activate = true;
                }
                else //�ٸ� ������ ���
                {
                    if (random < ((int)(ai_ref_decelerationStartDiffLaneProbability / 100)))
                        activate = true;
                }

                ai_decelerationStartCooltime = UnityEngine.Random.Range(ai_ref_decelerationStartCooltime_min, ai_ref_decelerationStartCooltime_max);
                ai_decelerationStartCooltimeCounter = 0f; //�ʱ�ȭ....
                break;
            }
        }

        if (isShield)
        {
            if (ai_decelerationEndCooltimeCounter < float.MaxValue)
                ai_decelerationEndCooltimeCounter += Runner.DeltaTime;

            if (ai_skillsUsedList.Contains(AI_Skills.EndDecelerationAfterBlocking) == true)
            {
                //��� ������ ���... 
                if (ai_successInBlockingOtherPlayer)
                {
                    //�ֺ��� �ٸ� �÷��̾ ���� ���
                    if (ai_nearPlayerList != null && ai_nearPlayerList.Count <= 1)
                    {
                        //Decelration ����(?) �ٷ� ����������!!
                        if (ai_decelerationEndCooltimeCounter < ai_decelerationEndCooltime && ai_decelerationEndCooltimeCounter < ai_decelerationEndCooltime * 0.7f)
                            ai_decelerationEndCooltimeCounter = ai_decelerationEndCooltime * 0.7f;
                        ai_successInBlockingOtherPlayer = false;
                    }
                }
            }

            //Ÿ�̹� �ν��� �������� ���
            if (ai_timingBoosterAvailable)
            {
                //Decelration ����(?) �ٷ� ����������!!
                if (ai_decelerationEndCooltimeCounter < ai_decelerationEndCooltime && ai_decelerationEndCooltimeCounter < ai_decelerationEndCooltime * 0.8f)
                    ai_decelerationEndCooltimeCounter = ai_decelerationEndCooltime * 0.8f;
            }
        }
        else
        {
            ai_decelerationEndCooltimeCounter = 0f;
            ai_successInBlockingOtherPlayer = false;
        }

        if (activate)
        {
            ActivateAction(AI_Action.Shield);
            ai_decelerationStartCooltimeCounter = 0f;
        }
        else
        {
            if (isShield && ai_decelerationEndCooltimeCounter >= ai_decelerationEndCooltime) //TODO... �и� �������� ����?
            {
                //ActivateAction(AI_Action.Shield_End);
                ai_boosterInputCooltimeCounter = ai_boosterInputCooltime;
                ai_decelerationEndCooltime = UnityEngine.Random.Range(ai_ref_decelerationEndCooltime_min, ai_ref_decelerationEndCooltime_max);
                ai_decelerationEndCooltimeCounter = 0f;
            }
        }
    }

    private IEnumerator CheckMapObjects()
    {
        if (ai_isCheckingMapObjects)
            yield break;

        var mapManager = MapObjectManager.Instance;
        var ingameManager = InGameManager.Instance;
        ai_isCheckingMapObjects = true;
        while (true)
        {
            //�տ� ��ġ�� ����� Charge Pad ã��
            ai_nearChargePadList.Clear();

            foreach (var cp in mapManager.chargePadList)
            {
                if (cp == null)
                    continue;

                var dist = Vector3.Distance(transform.position, cp.transform.position);

                if (dist < ai_nearCheckChargePadDist
                    && client_currentMoveIndex <= cp.nearestWayPointIndex
                    && Mathf.Abs((int)client_currentLaneType - (int)cp.nearestWayPointLaneType) <= 1)
                {
                    ai_nearChargePadList.Add(cp);
                }
            }

            ai_nearChargePadList.Sort((x, y) =>
            {
                float dist1 = Vector3.Distance(x.transform.position, this.transform.position);
                float dist2 = Vector3.Distance(y.transform.position, this.transform.position);

                return dist1.CompareTo(dist2);
            });

            //����� Player ã��
            ai_nearPlayerList.Clear();
            foreach (var pl in ingameManager.ListOfPlayers)
            {
                if (networkPlayerID == null || pl == null || pl.pm == null || pl.photonNetworkID == null || pl.photonNetworkID.Equals(this.networkPlayerID))
                    continue;

                var dist = Vector3.Distance(transform.position, pl.pm.transform.position);
                if (dist < ai_nearCheckPlayerDist)
                    ai_nearPlayerList.Add(pl);
            }

            ai_nearPlayerList.Sort((x, y) =>
            {
                float dist1 = Vector3.Distance(x.go.transform.position, this.transform.position);
                float dist2 = Vector3.Distance(y.go.transform.position, this.transform.position);

                return dist1.CompareTo(dist2);
            });

            yield return new WaitForSecondsRealtime(0.25f);
        }
    }





    private bool IsFrontValidLane_AI(LaneType lane, int moveIndex)
    {
        var nextIndex = moveIndex;

        var pointList = GetWayPointsList(lane);

        if (pointList != null && pointList.Count > 0)
        {
            switch (pointList[nextIndex].currentWayPointType)
            {
                case WayPointSystem.Waypoint.WayPointType.Normal:
                case WayPointSystem.Waypoint.WayPointType.OnlyFront:
                case WayPointSystem.Waypoint.WayPointType.Sky:
                    {
                        return true;
                    }

                case WayPointSystem.Waypoint.WayPointType.Blocked:
                case WayPointSystem.Waypoint.WayPointType.OutOfBoundary:
                    {
                        return false;
                    }
            }
        }

        return false;
    }

    private bool IsLeftValidLane_AI(LaneType lane, int moveIndex)
    {
        LaneType nextLaneType = LaneType.None;
        if (lane == LaneType.Zero)
            return false;
        else
        {
             var nlane = (int)lane - 1;
            nextLaneType = (LaneType)nlane;
        }

        var nextIndex = GetNextMoveIndex(nextLaneType, moveIndex);
        var pointList = GetWayPointsList(nextLaneType);

        if (pointList != null && pointList.Count > 0)
        {
            switch (pointList[nextIndex].currentWayPointType)
            {
                case WayPointSystem.Waypoint.WayPointType.Normal:
                case WayPointSystem.Waypoint.WayPointType.OnlyFront:
                case WayPointSystem.Waypoint.WayPointType.Sky:
                    {
                        return true;
                    }

                case WayPointSystem.Waypoint.WayPointType.Blocked:
                case WayPointSystem.Waypoint.WayPointType.OutOfBoundary:
                    {
                        return false;
                    }
            }
        }

        return false;
    }

    private bool IsRightValidLane_AI(LaneType lane, int moveIndex)
    {
        LaneType nextLaneType;
        if (lane == LaneType.Six)
            return false;
        else
        {
            var nlane = (int)lane + 1;
            nextLaneType = (LaneType)nlane;
        }

        var nextIndex = GetNextMoveIndex(nextLaneType, moveIndex);
        var pointList = GetWayPointsList(nextLaneType);

        if (pointList != null && pointList.Count > 0)
        {
            switch (pointList[nextIndex].currentWayPointType)
            {
                case WayPointSystem.Waypoint.WayPointType.Normal:
                case WayPointSystem.Waypoint.WayPointType.OnlyFront:
                case WayPointSystem.Waypoint.WayPointType.Sky:
                    {
                        return true;
                    }

                case WayPointSystem.Waypoint.WayPointType.Blocked:
                case WayPointSystem.Waypoint.WayPointType.OutOfBoundary:
                    {
                        return false;
                    }
            }
        }

        return false;
    }

    private bool IsFrontPlayerExsisting(bool checkIfShield = true)
    {
        foreach (var i in inGameManager.ListOfPlayers)
        {
            if (i == null || i.pm == null || i.pm.Equals(this))
                continue;

            if (i.pm.isFlipped || i.pm.isOutOfBoundary) //������ ��볪 �ܰ��� �ִ� ����� ��� �ߵ�x
                continue;

            //���� �Ÿ� �̳��� �ְ� && ���� ���ο� �ְ� && �տ� �ִ� ���
            if (Vector3.Distance(this.transform.position, i.pm.transform.position) < ai_autoDodgePlayerActivationDist
                && UtilityCommon.IsFront(transform.forward, transform.position, i.pm.transform.position) == true
                && Mathf.Abs((int)i.pm.client_currentLaneType - (int)client_currentLaneType) == 0)
            {
                if (checkIfShield) //���� ���� �и��ϴ��� Ȯ�� �ϴ� ���̽�...!
                {
                    if (i.pm.isShield == true)
                        return true;
                    else
                        return false;
                }
                else
                    return true;
            }
        }
        
        return false;
    }

    private bool IsFrontObstacleExsisting(bool checkParring = true)
    {
        foreach (var i in MapObjectManager.Instance.obstacleList)
        {
            if (i == null || i.id <= -1)
                continue;

            var v1 = currDirection;
            var v2 = (i.ObstaclePosition - transform.position).normalized;

            //���� �Ÿ� �̳��� �ְ�  && �տ� �ִ� ��� && ���� ���� ���ʿ� �ִ� ���
            if (Vector3.Distance(this.transform.position, i.ObstaclePosition) < ai_autoDodgeObstacleActivationDist
                && UtilityCommon.IsFront(transform.forward, transform.position, i.ObstaclePosition) == true
                && Vector3.Angle(v1,v2) < 10)
            {
                return true;
            }
        }

        return false;
    }

    private void MoveRandomRightOrLeft_AI()
    {
        //��ó������� �ִ� ��� ���� Ȯ���� �¿� �̵���Ű��
        int additionalInt = 0;
        if (ai_nearPlayerList != null && ai_nearPlayerList.Count > 0)
            additionalInt = 20;

        int random = UnityEngine.Random.Range(0, 100);
        if (random < 15 + additionalInt)
        {
            if (IsLeftValidLane_AI(client_currentLaneType, client_currentMoveIndex))
            {
                ActivateAction(AI_Action.ChangeLane_Left);
            }
        }
        else if (random < 40 + additionalInt)
        {
            if (IsRightValidLane_AI(client_currentLaneType, client_currentMoveIndex))
                ActivateAction(AI_Action.ChangeLane_Right);
        }
        else
        {
            //����.... �¿� �̵�.x
        }
    }

    private void MoveToChargePad_AI()
    {
        foreach (var cp in ai_nearChargePadList)
        {
            if (cp.nearestWayPointLaneType == client_currentLaneType) //���� ������ �ǳʶ���
                continue;

            //�ѷ��� ���̳���
            if (Mathf.Abs((int)client_currentLaneType - (int)cp.nearestWayPointLaneType) == 1)
            {
                //����
                if ((int)client_currentLaneType > (int)cp.nearestWayPointLaneType)
                {
                    if (IsLeftValidLane_AI(client_currentLaneType, client_currentMoveIndex))
                    {
                        ActivateAction(AI_Action.ChangeLane_Left);
                        break;
                    }
                }
                //������
                else if ((int)client_currentLaneType < (int)cp.nearestWayPointLaneType)
                {
                    if (IsRightValidLane_AI(client_currentLaneType, client_currentMoveIndex))
                    {
                        ActivateAction(AI_Action.ChangeLane_Right);
                        break;
                    }
                }
            }
        }
    }



    private void ActivateAction(AI_Action type)
    {
        if (networkInGameRPCManager == null)
            return;

        switch (type)
        {
            case AI_Action.ChangeLane_Left:
                {
                    networkInGameRPCManager.RPC_ChangeLane_Left(this.networkPlayerID);
                }
                break;

            case AI_Action.ChangeLane_Right:
                {
                    networkInGameRPCManager.RPC_ChangeLane_Right(this.networkPlayerID);
                }
                break;

            case AI_Action.Booster:
                {
                    CarBoosterType boosterLv = GetAvailableInputBooster();
                    if (boosterLv == CarBoosterType.None)
                        return;

                    networkInGameRPCManager.RPC_BoostPlayer(this.networkPlayerID, (int)boosterLv, (int)currentTimingBoosterSuccessType);
                }
                break;

            case AI_Action.Shield:
                {
                    networkInGameRPCManager.RPC_Shield(this.networkPlayerID);
                }
                break;

        }
    }

    private void SuccessInBlockingOtherPlayer_AI(PlayerTriggerChecker.CheckParts parts)
    {
        ai_successInBlockingOtherPlayer = true;
    }
}
