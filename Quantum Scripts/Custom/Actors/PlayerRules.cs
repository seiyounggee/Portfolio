using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quantum.Core;
using Quantum.Physics3D;
using Quantum.Custom;
using Photon.Deterministic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using static Quantum.Custom.InputSystem;

namespace Quantum
{
    #region Data Client->Quantum
    public class PlayerData
    {
        public int TeamID = -1;

        public int Character_SkinID;
        public int Weapon_SkinID;
        public int Passive_SkillID;
        public int Active_SkillID;

        public int MaxHealthPoint = 20;
        public int MaxSpeed = 8_000;    // (8FP)
        public int AttackDamage = 0;    // 공격력
        public int AttackRange = 2_500;  //millimeter 기준 (2.5미터)
        public int AttackDuration = 500;  //millisecond 기준 (0.5초)

        public int Input_AttackCooltime = 2_000; //millisecond 기준 (2초)
        public int Input_JumpCooltime = 1_000; //millisecond 기준 (1초)
        public int Input_SkillCooltime = 10_000; //millisecond 기준 (10초)
    }
    #endregion

    public unsafe partial struct PlayerRules
    {
        public void SetData(Frame frame, EntityRef entity, PlayerType type, PlayerData data = default)
        {
            SelfEntity = entity;

            if (data != null)
            {
                teamID = data.TeamID;

                if (data.Active_SkillID >= 0 && data.Active_SkillID < (int)PlayerActiveSkill.Max)
                    currentActiveSkillType = (PlayerActiveSkill)data.Active_SkillID;

                if (data.Passive_SkillID >= 0 && data.Passive_SkillID < (int)PlayerPassiveSkill.Max)
                    currentPassiveSkillType = (PlayerPassiveSkill)data.Passive_SkillID;

                maxHealthPoint = data.MaxHealthPoint;
                currHealthPoint = maxHealthPoint;

                attackDamage = data.AttackDamage;
                attackRange = data.AttackRange * FP._0_01 * FP._0_10;
                attackDuration = data.AttackDuration * FP._0_01 * FP._0_10;

                maxSpeed = data.MaxSpeed * FP._0_01 * FP._0_10;

                inputAttackCooltime = data.Input_AttackCooltime * FP._0_01 * FP._0_10;
                inputJumpCooltime = data.Input_JumpCooltime * FP._0_01 * FP._0_10;
                inputSkillCooltime = data.Input_SkillCooltime * FP._0_01 * FP._0_10;
            }

            isIdle = true;
            isRunning = false;
            isJumping = false;
            isAttacking = false;
            isSkill = false;
            isDead = false;

            gravity = FP._10 * FP.Minus_1;

            jumpCounter = 0;
            jumpMaxCount = 2;
            jumpDuration = FP._1;

            inputAttackCooltimeCounter = 0;
            inputJumpCooltimeCounter = 0;
            inputSkillCooltimeCounter = 0;

            isAutoAttack = false;
            isSkill_Invincible = false;

            playerType = type;

            ListOfSkilLaunchlTiming = frame.AllocateList<PlayerSkilLaunchTiming>();
            ListOfSkillRemoveTiming = frame.AllocateList<PlayerSkilRemoveTiming>();
        }

        public void Update(Frame f)
        {

        }

        public void Onremoved(Frame f, EntityRef entity, PlayerRules* component)
        {
            f.FreeList(ListOfSkilLaunchlTiming);
            f.FreeList(ListOfSkillRemoveTiming);
        }

        public void SetPosition(Frame f, EntityRef entity)
        {
            var mapSettings = f.FindAsset<CustomMapDataSettings>(f.RuntimeConfig.CustomMapDataSettingsRef.Id);

            if (mapSettings != null)
            {
                //Spawn Point 에 Spawn 시키기...!

                if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager))
                {
                    var playerList = f.ResolveList(gameManager->ListOfPlayers_All);
                    int indexCounter = 0;
                    foreach (var i in playerList)
                    {
                        if (i.Equals(entity))
                            break;

                        ++indexCounter;
                    }

                    (FPVector3 pos, FPQuaternion rot) = mapSettings.GetSpawnPoint(f, indexCounter);

                    //Log.Error("entity>> " + entity + "  | " + pos + " | " + indexCounter);

                    if (f.Unsafe.TryGetPointer<Transform3D>(entity, out Transform3D* tr))
                    {
                        tr->Position = pos;
                        tr->Rotation = rot;
                    }
                }
            }
            else
            {
                //null 인 경우는 없긴 할텐데... 혹시 모르니까 코드 남겨두자
                if (f.Unsafe.TryGetPointer<Transform3D>(entity, out Transform3D* tr))
                {
                    if (entity.Index < CommonDefine.SpawnPositionArray.Length)
                    {
                        tr->Position = CommonDefine.SpawnPositionArray[entity.Index];
                    }
                    else
                    {
                        tr->Position = CommonDefine.RandomSpawnPosition(f);
                    }

                    tr->Rotation = FPQuaternion.Identity;
                }
            }
        }

        private void SetHit(Frame f)
        {
            f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Hit);
        }

        private void SetDead(Frame f)
        {
            f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Die);

            currHealthPoint = 0;

            isIdle = false;
            isRunning = false;
            isJumping = false;
            isAttacking = false;
            isSkill = false;
            isDead = true;

            isSkill_Invincible = false;
            isSkill_Shield = false;
            isSkill_Dash = false;

            if (f.Unsafe.TryGetPointer<PhysicsCollider3D>(SelfEntity, out PhysicsCollider3D* collider))
                collider->Enabled = false;

            if (f.Unsafe.TryGetPointer<PhysicsBody3D>(SelfEntity, out PhysicsBody3D* body))
                body->Enabled = false;
        }

        #region CHEAT KEY
        public void SetDead_Cheat(Frame f)
        {
            SetDead(f);
        }

        public void SetHealHp_Cheat(Frame f, int amount = 0)
        {
            if (isDead == false)
            {
                if (amount == 0)
                    currHealthPoint = maxHealthPoint;
                else
                    currHealthPoint += amount;

                if(currHealthPoint > maxHealthPoint)
                    currHealthPoint = maxHealthPoint;
            }
        }

        public void SetHp_Cheat(Frame f, int amount = 0)
        {
            if (isDead == false)
            {
                currHealthPoint = amount;
            }
        }

        public void SetAutoAttack_Cheat(Frame f)
        {
            isAutoAttack = !isAutoAttack;
        }

        public void SetInvincible_Cheat(Frame f)
        {
            isSkill_Invincible = !isSkill_Invincible;
        }

        #endregion

        //Update Movement
        public void Update_Movement(Frame f, Input input, InputSystem.Filter filter)
        {
            if (input.Jump)
            {
                if (inputJumpCooltimeCounter <= FP._0)
                {
                    //Jump
                    if (jumpCounter < jumpMaxCount && isDead == false && f.Global->gamePlayState == GamePlayState.Play)
                    {
                        ++jumpCounter;
                        isJumping = true;

                        if (jumpCounter == 2)
                            inputJumpCooltimeCounter = inputJumpCooltime;
                    }
                }

                input.Jump = false;
            }

            if (input.Skill)
            {
                if (inputSkillCooltimeCounter <= FP._0)
                {
                    //Skill
                    if (isSkill == false && isDead == false && f.Global->gamePlayState == GamePlayState.Play)
                    {
                        isSkill = true;
                        inputSkillCooltimeCounter = inputSkillCooltime;

                        switch (currentActiveSkillType)
                        {
                            case PlayerActiveSkill.FastBall:
                            case PlayerActiveSkill.CurveBall:
                            case PlayerActiveSkill.SkyRocketBall:
                                {
                                    input.Attack = true;
                                    inputAttackCooltimeCounter = FP._0;
                                }
                                break;
                        }
                    }
                }

                input.Skill = false;
            }

            if (input.Attack)
            {
                if (inputAttackCooltimeCounter <= FP._0)
                {
                    //Attack
                    if (isAttacking == false && isDead == false && f.Global->gamePlayState == GamePlayState.Play)
                    {
                        isAttacking = true;
                        attackDurationCounter = attackDuration;
                        inputAttackCooltimeCounter = inputAttackCooltime;

                        f.Events.PlayerEvents(filter.entity, PlayerEvent.Event_AttackAttempt);
                    }
                }

                input.Attack = false;
            }

            #region Movement
            FP h = input.horizontal;
            FP v = input.vertical;

            //카메라 방향으로 이동하는 로직
            FPVector3 camFoward = input.cameraDirection;
            FPVector3 camRight = FPVector3.Cross(input.cameraDirection, FPVector3.Down);
            camFoward.Y = 0;
            camRight.Y = 0;
            FPVector3 moveDir = (camFoward.Normalized * v + camRight.Normalized * h).Normalized;
            moveDir.Y = FP._0;
            #endregion


            #region Rotation
            FPVector3 rotationDir = new FPVector3(moveDir.X, FP._0, moveDir.Z);
            FPVector3 lerpedDir = FPVector3.Lerp(filter.transform3d->TransformDirection(FPVector3.Forward).Normalized, rotationDir.Normalized, FP._0_75);
            FPQuaternion finalRotation = FPQuaternion.Identity;

            //input 없는 경우 유지
            if (moveDir.X == FP._0 && moveDir.Z == FP._0)
            {
                finalRotation = FPQuaternion.LookRotation(filter.transform3d->TransformDirection(FPVector3.Forward).Normalized, FPVector3.Up);
            }
            else
            {
                finalRotation = FPQuaternion.LookRotation(lerpedDir, FPVector3.Up);
                finalRotation.X = FP._0;
                finalRotation.Z = FP._0;
            }

            #endregion

            if (filter.characterController->Grounded)
            {
                isGrounded = true;

                if (( moveDir.X != FP._0 || moveDir.Z != FP._0 ) && f.Global->gamePlayState == GamePlayState.Play)
                {
                    isRunning = true;
                    isIdle = false;
                }
                else
                {
                    isRunning = false;
                    isIdle = true;
                }
            }
            else
            {
                isGrounded = false;
            }

            if (isJumping)
            {
                FP? impulse = null;
                if (jumpCounter == 1)
                {
                    impulse = FP._1 * 25; //최소 15 이상이어야 안어색함...
                }
                else if (jumpCounter == 2)
                {
                    impulse = FP._1 * 30;
                }

                if (jumpCounter <= jumpMaxCount && isDead == false)
                {
                    filter.characterController->Jump(f, true, impulse);

                    if (jumpCounter == 1)
                        f.Events.PlayerEvents(filter.entity, PlayerEvent.Event_Jump_First);
                    if (jumpCounter == 2)
                        f.Events.PlayerEvents(filter.entity, PlayerEvent.Event_Jump_Second);

                }

                isJumping = false;
            }

            if (isJumping == false
                && filter.characterController->Jumped == false
                && filter.characterController->Grounded
                && jumpDurationCounter >= jumpDuration)
            {
                jumpCounter = 0;
                jumpDurationCounter = FP._0;
            }

            if (jumpDurationCounter < jumpDuration)
                jumpDurationCounter += f.DeltaTime;

            if (inputJumpCooltimeCounter > FP._0)
                inputJumpCooltimeCounter -= f.DeltaTime; //쿨타임중....
            else
                inputJumpCooltimeCounter = FP._0; //사용 가능한 상태


            if (isAttacking)
            {
                attackDurationCounter -= f.DeltaTime;

                var gameManager = f.Unsafe.GetOrAddSingletonPointer<GameManager>();
                if (gameManager != null && gameManager->ball != null)
                {
                    if (f.Unsafe.TryGetPointer<Transform3D>(gameManager->ball, out Transform3D* ballTrans)
                        && f.Unsafe.TryGetPointer<BallRules>(gameManager->ball, out BallRules* ballRules))
                    {
                        if (ballRules->TargetEntity.Equals(SelfEntity)
                            && FPVector3.Distance(ballTrans->Position, filter.transform3d->Position) < attackRange)
                        {
                            AttackSuccess(f, filter); //공격 성공에 대한 처리

                            inputAttackCooltimeCounter = FP._0;
                            attackDurationCounter = attackDuration;
                            isAttacking = false;
                        }
                    }
                }

                if (attackDurationCounter <= FP._0)
                {
                    attackDurationCounter = attackDuration;
                    isAttacking = false;
                }
            }

            if (inputAttackCooltimeCounter > FP._0)
                inputAttackCooltimeCounter -= f.DeltaTime; //쿨타임중....
            else
                inputAttackCooltimeCounter = FP._0; //사용 가능한 상태



            if (isSkill_Shield)
            {
                var gameManager = f.Unsafe.GetOrAddSingletonPointer<GameManager>();
                if (gameManager != null && gameManager->ball != null)
                {
                    if (f.Unsafe.TryGetPointer<Transform3D>(gameManager->ball, out Transform3D* ballTrans)
                        && f.Unsafe.TryGetPointer<BallRules>(gameManager->ball, out BallRules* ballRules))
                    {
                        //Effect 크기랑 맞춰야함....!
                        if (FPVector3.Distance(ballTrans->Position, filter.transform3d->Position) < attackRange * 15 * FP._0_10)
                        {
                            f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Skill_Deactive_Shield);

                            ShieldSuccess(f, filter); //공격 성공에 대한 처리
                            inputAttackCooltimeCounter = FP._0;
                            isSkill_Shield = false;
                        }
                    }
                }
            }


            if (isSkill)
            {
                if (IsActivateSkill(f))
                {
                    RegisterSkill(f);

                    if (GetSkillLaunchList(f).Contains(PlayerSkilLaunchTiming.InputImmediately))
                    {
                        ActivateSkill(f);
                    }

                    isSkill = false;
                }
            }

            //스킬 발동 중인 경우
            if (activeSkillDurationCounter > 0)
            {
                activeSkillDurationCounter -= f.DeltaTime;

                if(activeSkillDurationCounter <= 0)
                    DeactivateSkill(f); //스킬 발동 시간이 지난 경우 발동중인 스킬 제거
            }

            if (inputSkillCooltimeCounter > FP._0)
                inputSkillCooltimeCounter -= f.DeltaTime; //쿨타임중....
            else
                inputSkillCooltimeCounter = FP._0; //사용 가능한 상태

            if (isDead)
            {
                moveDir = FPVector3.Zero;
                finalRotation = FPQuaternion.LookRotation(filter.transform3d->TransformDirection(FPVector3.Forward).Normalized, FPVector3.Up);
            }

            //자동 공격 치트키...
            if (isAutoAttack)
            {
                if (isDead == false && f.Global->gamePlayState == GamePlayState.Play)
                {
                    var gameManager = f.Unsafe.GetOrAddSingletonPointer<GameManager>();
                    if (gameManager != null && gameManager->ball != null)
                    {
                        if (f.Unsafe.TryGetPointer<Transform3D>(gameManager->ball, out Transform3D* ballTrans)
                            && f.Unsafe.TryGetPointer<BallRules>(gameManager->ball, out BallRules* ballRules))
                        {
                            if (FPVector3.Distance(ballTrans->Position, filter.transform3d->Position) < attackRange
                                && ballRules->TargetEntity.Equals(SelfEntity))
                            {
                                //공격에 성공...!
                                AttackSuccess(f, filter);
                            }
                        }
                    }
                }
            }

            if (f.Global->gamePlayState != GamePlayState.Play)
            {
                moveDir = FPVector3.Zero;
                finalRotation = FPQuaternion.LookRotation(filter.transform3d->TransformDirection(FPVector3.Forward).Normalized, FPVector3.Up);
            }

            filter.characterController->Velocity = GetVelocity(f, *filter.characterController, * filter.transform3d);
            filter.characterController->MaxSpeed = GetMaxSpeed(f, *filter.characterController);
            filter.characterController->Move(f, filter.entity, moveDir);
            filter.transform3d->Rotation = finalRotation;
        }

        public void Update_OutOfBoundary(Frame f)
        {
            //허용된 범위 밖으로 빠져나간 경우...
            //버그에 의해 범위 벗어난 경우 처리....
            if (f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out var trans))
            {
                if (trans->Position.X > CommonDefine.ROOM_OUT_OF_BOUNDARY_XYZ_HALF
                  || trans->Position.X < -CommonDefine.ROOM_OUT_OF_BOUNDARY_XYZ_HALF
                  || trans->Position.Y > CommonDefine.ROOM_OUT_OF_BOUNDARY_XYZ_HALF
                  || trans->Position.Y < -CommonDefine.ROOM_OUT_OF_BOUNDARY_XYZ_HALF
                  || trans->Position.Z > CommonDefine.ROOM_OUT_OF_BOUNDARY_XYZ_HALF
                  || trans->Position.Z < -CommonDefine.ROOM_OUT_OF_BOUNDARY_XYZ_HALF)
                {
                    SetDead(f);
                    trans->Position = FPVector3.Zero;
                }
            }
        }

        public void Update_Raycast(Frame f)
        {
            Transform3D playerTrans = GetPlayerTransform(f);

            if (playerTrans.Equals(default))
                return;

            if (isGrounded == false) //점프중일때 바닥 감지 로직
            {
                var hit = f.Physics3D.Raycast(playerTrans.Position, playerTrans.Down, FP._0_33);
                if (hit.HasValue)
                {
                    if (hit.Value.Entity.Equals(EntityRef.None)) //Static Collider 충돌했을때 None
                    {
                        // 바닥 장애물을 감지했을 때
                        // 속도를 일부로 줄여주자... 안그럼 땅 뚫고 내려는 현상 발생
                        isNearToFloorWhenJump = true;
                    }
                }
                else
                    isNearToFloorWhenJump = false;
            }
            else
                isNearToFloorWhenJump = false;
        }

        private FPVector3 GetVelocity(Frame f, CharacterController3D cc, Transform3D trans)
        { 
            FPVector3 velocity = cc.Velocity;

            if (isSkill_Dash) //대쉬 스킬 사용한 경우
                velocity = trans.Forward.Normalized * 25;
            else
                velocity = cc.Velocity;

            if (isNearToFloorWhenJump && velocity.Y < 0)
            {
                //속도가 너무 빨라서 바닥 뚫는 현상 방지 차원
                velocity.Y = cc.Velocity.Y / 3;
            }

            return velocity;
        }

        private FP GetMaxSpeed(Frame f, CharacterController3D cc)
        {
            FP speed = cc.MaxSpeed;

            if (isSkill_Dash) //대쉬 스킬 사용한 경우
                speed = 25 ;
            else
                speed = maxSpeed;


            speed = FPMath.Clamp(speed, 5, 25); //혹시 모르니까...

            return speed;
        }

        private void AttackSuccess(Frame f, Filter filter)
        {
            var gameManager = f.Unsafe.GetPointerSingleton<GameManager>();

            if (gameManager == null)
                return;

            //스킬 발동
            if (GetSkillLaunchList(f).Contains(PlayerSkilLaunchTiming.Before_AttackSuccess))
                ActivateSkill(f);

            //공격에 성공...!

            f.Events.PlayerEvents(filter.entity, PlayerEvent.Event_AttackSuccess);

            gameManager->ChangeBallTarget(f, SelfEntity, filter.transform3d->Forward);

            //스킬 발동
            if (GetSkillLaunchList(f).Contains(PlayerSkilLaunchTiming.After_AttackSuccess))
                ActivateSkill(f);

            if (GetSkillRemoveList(f).Contains(PlayerSkilRemoveTiming.After_AttackSuccess))
                ClearRegisteredSkill(f);

            //AI 용
            if (playerType == PlayerType.AIPlayer)
            {
                if (f.Unsafe.TryGetPointer<AIPlayerRules>(SelfEntity, out var ai))
                    ai->PlayerRulesCallback_AttackSuccess(f);
            }
        }

        private void ShieldSuccess(Frame f, Filter filter)
        {
            var gameManager = f.Unsafe.GetPointerSingleton<GameManager>();

            if (gameManager == null)
                return;

            gameManager->ChangeBallTarget(f, SelfEntity, filter.transform3d->Forward);

            if (GetSkillRemoveList(f).Contains(PlayerSkilRemoveTiming.After_ShieldSuccess))
                ClearRegisteredSkill(f);
        }


        //데미지 들어오는 곳 (공에 의해...)
        public void HitPlayer(Frame f, int damage)
        {
            if (isDead == false && isSkill_Invincible == false)
            {
                currHealthPoint -= damage;
                if (currHealthPoint > 0)
                {
                    SetHit(f);
                }
                else if (currHealthPoint <= 0)
                {
                    //플레이어가 아웃되면 공 속도 초기화 하자
                    var gameManager = f.Unsafe.GetPointerSingleton<GameManager>();
                    if (gameManager != null && gameManager->ball != null)
                    {
                        //연습모드에서는 플레이어를 죽이지 말자...!
                        if (gameManager->CurrentInGamePlayMode == InGamePlayMode.PracticeMode)
                        {
                            SetHit(f); //hit 된것 처럼 연출...
                            return;
                        }

                        SetDead(f);

                        if (f.Unsafe.TryGetPointer<BallRules>(gameManager->ball, out BallRules* br))
                        {
                            br->ResetBallSpeed(f);
                        }
                    }
                }

                inputAttackCooltimeCounter = 0; //공격input 쿨타임 초기화

                //AI 용
                if (playerType == PlayerType.AIPlayer)
                {
                    if (f.Unsafe.TryGetPointer<AIPlayerRules>(SelfEntity, out var ai))
                    {
                        ai->PlayerRulesCallback_HitPlayer(f);
                    }
                }

                if (GetSkillRemoveList(f).Contains(PlayerSkilRemoveTiming.After_Hit))
                {
                    ClearRegisteredSkill(f);
                }
            }
        }

        #region Skill Related
        private bool IsActivateSkill(Frame f)
        {

            return true;
        }

        //스킬 등록 (언제 발동 되는지, 언제 스킬 종료 되는지)
        private void RegisterSkill(Frame f)
        {
            //Skill LaunchTiming
            //언제 스킬이 발동되는지 세팅
            var skillLaunchList = f.ResolveList(ListOfSkilLaunchlTiming);
            skillLaunchList.Clear();
            switch (currentActiveSkillType)
            {
                default:
                case PlayerActiveSkill.None:
                    {
                    }
                    break;

                case PlayerActiveSkill.Dash:
                case PlayerActiveSkill.FreezeBall:
                case PlayerActiveSkill.TakeBallTarget:
                case PlayerActiveSkill.BlindZone:
                case PlayerActiveSkill.Shield:
                case PlayerActiveSkill.ChangeBallTarget:
                    {
                        skillLaunchList.Add(PlayerSkilLaunchTiming.InputImmediately);
                    }
                    break;

                case PlayerActiveSkill.FastBall:
                case PlayerActiveSkill.CurveBall:
                case PlayerActiveSkill.SkyRocketBall:
                    {
                        skillLaunchList.Add(PlayerSkilLaunchTiming.After_AttackSuccess);
                    }
                    break;
            }

            //Skill Remove Timing
            //언제 스킬이 초기화되는지 세팅
            var skillRemoveList = f.ResolveList(ListOfSkillRemoveTiming);
            skillRemoveList.Clear();
            switch (currentActiveSkillType)
            {
                default:
                case PlayerActiveSkill.None:
                    {
                        skillRemoveList.Clear();
                    }
                    break;

                case PlayerActiveSkill.Dash:
                case PlayerActiveSkill.FreezeBall:
                case PlayerActiveSkill.TakeBallTarget:
                case PlayerActiveSkill.BlindZone:
                case PlayerActiveSkill.ChangeBallTarget:
                    {
                        skillRemoveList.Add(PlayerSkilRemoveTiming.After_SkillLaunch);
                    }
                    break;

                case PlayerActiveSkill.FastBall:
                case PlayerActiveSkill.CurveBall:
                case PlayerActiveSkill.SkyRocketBall:
                    {
                        skillRemoveList.Add(PlayerSkilRemoveTiming.After_AttackSuccess);
                        skillRemoveList.Add(PlayerSkilRemoveTiming.After_Hit);
                        skillRemoveList.Add(PlayerSkilRemoveTiming.After_TimeElapsed);
                    }
                    break;
                case PlayerActiveSkill.Shield:
                    {

                        skillRemoveList.Add(PlayerSkilRemoveTiming.After_ShieldSuccess);
                        skillRemoveList.Add(PlayerSkilRemoveTiming.After_TimeElapsed);
                    }
                    break;
            }

            //Quantum -> Unity Client 이벤트 처리
            switch (currentActiveSkillType)
            {
                case PlayerActiveSkill.FastBall:
                    {
                        f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Skill_Register_FastBall);
                    }
                    break;

                case PlayerActiveSkill.CurveBall:
                    {
                        f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Skill_Register_CurveBall);
                    }
                    break;

                case PlayerActiveSkill.SkyRocketBall:
                    {
                        f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Skill_Register_SkyRocketBall);
                    }
                    break;
            }
        }

        //등록 된 스킬 제거
        private void ClearRegisteredSkill(Frame f)
        {
            var skillLaunchList = f.ResolveList(ListOfSkilLaunchlTiming);
            skillLaunchList.Clear();

            var skillRemoveList = f.ResolveList(ListOfSkillRemoveTiming);
            skillRemoveList.Clear();
        }

        //스킬 발동
        private void ActivateSkill(Frame f)
        {
            var gameManager = f.Unsafe.GetPointerSingleton<GameManager>();

            if (gameManager == null || gameManager->ball == null)
                return;

            if (f.Unsafe.TryGetPointer<BallRules>(gameManager->ball, out BallRules* br)) { }
            if (f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out Transform3D* tr)) { }

            if (br == null || tr == null)
                return;

            switch (currentActiveSkillType)
            {
                case PlayerActiveSkill.None:
                    break;
                case PlayerActiveSkill.Dash:
                    {
                        PlayerSkill_Dash(f);

                        f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Skill_Active_Dash);
                    }
                    break;
                case PlayerActiveSkill.FreezeBall:
                    {
                        if (br->CurrMovementLogic != BallMovementLogic.Default)
                            return;

                        br->PlayerSkill_SetFreezeBall(f);

                        f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Skill_Active_FreezeBall);
                    }
                    break;
                case PlayerActiveSkill.FastBall:
                    {
                        if (br->CurrMovementLogic != BallMovementLogic.Default)
                            return;

                        br->PlayerSkill_SetFastBall(f);

                        f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Skill_Active_FastBall);
                    }
                    break;
                case PlayerActiveSkill.TakeBallTarget:
                    {
                        br->PlayerSkill_TakeBallTarget(f, SelfEntity, tr->Forward);

                        f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Skill_Active_TakeBallTarget);
                    }
                    break;
                case PlayerActiveSkill.BlindZone:
                    {
                        f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Skill_Active_BlindZone);
                    }
                    break;
                case PlayerActiveSkill.Shield:
                    {
                        PlayerSkill_Shield(f);

                        f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Skill_Active_Shield);
                    }
                    break;
                case PlayerActiveSkill.ChangeBallTarget:
                    {
                        br->PlayerSkill_ChangeBallTarget(f, SelfEntity, tr->Forward);

                        f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Skill_Active_ChangeBallTarget);
                    }
                    break;
                case PlayerActiveSkill.CurveBall:
                    {
                        if (br->CurrMovementLogic != BallMovementLogic.Default)
                            return;

                        br->PlayerSkill_SetCurveBall(f);

                        f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Skill_Active_CurveBall);
                    }
                    break;
                case PlayerActiveSkill.SkyRocketBall:
                    {
                        if (br->CurrMovementLogic != BallMovementLogic.Default)
                            return;

                        br->PlayerSKill_SetSkyRocketBall(f, SelfEntity);

                        f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Skill_Active_SkyRocketBall);
                    }
                    break;
            }

            activeSkillDurationCounter = activeSkillDuration;

            if (GetSkillRemoveList(f).Contains(PlayerSkilRemoveTiming.After_SkillLaunch))
            {
                ClearRegisteredSkill(f);
            }
        }

        private void DeactivateSkill(Frame f)
        {
            //발동중이 스킬 꺼주자...! (플레이에 영향주는 스킬만 효과가 있음... 공에 효과주는건 의미x)

            switch (currentActiveSkillType)
            {
                case PlayerActiveSkill.Dash:
                    if (isSkill_Dash)
                    {
                        isSkill_Dash = false;
                    }
                    break;

                case PlayerActiveSkill.Shield:
                    if (isSkill_Shield)
                    {
                        f.Events.PlayerEvents(SelfEntity, PlayerEvent.Event_Skill_Deactive_Shield);
                        isSkill_Shield = false;
                    }
                    break;
            }

            activeSkillDurationCounter = 0;
        }

        public Quantum.Collections.QList<PlayerSkilLaunchTiming> GetSkillLaunchList(Frame f)
        {
            var list = f.ResolveList(ListOfSkilLaunchlTiming);
            return list;
        }

        public Quantum.Collections.QList<PlayerSkilRemoveTiming> GetSkillRemoveList(Frame f)
        {
            var list = f.ResolveList(ListOfSkillRemoveTiming);
            return list;
        }

        public void PlayerSkill_Dash(Frame f)
        {
            isSkill_Dash = true;
            activeSkillDuration = FP._0_33; //일단 하드 코딩... 스킬 지속 시간 세팅
        }

        public void PlayerSkill_Shield(Frame f)
        {
            isSkill_Shield = true;
            activeSkillDuration = FP._3; //일단 하드 코딩... 스킬 지속 시간 세팅
        }


        #endregion

        public FPVector3 GetPlayerPosition(Frame f)
        {
            if (f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out var transform))
            {
                return transform->Position;
            }

            return FPVector3.Zero;
        }

        public Transform3D GetPlayerTransform(Frame f)
        {
            if (f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out var transform))
            {
                return *transform;
            }

            return default;
        }


        public bool IsAlive
        {
            get { return !isDead; }
        }

        public int GetPlayerAttackDamage(Frame f)
        {
            return attackDamage;        
        }

        //현재 사용x... 
        #region Physics Collsion Or Trigger
        public void OnCharacterCollision3D(FrameBase f, EntityRef character, Hit3D hit)
        {
            //Log.Error("OnCharacterCollision3D!!!!!!!!");
        }

        public void OnCharacterTrigger3D(FrameBase f, EntityRef character, Hit3D hit)
        {
            //Log.Error("OnCharacterTrigger3D!!!!!!!!");
        }

        public void OnCollisionEnter3D(Frame f, CollisionInfo3D info)
        {
            //Log.Error("OnCollisionEnter3D!!!!!!!!");

        }

        public void OnCollisionExit3D(Frame f, ExitInfo3D info)
        {
            //Log.Error("OnCollisionExit3D!!!!!!!!");
        }

        public void OnTrigger3D(Frame f, TriggerInfo3D info)
        {
            //Log.Error("OnTrigger3D!!!!!!!!");
        }

        public void OnTriggerEnter3D(Frame f, TriggerInfo3D info)
        {
            //Log.Error("OnTriggerEnter3D!!!!!!!!");
        }

        public void OnTriggerExit3D(Frame f, ExitInfo3D info)
        {
            //Log.Error("OnTriggerExit3D!!!!!!!!");
        }
        #endregion
    }
}
