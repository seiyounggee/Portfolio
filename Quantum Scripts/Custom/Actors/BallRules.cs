using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Deterministic;

namespace Quantum
{
    #region Data Client->Quantum
    public class BallData
    {
        public int BallStartSpeed = 10_000;  //millisecond 기준 (=10) 
        public int BallMinSpeed = 5_000;  //millisecond 기준 (=5)
        public int BallIncreaseSpeed = 200;  //millisecond 기준 (=0.2)
        public int BallMaxSpeed = 100_000;  //millisecond 기준 (=100)

        public int BallRotationSpeed = 5_000;  //millisecond 기준 (=5) 
        public int BallRotationMinSpeed = 5_000;  //millisecond 기준 (=5)
        public int BallRotationIncreaseSpeed = 200;  //millisecond 기준 (=0.2)
        public int BallRotationMaxSpeed = 50_000;  //millisecond 기준 (=50)

        public int DefaultAttackDamage = 5;

        public int BallCollisionCooltime = 200;  //millisecond 기준 (=0.2)
    }
    #endregion

    public unsafe partial struct BallRules
    {
        public void SetData(Frame frame, BallData data = default)
        {
            isActive = true;
            isNoDamage = false;

            if (data != null)
            {
                BallStartSpeed = data.BallStartSpeed * FP._0_01 * FP._0_10; ;
                BallSpeed = BallStartSpeed;
                BallMinSpeed = data.BallMinSpeed * FP._0_01 * FP._0_10; ;
                BallIncreaseSpeed = data.BallIncreaseSpeed * FP._0_01 * FP._0_10; ;
                BallMaxSpeed = data.BallMaxSpeed * FP._0_01 * FP._0_10; ;

                BallRotationSpeed = data.BallRotationSpeed * FP._0_01 * FP._0_10; ;
                BallRotationMinSpeed = data.BallRotationMinSpeed * FP._0_01 * FP._0_10; ;
                BallRotationIncreaseSpeed = data.BallRotationIncreaseSpeed * FP._0_01 * FP._0_10; ;
                BallRotationMaxSpeed = data.BallRotationMaxSpeed * FP._0_01 * FP._0_10; ;

                DefaultAttackDamage = data.DefaultAttackDamage;

                BallCollisionCooltime = data.BallCollisionCooltime * FP._0_01 * FP._0_10; ;
            }

            BallForcedDirection = FPVector3.Zero;
            BallForcedDist = FP._2;
            BallForcedDistCounter = FP._0;

            CurveBallForcedDist = FP._6;
            CurveBallForcedDistCounter = FP._0;

            TargetEntity = EntityRef.None;
            TargetPosition = FPVector3.Zero;

            ErrorCheckTimer = FP._0;
            ErrorValidTime = 20; //20초 안에 도달 안하면 에러로 판단.... 조치를 취하자

            BallCollisionCooltimeCounter = BallCollisionCooltime;

            CurrMovementLogic = BallMovementLogic.Default;
        }

        //타켓 갱신
        public void Update_Target(Frame f)
        {
            if (isActive == false)
                return;

            if (TargetEntity != Quantum.EntityRef.None)
            {
                if (f.Unsafe.TryGetPointer<Transform3D>(TargetEntity, out var transform))
                {
                    TargetPosition = transform->Position + FPVector3.Up * FP._1;
                }
            }
            else
            {
                //혹시나 TargetEntity가 없으면 Random으로 찾아서 타겟 설정하자... (이런 케이스가 거의 없...)
                if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gm))
                {
                    var randomTarget = gm->GetBallTarget(f, GetBallTargetType.Random, EntityRef.None);
                    if (randomTarget != EntityRef.None)
                        SetTarget(f, randomTarget, FPVector3.Zero);
                }
            }
        }

        //타켓으로 이동
        public void Update_Movement(Frame f)
        {
            if (f == null)
                return;

            if (isActive == false)
                return;

            if (SelfEntity == null)
                return;

            if (TargetEntity == Quantum.EntityRef.None)
                return;

            if (f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out var transform))
            {
                switch (CurrMovementLogic)
                {
                    default:
                    case BallMovementLogic.Default:
                    case BallMovementLogic.FastBall:
                    case BallMovementLogic.CurveBall:
                        {
                            var pos = transform->Position;

                            FPQuaternion rot = GetRotation(f, transform);

                            pos += transform->Forward * f.DeltaTime * GetSpeed(f, transform);

                            transform->Position = pos;
                            transform->Rotation = rot;

                            if (ErrorCheckTimer < ErrorValidTime)
                                ErrorCheckTimer += f.DeltaTime;
                        }
                        break;

                    case BallMovementLogic.SkyRocketBall:
                        {
                            if (FPVector3.Distance(SkyRocketBallGoingUpPosition, transform->Position) < FP._0_50)
                                SkyRocketBallGoingUpFinished = true;

                            var pos = transform->Position;

                            FPQuaternion rot = GetRotation(f, transform);

                            pos += transform->Forward * f.DeltaTime * GetSpeed(f, transform);

                            transform->Position = pos;
                            transform->Rotation = rot;

                            if (ErrorCheckTimer < ErrorValidTime)
                                ErrorCheckTimer += f.DeltaTime;
                        }
                        break;

                    case BallMovementLogic.FreezeBall:
                        {
                            FreezeBallCooltimeCounter -= f.DeltaTime;
                            if (FreezeBallCooltimeCounter <= 0)
                            {
                                CurrMovementLogic = BallMovementLogic.Default;
                            }
                        }
                        break;
                }
            }
        }

        public void Update_Raycast(Frame f)
        {
            Transform3D ballTrans = GetBallTransform(f).Value;

            var hit = f.Physics3D.Raycast(ballTrans.Position, ballTrans.Forward, FP._3);
            if (hit.HasValue)
            {
                if (f.Unsafe.TryGetPointer<MapObjectRules>(hit.Value.Entity, out var mo))
                {
                    if (mo->ObjectType == MapObjectType.StaticPillar || mo->ObjectType == MapObjectType.MovingPillar)
                    {
                        // 장애물을 감지했을 때
                        var reflectPos = FPVector3.Reflect(ballTrans.Forward, hit.Value.Normal);
                        BallObjstacleAvoidDirection = (ballTrans.Position - reflectPos).Normalized;

                        isRayCastHit = true;
                    }
                }
                else
                {
                    BallObjstacleAvoidDirection = FPVector3.Zero;

                    isRayCastHit = false;
                }
            }
            else
            {
                BallObjstacleAvoidDirection = FPVector3.Zero;
            }
        }

        //총돌 체크
        public void Update_Collision(Frame f)
        {
            if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager))
            {
                //게임 종료가 되었을 경우엔 충돌판정x
                if (f.Global->gamePlayState == GamePlayState.End)
                    return;

                if (BallCollisionCooltimeCounter > 0)
                    BallCollisionCooltimeCounter -= f.DeltaTime;

                //두 플레이어가 붙어있는 경우 몇프레임안에 데미지가 달아버림
                //프레임 단위로 데미지 들어가지 않게 살짝의 쿨타임 두자....
                if (BallCollisionCooltimeCounter <= 0)
                {
                    if (f.Unsafe.TryGetPointer<Transform3D>(gameManager->ball, out Transform3D* ballTrans))
                    {
                        var playerList = f.ResolveList(gameManager->ListOfPlayers_All);

                        foreach (var player in playerList)
                        {
                            if (f.Unsafe.TryGetPointer<PlayerRules>(player, out PlayerRules* targetPlayerRule))
                            {
                                if (FPVector3.Distance(ballTrans->Position, targetPlayerRule->GetPlayerPosition(f)) <= FP._1_50)
                                {
                                    //타겟이 해당 플레이어 인 경우에만 타격 판정! + 공격 상태가 아닐 경우
                                    if (TargetEntity.Equals(player) 
                                        && targetPlayerRule->isAttacking == false)
                                    {
                                        targetPlayerRule->HitPlayer(f, GetAttackDamage(f, TargetEntity, PreviousEntity));

                                        gameManager->ChangeBallTarget(f, player, FPVector3.Zero);

                                        BallCollisionCooltimeCounter = BallCollisionCooltime;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        public void SetTarget(Frame f, EntityRef target, FPVector3 direction)
        {
            PreviousEntity = TargetEntity;
            TargetEntity = target;

            IncreaseBallSpeed(1);

            BallSpeed = FPMath.Clamp(BallSpeed, BallMinSpeed, BallMaxSpeed);
            BallRotationSpeed = FPMath.Clamp(BallRotationSpeed, BallRotationMinSpeed, BallRotationMaxSpeed);

            //direction이 지정된 경우 강제로 일정거리동안 해당 방향으로 공을 보내주자
            if (direction != FPVector3.Zero && SelfEntity != EntityRef.None)
            {
                if (f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out var transform))
                {
                    IncreaseBallSpeed(2); //추가적으로 속도 더 올려주자

                    BallForcedDist = FPMath.Lerp(FP._3, FP._1, f.DeltaTime * BallRotationSpeed);
                    BallForcedDistCounter = BallForcedDist;
                    BallForcedDirection = direction.Normalized;
                    transform->Rotation = FPQuaternion.LookRotation(direction);
                }
            }

            //Target 바뀔때마다 체크 시간 다시 계산
            ErrorCheckTimer = FP._0;

            if (PreviousEntity != TargetEntity)
                ResetBallMovementLogic(f); //Target이 바뀐 경우 공에 적용된 스킬 초기화, 여러 스킬이 겹치지 않기 위해선 타겟 바뀔때마다 Reset하는 좋음
        }

        private void IncreaseBallSpeed(Int32 mutiplier = 1)
        {
            BallSpeed += BallIncreaseSpeed * mutiplier;
            BallRotationSpeed += BallRotationIncreaseSpeed * mutiplier;

            if (BallSpeed > BallMaxSpeed)
            {
                BallSpeed = BallMaxSpeed;
            }

            if (BallRotationSpeed > BallRotationMaxSpeed)
            {
                BallRotationSpeed = BallRotationMaxSpeed;
            }
        }

        public void ResetBallSpeed(Frame f)
        {
            //공속도 재 지정
            (BallSpeed, BallRotationSpeed) = GetResetBallSpeedAndRotation(f);
        }

        public void ResetBallMovementLogic(Frame f)
        {
            CurrMovementLogic = BallMovementLogic.Default;

            f.Events.BallEvents(SelfEntity, BallEvent.Event_ResetMovementLogic);
        }

        private FPQuaternion GetRotation(Frame f, Transform3D* transform)
        {
            var targetDir = (TargetPosition - transform->Position).Normalized;
            FPQuaternion rot = FPQuaternion.LookRotation(targetDir);

            if (isRayCastHit && BallObjstacleAvoidDirection != FPVector3.Zero)
            {
                //앞에 장애물있으면 피해서 가자...
                rot = FPQuaternion.LookRotation(BallObjstacleAvoidDirection);
            }
            else //장애물이 없는 경우
            {
                switch (CurrMovementLogic)
                {
                    case BallMovementLogic.Default: //기본 회전 로직
                        {
                            if (FPVector3.Distance(TargetPosition, transform->Position) < FPMath.Lerp(FP._0_50, FP._6, f.DeltaTime * BallRotationSpeed))
                            {
                                //그냥 직진
                                rot = FPQuaternion.LookRotation(targetDir);
                            }
                            else if (BallForcedDistCounter > FP._0 && BallForcedDirection != FPVector3.Zero)
                            {
                                //BallForcedDist 거리 만큼 BallForcedDirection 방향으로 직진

                                BallForcedDistCounter -= f.DeltaTime * GetSpeed(f, transform);

                                var dir = FPVector3.Lerp(transform->Forward.Normalized, BallForcedDirection.Normalized, FP._0_50);
                                rot = FPQuaternion.Lerp(transform->Rotation, FPQuaternion.LookRotation(dir), f.DeltaTime * BallRotationSpeed);

                                if (BallForcedDistCounter <= FP._0)
                                {
                                    BallForcedDirection = FPVector3.Zero;
                                    BallForcedDistCounter = FP._0;
                                }
                            }
                            else
                            {
                                //적당히 회전하면서...!
                                rot = FPQuaternion.Lerp(transform->Rotation, FPQuaternion.LookRotation(targetDir), f.DeltaTime * BallRotationSpeed);
                            }
                        }
                        break;

                    case BallMovementLogic.FastBall: //FastBall 스킬 사용시... 직진 하는 로직
                        {
                            //그냥 직진
                            rot = FPQuaternion.LookRotation(targetDir);
                        }
                        break;

                    case BallMovementLogic.CurveBall: //회전 하는 로직
                        {
                            FPVector3 orthogonalDirection1 = new FPVector3(-targetDir.Z, 0, targetDir.X);
                            //FPVector3 orthogonalDirection2 = new FPVector3(targetDir.Z, 0, -targetDir.X);

                            if (CurveBallForcedDistCounter > FP._0 && CurveBallForcedDirection != FPVector3.Zero)
                            {
                                CurveBallForcedDistCounter -= f.DeltaTime * GetSpeed(f, transform);

                                CurveBallRotationSpeed += f.DeltaTime * 10;
                                CurveBallRotationSpeed = FPMath.Clamp(CurveBallRotationSpeed, FP._0_10, BallRotationMaxSpeed);
                                rot = FPQuaternion.Lerp(transform->Rotation, FPQuaternion.LookRotation(CurveBallForcedDirection), f.DeltaTime * CurveBallRotationSpeed);

                                if (CurveBallForcedDistCounter <= FP._0)
                                {
                                    CurveBallForcedDistCounter = FP._0;
                                }
                            }
                            else
                            {
                                CurveBallRotationSpeed += f.DeltaTime * 20;
                                CurveBallRotationSpeed = FPMath.Clamp(CurveBallRotationSpeed, FP._0_10, BallRotationMaxSpeed);
                                rot = FPQuaternion.Lerp(transform->Rotation, FPQuaternion.LookRotation(targetDir), f.DeltaTime * CurveBallRotationSpeed);
                            }

                        }
                        break;

                    case BallMovementLogic.SkyRocketBall:
                        {
                            if (SkyRocketBallGoingUpFinished == false)
                            {
                                var dir = (SkyRocketBallGoingUpPosition - transform->Position).Normalized;
                                rot = FPQuaternion.LookRotation(dir);
                            }
                            else
                            {
                                rot = FPQuaternion.LookRotation(targetDir);
                            }
                        }
                        break;
                }
            }

            if (ErrorCheckTimer >= ErrorValidTime)
            {
                rot = FPQuaternion.LookRotation(targetDir);
            }

            return rot;
        }

        private FP GetSpeed(Frame f, Transform3D* transform)
        {
            var speed = BallSpeed;

            switch (CurrMovementLogic)
            {
                case BallMovementLogic.Default:
                    {
                        speed = BallSpeed;
                    }
                    break;

                case BallMovementLogic.FastBall:
                    {
                        speed = BallSpeed;
                        speed += BallIncreaseSpeed * 200;
                    }
                    break;

                case BallMovementLogic.CurveBall:
                    {
                        speed = BallSpeed;
                        speed += BallIncreaseSpeed * 100;
                    }
                    break;

                case BallMovementLogic.SkyRocketBall:
                    {
                        if (SkyRocketBallGoingUpFinished == false)
                        {
                            speed = BallStartSpeed;
                        }
                        else
                        {
                            speed = BallSpeed;
                            speed += BallIncreaseSpeed * 150;
                        }
                    }
                    break;
            }

            speed = FPMath.Clamp(speed, BallMinSpeed, BallMaxSpeed);

            if (FPVector3.Distance(TargetPosition, transform->Position) < FPMath.Lerp(FP._0_50, FP._6, f.DeltaTime * BallRotationSpeed))
            {
                speed = FPMath.Clamp(speed, BallMinSpeed, FP._10 * 3); //너무 빠르면 가까울 경우 그냥 통과해서 속도를 줄이자
            }

            if (ErrorCheckTimer >= ErrorValidTime)
            {
                speed = BallStartSpeed;
            }
   
            return speed;
        }

        public Transform3D? GetBallTransform(Frame f)
        {
            if (f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out var transform))
            {
                return *transform;
            }

            return null;
        }

        public FPVector3? GetBallPosition(Frame f)
        {
            if (f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out var transform))
            {
                return transform->Position;  
            }

            return null;
        }

        public int GetAttackDamage(Frame f, EntityRef targetPlayer, EntityRef attackPlayer)
        {
            int dmg = DefaultAttackDamage;

            if (f.Unsafe.TryGetPointer<PlayerRules>(attackPlayer, out var pr))
            {
                dmg += pr->GetPlayerAttackDamage(f);
            }

            if (isNoDamage)
                dmg = 0;

            return dmg;
        }

        #region Skill Related
        public void PlayerSkill_SetFastBall(Frame f)
        {
            CurrMovementLogic = BallMovementLogic.FastBall;

            f.Events.BallEvents(SelfEntity, BallEvent.Event_Active_FastBall);
        }

        public void PlayerSkill_SetCurveBall(Frame f)
        {
            CurrMovementLogic = BallMovementLogic.CurveBall;
            CurveBallRotationSpeed = 3;
            CurveBallForcedDistCounter = CurveBallForcedDist;

            Transform3D ballTrans = GetBallTransform(f).Value;

            var dir = (TargetPosition - ballTrans.Position).Normalized;

            // 수직 직교하는 벡터 계산 (좌우...)
            FPVector3 orthogonalDirection1 = new FPVector3(-dir.Z, 0, dir.X);
            FPVector3 orthogonalDirection2 = new FPVector3(dir.Z, 0, -dir.X);

            var randomNumber = f.Global->RngSession.Next(0, 100);
            if (randomNumber < 50)
            {
                CurveBallForcedDirection = orthogonalDirection1.Normalized;
                ballTrans.Rotation = FPQuaternion.LookRotation(CurveBallForcedDirection);
            }
            else
            {
                CurveBallForcedDirection = orthogonalDirection2.Normalized;
                ballTrans.Rotation = FPQuaternion.LookRotation(CurveBallForcedDirection);
            }

            f.Events.BallEvents(SelfEntity, BallEvent.Event_Active_CurveBall);
        }

        public void PlayerSKill_SetSkyRocketBall(Frame f, EntityRef attacker)
        {
            CurrMovementLogic = BallMovementLogic.SkyRocketBall;
            SkyRocketBallGoingUpFinished = false;
            if (f.Unsafe.TryGetPointer<Transform3D>(attacker, out var player_transform))
            {
                SkyRocketBallGoingUpPosition = player_transform->Position + FPVector3.Up * 10;
            }


            f.Events.BallEvents(SelfEntity, BallEvent.Event_Active_SkyRocketBall);
        }

        public void PlayerSkill_SetFreezeBall(Frame f)
        {
            CurrMovementLogic = BallMovementLogic.FreezeBall;
            FreezeBallCooltime = FP._3; //TODO...
            FreezeBallCooltimeCounter = FreezeBallCooltime;

            f.Events.BallEvents(SelfEntity, BallEvent.Event_Active_FreezeBall);
        }

        public void PlayerSkill_TakeBallTarget(Frame f, EntityRef player, FPVector3 direction)
        {
            if (TargetEntity != player)
                SetTarget(f, player, direction);
        }

        public void PlayerSkill_ChangeBallTarget(Frame f, EntityRef attacker, FPVector3 direction)
        {
            if (f.Unsafe.TryGetPointer<Transform3D>(SelfEntity, out var ball_transform)
                && f.Unsafe.TryGetPointer<Transform3D>(attacker, out var player_transform))
            {
                var dist = FPVector3.Distance(ball_transform->Position, player_transform->Position);

                if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager))
                {
                    var target = gameManager->GetBallTarget(f, GetBallTargetType.ClosestToFront, attacker);
                    if (target != EntityRef.None)
                    {
                        SetTarget(f, target, direction);
                        IncreaseBallSpeed(10); //공격 속도 증가
                    }
                }
            }
        }
        #endregion

        private (FP, FP) GetResetBallSpeedAndRotation(Frame f)
        {
            FP speed = BallStartSpeed;
            FP rotationSpeed = BallRotationSpeed;

            if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager))
            {
                var playerList = f.ResolveList(gameManager->ListOfPlayers_All);
                int aliveCount = 0;
                foreach (var player in playerList)
                {
                    if (f.Unsafe.TryGetPointer<PlayerRules>(player, out PlayerRules* pr))
                    {
                        if (pr->IsAlive)
                            ++aliveCount;
                    }
                }

                speed = BallStartSpeed + (playerList.Count - aliveCount) * BallIncreaseSpeed * 3;
                rotationSpeed = BallRotationSpeed + (playerList.Count - aliveCount) * BallRotationIncreaseSpeed * 3;

                speed = FPMath.Clamp(speed, BallMinSpeed, BallMaxSpeed);
                rotationSpeed = FPMath.Clamp(rotationSpeed, BallRotationMinSpeed, BallRotationMaxSpeed);
            }

            return (speed, rotationSpeed);
        }

        #region CHEAT KEY
        public void SetAttackDamage_Cheat(Frame f, int dmg = 0)
        {
            isNoDamage = !isNoDamage;

            if (dmg > 0)
                DefaultAttackDamage = dmg;
        }

        public void SetMaxSpeed_Cheat(Frame f)
        {
            BallSpeed = BallMaxSpeed;
            BallRotationSpeed = BallRotationMaxSpeed;
        }
        #endregion
    }
}
