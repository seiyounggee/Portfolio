using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Deterministic;
using Quantum.Custom;

namespace Quantum
{
    public unsafe partial struct AIPlayerRules
    {
        public static FP AI_ROOM_MOVE_LIMT_BOUNDARY_X = 35;
        public static FP AI_ROOM_MOVE_LIMT_BOUNDARY_Z = 35;

        public const int ATTACK_SECTION_1 = 1;
        public const int ATTACK_SECTION_2 = 2;
        public const int ATTACK_SECTION_ELSE = 99;

        public static FP ATTACK_SECTION_1_RANGE = 10; //공격자와 나의 거리가 distance 10인 구간 1
        public static FP ATTACK_SECTION_2_RANGE = 15; //공격자와 나의 거리가 distance 15인 구간 2
        public static FP ATTACK_SECTION_ELSE_RANGE = 999; //나머지 구간

        public void SetAIDifficulty(Frame f, int randomSeed, int matchMakingGroup)
        {
            RngSession = new RNGSession(100 + randomSeed);

            switch (matchMakingGroup)
            {
                case 0: //Beginner_Group
                    {
                        AIDifficulty = AIDifficulty.Easy;
                        RankingPoint = RngSession.Next(1, 2000);
                    }
                    break;

                default:
                case 1: //Normal_Group
                    {
                        var random = RngSession.Next(0, 100);
                        if (random > 50)
                        {
                            AIDifficulty = AIDifficulty.Normal;
                            RankingPoint = RngSession.Next(1000, 3500);
                        }
                        else
                        {
                            AIDifficulty = AIDifficulty.Hard;
                            RankingPoint = RngSession.Next(3500, 7000);
                        }
                    }
                    break;

                case 2: //Expert_Group
                    {
                        AIDifficulty = AIDifficulty.Expert;
                        RankingPoint = RngSession.Next(5000, 7000);
                    }
                    break;

                case 3: //Pro_Group //현재는 불가능... Pro Group엔 AI 존재x
                    {
                        AIDifficulty = AIDifficulty.Pro;
                        RankingPoint = RngSession.Next(6000, 10000);
                    }
                    break;
            }
        }

        public void SetData(Frame frame, int randomSeed)
        {
            if (frame.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out Transform3D* trans))
            {
                trans->Rotation = FPQuaternion.Euler(FPVector3.Zero);
            }
            RngSession = new RNGSession(randomSeed);

            aiPlayerState = AIPlayerState.None;
            stateCooltime = FP._5;
            stateCooltimeCounter = stateCooltime;

            isInputJump = false;
            jumpCooltime = FP._4;
            jumpCooltimeCounter = jumpCooltime;

            doubleJumpCooltime = FP._0_33;
            doubleJumpCooltimeCounter = 0;

            isInputAttack = false;
            attackCooltime = FP._2;
            attackCooltimeCounter = 0;

            isInputSkill = false;
            skillCooltime = 10;
            skillCooltimeCounter = 0;

            raycastStateChangeCooltime = FP._2;
            raycastStateChangeCooltimeCounter = 0;

            randomPosition = FPVector3.Zero;

        }

        public void SetCounter(Frame f, int index)
        {
            //미세하게 서로 다르게 세팅
            //그래야 랜덤 시드가 다르게 나와 다르게 보임 (같은 프레임에 설정하면 완전 똑같이 움직임)
            stateCooltimeCounter = stateCooltime - FP._0_20 * index;
            jumpCooltimeCounter = jumpCooltime - FP._0_10 * index;

            if (stateCooltimeCounter < 0)
                stateCooltimeCounter = 0;
            if (jumpCooltimeCounter < 0)
                jumpCooltimeCounter = 0;
        }

        public unsafe void Update_AI_Inputs(Frame frame)
        {
            if (frame.Unsafe.TryGetPointer<PlayerRules>(SelfEntity, out PlayerRules* playerRules))
            {
                var filter = new Quantum.Custom.InputSystem.Filter();
                filter.entity = SelfEntity;

                if (frame.Unsafe.TryGetPointer<CharacterController3D>(SelfEntity, out CharacterController3D* controller))
                    filter.characterController = controller;

                if (frame.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out Transform3D* trans))
                    filter.transform3d = trans;

                if (frame.Unsafe.TryGetPointer<PhysicsBody3D>(SelfEntity, out PhysicsBody3D* body))
                    filter.physicsBody3d = body;

                CheckForAIState(frame);

                CheckForFleePosition(frame);
                CheckForRandomPosition(frame);
                CheckFollowTarget(frame);
                CheckForAttack(frame);
                CheckForJump(frame);
                CheckForSkill(frame);

                Quantum.Input input = new Input();

                switch (aiPlayerState)
                {
                    case AIPlayerState.Idle:
                        {
                            input.horizontal = 0;
                            input.vertical = 0;

                            input.cameraDirection = FPVector3.Forward;
                        }
                        break;

                    case AIPlayerState.Chase:
                        {
                            if (FollowTargetEntity != EntityRef.None)
                            {
                                if (frame.Unsafe.TryGetPointer<Transform3D>(FollowTargetEntity, out Transform3D* target_trans)) { }
                                if (frame.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out Transform3D* this_trans)) { }

                                if (target_trans != null && this_trans != null)
                                {
                                    var dir = target_trans->Position - this_trans->Position;
                                    dir.Y = 0;
                                    dir = dir.Normalized;

                                    input.horizontal = dir.X;
                                    input.vertical = dir.Z;

                                    input.cameraDirection = FPVector3.Forward;
                                }
                            }
                        }
                        break;

                    case AIPlayerState.RandomPosition:
                        {
                            if (frame.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out Transform3D* this_trans)) { }

                            if (this_trans != null)
                            {
                                var dir = randomPosition - this_trans->Position;
                                dir.Y = 0;
                                dir = dir.Normalized;

                                input.horizontal = dir.X;
                                input.vertical = dir.Z;

                                input.cameraDirection = FPVector3.Forward;
                            }
                        }
                        break;

                    case AIPlayerState.FleeFromOtherPlayers:
                        {
                            if (frame.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out Transform3D* this_trans)) { }

                            if (this_trans != null)
                            {
                                fleeDirection = GetFleeDirection(frame);
                                var dir = fleeDirection;
                                dir.Y = 0;
                                dir = dir.Normalized;

                                input.horizontal = dir.X;
                                input.vertical = dir.Z;

                                input.cameraDirection = FPVector3.Forward;
                            }
                        }
                        break;

                }

                if (isInputJump)
                {
                    input.Jump = true;
                    isInputJump = false;
                }

                if (isInputAttack)
                {
                    input.Attack = true;
                    isInputAttack = false;
                }

                if (isInputSkill)
                {
                    input.Skill = true;
                    isInputSkill = false;
                }


                playerRules->Update_Movement(frame, input, filter);
            }
        }

        public unsafe void Update_Raycast(Frame f)
        {
            if (f == null || GetPlayerTransform(f) == null)
                return;

            Transform3D playerTrans = GetPlayerTransform(f).Value;

            var hit = f.Physics3D.Raycast(playerTrans.Position, playerTrans.Forward, FP._4);
            if (hit.HasValue)
            {
                if (f.Unsafe.TryGetPointer<MapObjectRules>(hit.Value.Entity, out var mo))
                {
                    if (mo->ObjectType == MapObjectType.StaticWall || mo->ObjectType == MapObjectType.StaticPillar)
                    {
                        // 일반 적인 장애물 + Wall 을 감지했을 때 State 바꿔주자
                        if (raycastStateChangeCooltimeCounter >= raycastStateChangeCooltime)
                        {
                            ForceAIState(f, AIPlayerState.Idle);
                            raycastStateChangeCooltimeCounter = 0;
                        }
                    }
                }
            }

            if (raycastStateChangeCooltimeCounter < raycastStateChangeCooltime)
                raycastStateChangeCooltimeCounter += f.DeltaTime;
        }

        private unsafe void CheckForAIState(Frame f)
        {
            stateCooltimeCounter += f.DeltaTime;

            if (stateCooltime <= stateCooltimeCounter)
            {
                ForceRandomAIState(f);
            }
        }

        private void ForceRandomAIState(Frame f)
        {
            //int random = f.RNG->Next((int)AIPlayerState.Idle, (int)AIPlayerState.Max);

            int random = RngSession.Next(0, 101);

            //가중치 다르게...!
            if (random >= 0 && random <= 60) //60%
            {
                ForceAIState(f, AIPlayerState.FleeFromOtherPlayers);
            }
            else if (random <= 70)  //10%
            {
                ForceAIState(f, AIPlayerState.RandomPosition);
            }
            else if (random <= 80)  //10%
            {
                ForceAIState(f, AIPlayerState.Chase);
            }
            else  //20%
            {
                ForceAIState(f, AIPlayerState.Idle);
            }

            stateCooltimeCounter = 0;
        }

        private void ForceAIState(Frame f, AIPlayerState state)
        {
            //Log.Error("aiPlayerState>>> " + state);

            stateCooltimeCounter = 0;

            switch (state)
            {
                case AIPlayerState.Idle:
                    {
                        aiPlayerState = AIPlayerState.Idle;
                        stateCooltime = RngSession.Next(FP._1, FP._3);
                    }
                    break;

                case AIPlayerState.Chase:
                    {
                        aiPlayerState = AIPlayerState.Chase;
                        stateCooltime = RngSession.Next(FP._4, FP._7);
                    }
                    break;

                case AIPlayerState.RandomPosition:
                    {
                        aiPlayerState = AIPlayerState.RandomPosition;
                        stateCooltime = RngSession.Next(FP._4, FP._7);
                        randomPosition = new FPVector3(RngSession.Next(-AI_ROOM_MOVE_LIMT_BOUNDARY_X, AI_ROOM_MOVE_LIMT_BOUNDARY_X), 0, f.RNG->Next(-AI_ROOM_MOVE_LIMT_BOUNDARY_Z, AI_ROOM_MOVE_LIMT_BOUNDARY_Z));
                    }
                    break;

                case AIPlayerState.FleeFromOtherPlayers:
                    {
                        aiPlayerState = AIPlayerState.FleeFromOtherPlayers;
                        stateCooltime = RngSession.Next(FP._4, FP._7);
                    }
                    break;
            }
        }

        private unsafe void CheckForFleePosition(Frame f)
        {
            if (aiPlayerState != AIPlayerState.FleeFromOtherPlayers)
                return;

            if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager)
                && f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out Transform3D* this_trans))
            {
                var playerList = f.ResolveList(gameManager->ListOfPlayers_All);
                FP nearestDist = FP.MaxValue;
                foreach (var player in playerList)
                {
                    if (f.Unsafe.TryGetPointer<PlayerRules>(player, out PlayerRules* pr))
                    {
                        if (pr->SelfEntity.Equals(SelfEntity))
                            continue;

                        var dist = FPVector3.Distance(this_trans->Position, pr->GetPlayerPosition(f));
                        if (dist < nearestDist)
                            nearestDist = dist;
                    }
                }

                if (nearestDist > 20)
                {
                    //충분히 멀어진 경우 다른 State로...!
                    ForceRandomAIState(f);
                    return;
                }

                if (this_trans->Position.X < -AI_ROOM_MOVE_LIMT_BOUNDARY_X
                    || this_trans->Position.X > AI_ROOM_MOVE_LIMT_BOUNDARY_X
                    || this_trans->Position.Z < -AI_ROOM_MOVE_LIMT_BOUNDARY_Z
                    || this_trans->Position.Z > AI_ROOM_MOVE_LIMT_BOUNDARY_Z)
                {
                    //충분히 멀어진 경우 다른 State로...!
                    ForceRandomAIState(f);
                    return;
                }
            }
        }

        private unsafe void CheckForRandomPosition(Frame f)
        {
            if (aiPlayerState != AIPlayerState.RandomPosition)
                return;

            if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager)
                && f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out Transform3D* this_trans))
            {
                var dist = FPVector3.Distance(this_trans->Position, randomPosition);
                if (dist <= 3)
                {
                    //도착
                    ForceRandomAIState(f);
                    return;
                }

                var playerList = f.ResolveList(gameManager->ListOfPlayers_All);
                FP nearestDist = FP.MaxValue;
                foreach (var player in playerList)
                {
                    if (f.Unsafe.TryGetPointer<PlayerRules>(player, out PlayerRules* pr))
                    {
                        if (pr->SelfEntity.Equals(SelfEntity))
                            continue;

                        var dist2 = FPVector3.Distance(this_trans->Position, pr->GetPlayerPosition(f));
                        if (dist2 < 5)
                        {
                            //랜덤 지역으로 가다가 다른 플레이어와 너무 가까워 지면 State 바꿔주자
                            //덜 겹치게 하기 위해...
                            ForceRandomAIState(f);
                            return;
                        }
                    }
                }
            }
        }

        private unsafe void CheckFollowTarget(Frame f)
        {
            if (aiPlayerState != AIPlayerState.Chase)
                return;

            if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager))
            {
                //FollowTargetEntity 있는 경우 도착했는지 체크
                if (FollowTargetEntity != EntityRef.None)
                {
                    if (f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out Transform3D* trans_self)
                        && f.Unsafe.TryGetPointer<Transform3D>(FollowTargetEntity, out Transform3D* trans_other))
                    {
                        var dist = FPVector3.Distance(trans_self->Position, trans_other->Position);
                        if (dist <= 15)
                        {
                            //도착
                            FollowTargetEntity = EntityRef.None;
                            ForceAIState(f, AIPlayerState.Idle);
                            return;
                        }
                    }
                }
                else //FollowTargetEntity 없는 경우 새로운 Target 찾자
                {
                    var list = f.ResolveList(gameManager->ListOfPlayers_All);

                    //FollowType
                    int followType = RngSession.Next(0, 2);

                    //최소 거리에 위치한 놈 Follow
                    if (followType == 0)
                    {
                        FP minDist = FP.MaxValue;
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (SelfEntity == list[i])
                                continue;

                            if (f.Unsafe.TryGetPointer<PlayerRules>(list[i], out PlayerRules* pr))
                            {
                                if (pr->isDead)
                                    continue;
                            }

                            if (f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out Transform3D* trans_self)
                                && f.Unsafe.TryGetPointer<Transform3D>(list[i], out Transform3D* trans_other))
                            {
                                var dist = FPVector3.Distance(trans_self->Position, trans_other->Position);
                                if (dist < minDist && dist > 15)
                                {
                                    //가장 가까운놈으로 이동
                                    minDist = dist;
                                    FollowTargetEntity = list[i];
                                }
                            }
                        }
                    }

                    //랜덤 한명 Follow
                    if (followType == 1)
                    {
                        FP minDist = FP.MaxValue;
                        int index = f.RNG->Next(0, list.Count);

                        for (int i = 0; i < list.Count; i++)
                        {
                            if (SelfEntity == list[index])
                                continue;

                            if (f.Unsafe.TryGetPointer<PlayerRules>(list[index], out PlayerRules* pr))
                            {
                                if (pr->isDead)
                                    continue;
                            }

                            if (f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out Transform3D* trans_self)
                                && f.Unsafe.TryGetPointer<Transform3D>(list[index], out Transform3D* trans_other))
                            {
                                var dist = FPVector3.Distance(trans_self->Position, trans_other->Position);
                                if (dist > 15)
                                {
                                    FollowTargetEntity = list[index];
                                    break;
                                }
                            }

                            ++index;
                            if (index >= list.Count)
                                index = 0;
                        }
                    }
                }
            }
        }

        private unsafe void CheckForAttack(Frame f)
        {
            if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager))
            {
                var ball = gameManager->ball;
                if (f.Unsafe.TryGetPointer<BallRules>(ball, out var ballRules)
                    && f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out Transform3D* trans_self)
                    && f.Unsafe.TryGetPointer<PlayerRules>(SelfEntity, out PlayerRules* playerRules_self))
                {
                    var ballPos = ballRules->GetBallPosition(f);
                    if (ballPos != null && ballPos.HasValue)
                    {
                        if (attackCooltimeCounter < attackCooltime && isInputAttack == false)
                            attackCooltimeCounter += f.DeltaTime;

                        if (ballRules->TargetEntity == SelfEntity 
                            && ballRules->PreviousEntity != SelfEntity
                            && attackCooltimeCounter >= attackCooltime
                            && isInputAttack == false)
                        {
                            var checkDist = FPMath.Lerp(playerRules_self->attackRange + FP._2, playerRules_self->attackRange + FP._4, f.DeltaTime * ballRules->BallRotationSpeed);
                            if (FPVector3.Distance(ballPos.Value, trans_self->Position) < checkDist)
                            {
                                if (f.Unsafe.TryGetPointer<Transform3D>(ballRules->PreviousEntity, out var trans_attacker))
                                {
                                    bool isAttack = false;
                                    int randomNum = RngSession.Next(0, 100);
                                    var dist = FPVector3.Distance(trans_attacker->Position, trans_self->Position);
                                    //거리에 따라 확률 다르게...!
                                    //나중에 추가 조건 넣자...!
                                    if (dist < ATTACK_SECTION_1_RANGE)
                                    {
                                        if (randomNum < GetAttackSuccessProbability(ATTACK_SECTION_1))
                                        {
                                            isAttack = true;
                                        }
                                    }
                                    else if (dist < ATTACK_SECTION_2_RANGE)
                                    {
                                        if (randomNum < GetAttackSuccessProbability(ATTACK_SECTION_2))
                                        {
                                            isAttack = true;
                                        }
                                    }
                                    else
                                    {
                                        if (randomNum < GetAttackSuccessProbability(ATTACK_SECTION_ELSE))
                                        {
                                            isAttack = true;
                                        }
                                    }

                                    if (isAttack)
                                    {
                                        isInputAttack = true;
                                        attackCooltimeCounter = attackCooltime * FP._0_75;
                                    }
                                    else
                                    {
                                        isInputAttack = false;
                                        attackCooltimeCounter = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private unsafe void CheckForJump(Frame f)
        {
            if (aiPlayerState == AIPlayerState.Chase || aiPlayerState == AIPlayerState.RandomPosition)
            {
                if (f.Unsafe.TryGetPointer<PlayerRules>(SelfEntity, out PlayerRules* playerRules))
                {
                    if (playerRules->isGrounded)
                    {
                        jumpCooltimeCounter += f.DeltaTime;
                        doubleJumpCooltimeCounter = 0;
                    }
                    else
                        doubleJumpCooltimeCounter += f.DeltaTime;

                    if (isInputJump == false && jumpCooltime <= jumpCooltimeCounter)
                    {
                        //첫번째 일반 점프
                        jumpCooltimeCounter = 0;
                        isInputJump = true;

                        jumpCooltime = RngSession.Next(FP._1, FP._3);
                    }

                    if (isInputJump == false && playerRules->isGrounded == false
                        && doubleJumpCooltime <= doubleJumpCooltimeCounter)
                    {
                        //확률 적으로 더블점프
                        int randomNum = RngSession.Next(0, 100);
                        if (randomNum < 70)
                        {
                            doubleJumpCooltimeCounter = 0;
                            isInputJump = true;
                        }
                        else
                        {
                            doubleJumpCooltimeCounter = 0;
                        }
                    }
                }
            }
        }

        private unsafe void CheckForSkill(Frame f)
        {
            if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager)) { }
            if (gameManager == null)
                return;

            if (skillCooltimeCounter < skillCooltime && isInputSkill == false)
                skillCooltimeCounter += f.DeltaTime;

            if (isInputSkill == false && skillCooltime <= skillCooltimeCounter)
            {
                var ball = gameManager->ball;
                if (f.Unsafe.TryGetPointer<PlayerRules>(SelfEntity, out PlayerRules* playerRules)
                    && f.Unsafe.TryGetPointer<BallRules>(ball, out var ballRules))
                {
                    //스킬 사용 가능할때
                    if (playerRules->inputSkillCooltimeCounter <= FP._0)
                    {
                        switch (playerRules->currentActiveSkillType)
                        {
                            case PlayerActiveSkill.Dash:
                                {
                                    isInputSkill = true;
                                    skillCooltimeCounter = 0;
                                    skillCooltime = playerRules->inputSkillCooltime + RngSession.Next(0, FP._3); 
                                }
                                break;
                            case PlayerActiveSkill.FreezeBall:
                                {
                                    isInputSkill = true;
                                    skillCooltimeCounter = 0;
                                    skillCooltime = playerRules->inputSkillCooltime + RngSession.Next(0, FP._5);
                                }
                                break;
                            case PlayerActiveSkill.FastBall:
                            case PlayerActiveSkill.CurveBall:
                            case PlayerActiveSkill.SkyRocketBall:
                                {
                                    //나인 경우에만 발동...?
                                    if (ballRules->TargetEntity == playerRules->SelfEntity)
                                    {
                                        isInputSkill = true;
                                        skillCooltimeCounter = 0;
                                        skillCooltime = playerRules->inputSkillCooltime + RngSession.Next(0, FP._3);
                                    }
                                }
                                break;
                            case PlayerActiveSkill.Shield:
                                {
                                    //나인 경우에만 발동...?
                                    if (ballRules->TargetEntity == playerRules->SelfEntity)
                                    {
                                        isInputSkill = true;
                                        skillCooltimeCounter = 0;
                                        skillCooltime = playerRules->inputSkillCooltime + RngSession.Next(0, FP._10);
                                    }
                                }
                                break;
                            case PlayerActiveSkill.TakeBallTarget:
                                {
                                    //내가 아닌 경우에 발동
                                    if (ballRules->TargetEntity != playerRules->SelfEntity)
                                    {
                                        isInputSkill = true;
                                        skillCooltimeCounter = 0;
                                        skillCooltime = playerRules->inputSkillCooltime + RngSession.Next(FP._3, FP._5);
                                    }
                                }
                                break;
                            case PlayerActiveSkill.BlindZone:
                                {
                                    //TODO...
                                    skillCooltimeCounter = 0;
                                }
                                break;
                        }
                    }
                }
            }

        }

        public void PlayerRulesCallback_AttackSuccess(Frame f)
        {
            attackCooltimeCounter = attackCooltime;
        }

        public void PlayerRulesCallback_HitPlayer(Frame f)
        {
            attackCooltimeCounter = attackCooltime;
        }

        private FPVector3 GetFleeDirection(Frame f)
        {
            FPVector3 finalDirection = FPVector3.Zero;

            if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager)
                && f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out Transform3D* this_trans))
            {
                var playerList = f.ResolveList(gameManager->ListOfPlayers_All);
                foreach (var player in playerList)
                {
                    if (f.Unsafe.TryGetPointer<PlayerRules>(player, out PlayerRules* pr))
                    {
                        if (pr->SelfEntity.Equals(SelfEntity) || pr->isDead)
                            continue;

                        FPVector3 directionFromEnemy = this_trans->Position - pr->GetPlayerPosition(f);
                        if (directionFromEnemy.SqrMagnitude != 0)
                            fleeDirection += directionFromEnemy.Normalized / directionFromEnemy.SqrMagnitude;
                    }
                }

                finalDirection = fleeDirection.Normalized;
            }

            
            return finalDirection;
        }

        public Transform3D? GetPlayerTransform(Frame f)
        {
            if (f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out var transform))
            {
                return *transform;
            }

            return null;
        }

        public int GetAttackSuccessProbability(int section)
        {
            //공격자와 AI와의 거리가 짧을 수록 공 칠 확률(prop)이 줄어듦

            int prop = 75;
            switch (AIDifficulty)
            {
                case AIDifficulty.Easy:
                    {
                        switch (section)
                        {
                            case 1:
                                prop = 60;
                                break;
                            case 2:
                                prop = 70;
                                break;
                            default:
                                prop = 85;
                                break;
                        }
                    }
                    break;

                case AIDifficulty.Normal:
                    {
                        switch (section)
                        {
                            case 1:
                                prop = 70;
                                break;
                            case 2:
                                prop = 80;
                                break;
                            default:
                                prop = 90;
                                break;
                        }
                    }
                    break;

                case AIDifficulty.Hard:
                    {
                        switch (section)
                        {
                            case 1:
                                prop = 85;
                                break;
                            case 2:
                                prop = 90;
                                break;
                            default:
                                prop = 95;
                                break;
                        }
                    }
                    break;

                case AIDifficulty.Expert:
                    {
                        switch (section)
                        {
                            case 1:
                                prop = 88;
                                break;
                            case 2:
                                prop = 94;
                                break;
                            default:
                                prop = 98;
                                break;
                        }
                    }
                    break;

                case AIDifficulty.Pro:
                    {
                        switch (section)
                        {
                            case 1:
                                prop = 95;
                                break;
                            case 2:
                                prop = 97;
                                break;
                            default:
                                prop = 99;
                                break;
                        }
                    }
                    break;
            }

            return prop;
        }

        public string GetRandomAINickname(int randomSeed)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int length = 10;
            StringBuilder result = new StringBuilder(length);

            RngSession = new RNGSession(randomSeed);
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[RngSession.Next(0, chars.Length)]);
            }

            return "User" + result.ToString();
        }
    }
}
