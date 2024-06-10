using Photon.Deterministic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum
{
    public unsafe partial struct GameManager
    {
        public int RuntimeIndex => throw new NotImplementedException();

        public void OnInit(Frame f)
        {
            if (f == null)
                return;

            InGameTime = Photon.Deterministic.FP._0;
            ListOfPlayers_All = f.AllocateList<EntityRef>(); //꼭 allocate 해줘야함
            ListOfPlayers_Real = f.AllocateList<EntityRef>(); //꼭 allocate 해줘야함
            ListOfPlayers_AI = f.AllocateList<EntityRef>(); //꼭 allocate 해줘야함
            ListOfPlayers_SoloRanking = f.AllocateList<PlayerRankInfo>(); //꼭 allocate 해줘야함
            ListOfPlayers_TeamRanking = f.AllocateList<PlayerRankInfo>(); //꼭 allocate 해줘야함
            ListOfMapObject = f.AllocateList<EntityRef>(); //꼭 allocate 해줘야함
            ListOfPlayers_Statistics = f.AllocateList<InGamePlayStatisticsData>(); //꼭 allocate 해줘야함

            isEveryPlayerSpawned = false;
            isAIPlayerSpawned = false;

            totalPlayers = 0;
            realPlayers = 0;
            aiPlayers = 0;

            alivePlayerCount = 0;
            curr_aliveTeamCount = 0;

            CountDownTime = FP._1_50;

            SetGameState(f, GamePlayState.Wait);

            //랜덤 시드 설정...
            //이거 안해주면 최초가 항상 0임... 
            int newSeed = 100;
            f.Global->RngSession = new RNGSession(newSeed);
        }

        public void OnRemoved(Frame f, EntityRef entity, GameManager* component)
        {
            f.FreeList(ListOfPlayers_All);
            f.FreeList(ListOfPlayers_Real);
            f.FreeList(ListOfPlayers_AI);
            f.FreeList(ListOfPlayers_SoloRanking);
            f.FreeList(ListOfPlayers_TeamRanking);
            f.FreeList(ListOfMapObject);
            f.FreeList(ListOfPlayers_Statistics);
        }

        public void Update(Frame f)
        {
            if (f == null)
                return;

            Update_Timer(f);
            Update_GameState(f);
            Update_List(f);
            Update_AI(f);
            Update_Player(f);
            Update_Ball(f);
            Update_MapObject(f);
        }

        private void Update_Timer(Frame f)
        {
            if (InGameTime < FP.MaxValue)
                InGameTime += f.DeltaTime;

            if (f.Global->gamePlayState == GamePlayState.CountDown)
            {
                if (CountDownTime > FP.MinValue)
                    CountDownTime -= f.DeltaTime;
            }
        }

        private void Update_GameState(Frame f)
        {

            if (InGameTime <= Photon.Deterministic.FP._0)
            {
                SetGameState(f, GamePlayState.Wait);
            }
            else if (f.Global->gamePlayState == GamePlayState.Wait
                 && isEveryPlayerSpawned)
            {
                SetGameState(f, GamePlayState.CountDown);
            }

            if (f.Global->gamePlayState == GamePlayState.CountDown
                && CountDownTime <= FP._0)
            {
                SetGameState(f, GamePlayState.Play);
            }

            
            if (f.Global->gamePlayState == GamePlayState.Play)
            {

                #region Solo Rank 관련 처리
                alivePlayerCount = 0;
                EntityRef aliveEntity = EntityRef.None;

                var list = f.ResolveList(ListOfPlayers_All);
                for (int i = 0; i < list.Count; i++)
                {
                    if (f.Unsafe.TryGetPointer<PlayerRules>(list[i], out var playerRules))
                    {
                        if (playerRules->isDead == false)
                        {
                            alivePlayerCount += 1;

                            aliveEntity = playerRules->SelfEntity;
                        }
                        else
                        {
                            //2등~마지막 Rank 넣어주자
                            SetPlayerSoloRank(f, playerRules->SelfEntity);
                        }

                        if (list.Count - 1 == i && alivePlayerCount == 1 && aliveEntity != EntityRef.None)
                        {
                            //1등 Rank 넣어주자...
                            SetPlayerSoloRank(f, aliveEntity);
                        }
                    }
                }
                #endregion

                //TODO... 너무 쓰레기 코드같은데...? 나중에 리팩토링 가능하려나..
                #region Team Rank
                if (CurrentInGamePlayMode == InGamePlayMode.TeamMode)
                {
                    curr_aliveTeamCount = 0;
                    HashSet<int> teamList = new HashSet<int>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (f.Unsafe.TryGetPointer<PlayerRules>(list[i], out var playerRules))
                        {
                            if (playerRules->isDead == false)
                                teamList.Add(playerRules->teamID);
                        }
                    }
                    if (previous_aliveTeamCount == 0) //최초로 한번은...
                        previous_aliveTeamCount = teamList.Count;

                    curr_aliveTeamCount = teamList.Count;

                    if (curr_aliveTeamCount != previous_aliveTeamCount)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (f.Unsafe.TryGetPointer<PlayerRules>(list[i], out var playerRules))
                            {
                                if (playerRules->isDead)
                                {
                                    //2등~마지막 Team Rank 넣어주자
                                    SetPlayerTeamRank(f, playerRules->SelfEntity, curr_aliveTeamCount, false);
                                }
                            }
                        }

                        previous_aliveTeamCount = curr_aliveTeamCount;
                    }

                    if (curr_aliveTeamCount == 1)
                    {
                        //1등의 TeamId 가져오자...
                        int aliveTeamID = -1;
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (f.Unsafe.TryGetPointer<PlayerRules>(list[i], out var playerRules))
                            {
                                if (playerRules->IsAlive)
                                    aliveTeamID = playerRules->teamID;
                            }
                        }

                        //1등한 팀 모두에게 Team Rank 부여하자 (생존 여부와 상관없이 랭크는 1등임)
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (f.Unsafe.TryGetPointer<PlayerRules>(list[i], out var playerRules))
                            {
                                if (playerRules->teamID.Equals(aliveTeamID))
                                {
                                    //1등 Team Rank 넣어주자...
                                    SetPlayerTeamRank(f, playerRules->SelfEntity, 0, true);
                                }
                            }
                        }
                    }
                }
                #endregion

                //게임 종료 로직
                switch (CurrentInGamePlayMode)
                {
                    case InGamePlayMode.SoloMode:
                        {
                            //1명만 살았을 경우 End...
                            if (alivePlayerCount <= 1)
                            {
                                EndGame(f);
                            }
                        }
                        break;


                    case InGamePlayMode.TeamMode:
                        {
                            HashSet<int> teamHashSet = new HashSet<int>();
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (f.Unsafe.TryGetPointer<PlayerRules>(list[i], out var playerRules))
                                {
                                    if (playerRules->isDead == false)
                                        teamHashSet.Add(playerRules->teamID);
                                }
                            }

                            //1팀만 살았을 경우 End...
                            if (teamHashSet.Count <= 1)
                            {
                                EndGame(f);
                            }
                        }
                        break;

                    case InGamePlayMode.PracticeMode:
                        { 
                            //게임 종료x...?
                        }
                        break;
                }

            }

            var list_all = f.ResolveList(ListOfPlayers_All);
            if (f.Global->gamePlayState == GamePlayState.Wait)
            {
                if (isEveryPlayerSpawned == false)
                {
                    if (list_all.Count == totalPlayers && list_all.Count > 0)
                        isEveryPlayerSpawned = true;
                }
            }

        }

        private void EndGame(Frame f)
        {
            SetGameState(f, GamePlayState.End);

            if (CurrentInGamePlayMode == InGamePlayMode.SoloMode)
                f.Events.GameResult(CurrentInGamePlayMode, GetPlayerRankArray_Solo(f), GetInGamePlayStatisticsArray(f));
            else if (CurrentInGamePlayMode == InGamePlayMode.TeamMode)
                f.Events.GameResult(CurrentInGamePlayMode, GetPlayerRankArray_Team(f), GetInGamePlayStatisticsArray(f));

            if (f.Unsafe.TryGetPointer<BallRules>(ball, out var ballRules))
                ballRules->isActive = false;
        }

        private void Update_List(Frame f)
        {
            var list_all = f.ResolveList(ListOfPlayers_All);
            var list_real = f.ResolveList(ListOfPlayers_Real);
            var list_ai = f.ResolveList(ListOfPlayers_AI);
            foreach (var pair in f.Unsafe.GetComponentBlockIterator<PlayerRules>())
            {
                //요거 Spawn할때 이미 하긴 하는데 혹시 몰라서...
                if (list_all.Contains(pair.Entity) == false)
                    list_all.Add(pair.Entity);

                if (f.Unsafe.TryGetPointer<AIPlayerRules>(pair.Entity, out AIPlayerRules* ai))
                {
                    if (list_ai.Contains(pair.Entity) == false)
                        list_ai.Add(pair.Entity);
                }
                else
                {
                    if (list_real.Contains(pair.Entity) == false)
                        list_real.Add(pair.Entity);
                }
            }

            var list_mapObj = f.ResolveList(ListOfMapObject);
            foreach (var pair in f.Unsafe.GetComponentBlockIterator<MapObjectRules>())
            {
                if (list_mapObj.Contains(pair.Entity) == false)
                    list_mapObj.Add(pair.Entity);
            }
        }

        private void Update_Player(Frame f)
        {
            var list_all = f.ResolveList(ListOfPlayers_All);
            foreach (var entity in list_all)
            {
                if (f.Unsafe.TryGetPointer<PlayerRules>(entity, out PlayerRules* pr))
                {
                    pr->Update(f);
                    pr->Update_OutOfBoundary(f);
                    pr->Update_Raycast(f);
                }
            }
        }

        private void Update_Ball(Frame f)
        {
            if (isBallSpawned == false && f.Global->gamePlayState == GamePlayState.Play
                && InGameTime > FP._1)
                SpawnBall(f);

            if (ball != null)
            {
                if (f.Unsafe.TryGetPointer<BallRules>(ball, out var ballRules))
                {
                    var list_all = f.ResolveList(ListOfPlayers_All);
                    if (ballRules->TargetEntity == EntityRef.None && list_all.Count > 0)
                        ChangeBallTarget(f, EntityRef.None, FPVector3.Zero);

                    ballRules->Update_Target(f);
                    ballRules->Update_Movement(f);
                    ballRules->Update_Raycast(f);
                    ballRules->Update_Collision(f);
                }
            }

        }

        public void SetGameState(Frame f, GamePlayState state)
        {
            f.Global->gamePlayState = state;

            f.Events.GamePlayStateChanged(state);
        }

        public void SetInGamePlayMode(Frame f, int mode)
        {
            CurrentInGamePlayMode = (InGamePlayMode)mode;
        }

        public void SetData(Frame frame)
        {
            var gamePlaySettings = frame.FindAsset<GamePlaySettings>(frame.RuntimeConfig.GamePlaySettingsRef.Id);
            var ingameDataSettings = frame.FindAsset<InGameDataSettings>(frame.RuntimeConfig.InGameDataSettingsRef.Id);

            if (gamePlaySettings == null || ingameDataSettings == null)
                return;

            totalPlayers = gamePlaySettings.Total_PlayerNumber;
            realPlayers = gamePlaySettings.Real_PlayerNumber;
            aiPlayers = gamePlaySettings.AI_PlayerNumber;

            totalTeamCount = totalPlayers / gamePlaySettings.PlayerNumberEachTeam;
        }

        public void SetPlayerSoloRank(Frame f, EntityRef player)
        {
            var allList = f.ResolveList(ListOfPlayers_All);
            var rankingList_solo = f.ResolveList(ListOfPlayers_SoloRanking);

            for (int i = 0; i < rankingList_solo.Count; i++)
            {
                //이미 랭킹 데이터가 있으면 넘어가자...
                if (rankingList_solo[i].player == player)
                    return;
            }

            var info = new PlayerRankInfo();
            info.player = player;
            info.rank_solo = allList.Count - rankingList_solo.Count;

            rankingList_solo.Add(info);

            //Solo Mode / Team Mode 둘다 보내주고 있음
            f.Events.UpdatePlayerRank(CurrentInGamePlayMode, GetPlayerRankArray_Solo(f));
        }

        public void SetPlayerTeamRank(Frame f, EntityRef player, int currTeamCount, bool isFinalRank = false)
        {
            var allList = f.ResolveList(ListOfPlayers_All);
            var rankingList_team = f.ResolveList(ListOfPlayers_TeamRanking);

            if (f.Unsafe.TryGetPointer<PlayerRules>(player, out var playerRules)) { }
            if (playerRules == null)
                return;

            var teamID = playerRules->teamID;

            var teamList = new List<PlayerRules>();
            for (int i = 0; i < allList.Count; i++)
            {
                if (f.Unsafe.TryGetPointer<PlayerRules>(allList[i], out var pr))
                {
                    if (pr->teamID.Equals(teamID))
                        teamList.Add(*pr);
                }
            }

            if (isFinalRank == false)
            {
                foreach (var i in teamList)
                {
                    //마지막 랭킹(isFinalRank)이 아니면
                    //내 팀원중에서 한명이라도 살아 있으면 랭킹부여x
                    if (i.IsAlive)
                        return;
                }
            }
            
            for (int i = 0; i < rankingList_team.Count; i++)
            {
                if (rankingList_team[i].player.Equals(player))
                {
                    //이미 팀 랭킹 데이터가 있으면 넘어가자...
                    if (rankingList_team[i].rank_team > 0)
                        return;
                }
            }

            //새로운 데이터 넣자...
            var info = new PlayerRankInfo();
            info.player = player;
            if (isFinalRank == false)
                info.rank_team = currTeamCount + 1; //2등~나머지 등수
            else
                info.rank_team = 1; //1등

            rankingList_team.Add(info);

            //Team Mode 만 보내주고 있음
            f.Events.UpdatePlayerRank(CurrentInGamePlayMode, GetPlayerRankArray_Team(f));
        }

        public int GetPlayerRank(Frame f, EntityRef player)
        {
            int rank = -1;
            var list = f.ResolveList(ListOfPlayers_SoloRanking);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].player == player)                
                {
                    rank = list[i].rank_solo;
                    break;
                }
            }

            return rank;
        }

        private PlayerRankInfoArray GetPlayerRankArray_Solo(Frame f)
        {
            //List => Array

            var list = f.ResolveList(ListOfPlayers_SoloRanking);
            var rankInfo = new PlayerRankInfoArray();

            for (int i = 0; i < list.Count; i++)
            {
                if (i < rankInfo.PlayerRankInfoArr.Length)
                {
                    rankInfo.PlayerRankInfoArr[i].player = list[i].player;
                    rankInfo.PlayerRankInfoArr[i].rank_solo = list[i].rank_solo;
                }
            }

            return rankInfo;
        }

        private PlayerRankInfoArray GetPlayerRankArray_Team(Frame f)
        {
            //List => Array

            var list = f.ResolveList(ListOfPlayers_TeamRanking);
            var rankInfo = new PlayerRankInfoArray();

            for (int i = 0; i < list.Count; i++)
            {
                if (i < rankInfo.PlayerRankInfoArr.Length)
                {
                    rankInfo.PlayerRankInfoArr[i].player = list[i].player;
                    rankInfo.PlayerRankInfoArr[i].rank_team = list[i].rank_team;
                }
            }

            return rankInfo;
        }

        private InGamePlayStatisticsArray GetInGamePlayStatisticsArray(Frame f)
        {
            var list = f.ResolveList(ListOfPlayers_Statistics);
            var statistics = new InGamePlayStatisticsArray();

            for (int i = 0; i < list.Count; i++)
            {
                if (i < statistics.InGamePlayStatisticsDataArr.Length)
                {
                    statistics.InGamePlayStatisticsDataArr[i].activeSkillType = list[i].activeSkillType;
                    statistics.InGamePlayStatisticsDataArr[i].attackSuccessCount = list[i].attackSuccessCount;
                    statistics.InGamePlayStatisticsDataArr[i].dieCount = list[i].dieCount;
                    statistics.InGamePlayStatisticsDataArr[i].killCount = list[i].killCount;
                    statistics.InGamePlayStatisticsDataArr[i].mapID = list[i].mapID;
                    statistics.InGamePlayStatisticsDataArr[i].matchPlayMode = list[i].matchPlayMode;
                    statistics.InGamePlayStatisticsDataArr[i].passiveSkillType = list[i].passiveSkillType;
                    statistics.InGamePlayStatisticsDataArr[i].player = list[i].player;
                    statistics.InGamePlayStatisticsDataArr[i].skillActivationCount = list[i].skillActivationCount;
                }
            }

            return default;
        }


        public void SpawnPlayer(Frame frame, PlayerRef player)
        {
            //플레이어 소환
            var runtimeData = frame.GetPlayerData(player);
            var entity = frame.Create(runtimeData.CharacterPrototype);

            var playerLink = new PlayerLink()
            {
                PlayerRef = player,
            };
            frame.Add(entity, playerLink);

            var playerData = new Quantum.PlayerData()
            {
                TeamID = runtimeData.TeamID,

                Character_SkinID = runtimeData.Character_SkinID,
                Weapon_SkinID = runtimeData.Weapon_SkinID,
                Passive_SkillID = runtimeData.Passive_SkillID,
                Active_SkillID = runtimeData.Active_SkillID,

                MaxHealthPoint = runtimeData.MaxHealthPoint,
                MaxSpeed = runtimeData.MaxSpeed,
                AttackDamage = runtimeData.AttackDamage,
                AttackRange = runtimeData.AttackRange,
                AttackDuration = runtimeData.AttackDuration,

                Input_AttackCooltime = runtimeData.Input_AttackCooltime,
                Input_JumpCooltime = runtimeData.Input_JumpCooltime,
                Input_SkillCooltime = runtimeData.Input_SkillCooltime,
            };

            var list_all = frame.ResolveList(ListOfPlayers_All);
            if (list_all.Contains(entity) == false)
                list_all.Add(entity);

            if (frame.Unsafe.TryGetPointer<PlayerRules>(entity, out var playerRules))
            {
                playerRules->SetData(frame, entity, PlayerType.RealPlayer, playerData);
                playerRules->SetPosition(frame, entity);
            }

            frame.Events.PlayerSpawned(entity, runtimeData.PID, false, runtimeData.TeamID, runtimeData.Character_SkinID, runtimeData.Weapon_SkinID, runtimeData.Nickname, runtimeData.RankingPoint);
        }

        public void SpawnAIPlayer(Frame frame, PlayerRef player)
        {
            //AI 플레이어 소환

            if (isAIPlayerSpawned) //이미 소환 된적 있으면 제외하자... (AI Spawn Command를 모든 플레이어한테 받기 때문에 체크해줘야함)
                return;

            var gamePlaySettings = frame.FindAsset<GamePlaySettings>(frame.RuntimeConfig.GamePlaySettingsRef.Id);
            var ingameDataSettings = frame.FindAsset<InGameDataSettings>(frame.RuntimeConfig.InGameDataSettingsRef.Id);

            if (gamePlaySettings == null || ingameDataSettings == null)
                return;

            var totalNumber = gamePlaySettings.Total_PlayerNumber;
            var realNumber = gamePlaySettings.Real_PlayerNumber;
            var aiNumber = gamePlaySettings.AI_PlayerNumber;

            var skinList_char = ingameDataSettings.AI_AvailableCharacterSkinIDList;
            var skinList_weapon = ingameDataSettings.AI_AvailableWeaponSkinIDList;
            var skillList_passive = ingameDataSettings.AI_AvailablePassiveSkillList;
            var skillList_active = ingameDataSettings.AI_AvailableActiveSkillList;

            int maxTeamCount = totalNumber;
            if (CurrentInGamePlayMode == InGamePlayMode.SoloMode || CurrentInGamePlayMode == InGamePlayMode.PracticeMode)
                maxTeamCount = totalNumber;
            else if (CurrentInGamePlayMode == InGamePlayMode.TeamMode)
                maxTeamCount = totalNumber / gamePlaySettings.PlayerNumberEachTeam;

            int aiTeamID = (realNumber % maxTeamCount); //앞에 Real 플레이어들은 이미 teamId 가 지정됨. AI만 나머지 teamid지정해주자

            for (int i = 0; i < aiNumber; i++)
            {
                RngSession = new RNGSession(gamePlaySettings.RandomSeed + i);

                var data = frame.GetPlayerData(player);
                var entity = frame.Create(data.CharacterPrototype);

                var aiRules = new AIPlayerRules()
                {
                    SelfEntity = entity,
                };
                frame.Add(entity, aiRules);

                var randomRankingPoint = RngSession.Next(0, 5999);
                var randomNickname = "AI";
                if (frame.Unsafe.TryGetPointer<AIPlayerRules>(entity, out var aiPlayerRules))
                {
                    aiPlayerRules->SetAIDifficulty(frame, gamePlaySettings.RandomSeed + i, gamePlaySettings.MatchMakingGroup);
                    aiPlayerRules->SetData(frame, gamePlaySettings.RandomSeed + i);
                    aiPlayerRules->SetCounter(frame, i);
                    randomRankingPoint = aiPlayerRules->RankingPoint;
                    randomNickname = aiPlayerRules->GetRandomAINickname(gamePlaySettings.RandomSeed + i);
                }

                var playerData = new Quantum.PlayerData() { };

                //teamid 는 팀1번-> 팀2번 -> 팀3번 채우고 팀1번-> 팀2번 -> 팀3번 계속 반봅 하는 형식
                playerData.TeamID = aiTeamID;
                ++aiTeamID;
                if (aiTeamID >= maxTeamCount)
                {
                    aiTeamID = 0;
                }

                playerData.Character_SkinID = skinList_char[RngSession.Next(0, skinList_char.Count)];
                playerData.Weapon_SkinID = skinList_weapon[RngSession.Next(0, skinList_weapon.Count)];
                playerData.Passive_SkillID = skillList_passive[RngSession.Next(0, skillList_passive.Count)];
                playerData.Active_SkillID = skillList_active[RngSession.Next(0, skillList_active.Count)];

                playerData.MaxHealthPoint = ingameDataSettings.aiDefaultData.PlayerDafaultData.MaxHealthPoint + ingameDataSettings.aiDefaultData.PlayerAdditionalStatsData.MaxHealthPoint;
                playerData.MaxSpeed = ingameDataSettings.aiDefaultData.PlayerDafaultData.MaxSpeed + ingameDataSettings.aiDefaultData.PlayerAdditionalStatsData.MaxSpeed;
                playerData.AttackDamage = ingameDataSettings.aiDefaultData.PlayerDafaultData.AttackDamage + ingameDataSettings.aiDefaultData.PlayerAdditionalStatsData.AttackDamage;
                playerData.AttackRange = ingameDataSettings.aiDefaultData.PlayerDafaultData.AttackRange + ingameDataSettings.aiDefaultData.PlayerAdditionalStatsData.AttackRange;
                playerData.AttackDuration = ingameDataSettings.aiDefaultData.PlayerDafaultData.AttackDuration + ingameDataSettings.aiDefaultData.PlayerAdditionalStatsData.AttackDuration;

                playerData.Input_AttackCooltime = ingameDataSettings.aiDefaultData.PlayerDafaultData.Input_AttackCooltime + ingameDataSettings.aiDefaultData.PlayerAdditionalStatsData.Input_AttackCooltime;
                playerData.Input_JumpCooltime = ingameDataSettings.aiDefaultData.PlayerDafaultData.Input_JumpCooltime + ingameDataSettings.aiDefaultData.PlayerAdditionalStatsData.Input_JumpCooltime;
                playerData.Input_SkillCooltime = GetSkillCooltime_AI(frame, playerData.Active_SkillID);

                var list_all = frame.ResolveList(ListOfPlayers_All);
                if (list_all.Contains(entity) == false)
                    list_all.Add(entity);

                if (frame.Unsafe.TryGetPointer<PlayerRules>(entity, out var playerRules))
                {
                    playerRules->SetData(frame, entity, PlayerType.AIPlayer, playerData);
                    playerRules->SetPosition(frame, entity);
                }

                frame.Events.PlayerSpawned(entity, "-1", true, playerData.TeamID, playerData.Character_SkinID, playerData.Weapon_SkinID, randomNickname, randomRankingPoint);
            }

            isAIPlayerSpawned = true;
        }

        public void SpawnBall(Frame f)
        {
            //공 소환

            var gamePlaySettings = f.FindAsset<GamePlaySettings>(f.RuntimeConfig.GamePlaySettingsRef.Id);
            var ingameDataSettings = f.FindAsset<InGameDataSettings>(f.RuntimeConfig.InGameDataSettingsRef.Id);
            var dobj = f.FindAsset<DynamicCreateObject>(f.RuntimeConfig.DynamicCreateObjectRef.Id);

            if (gamePlaySettings == null || ingameDataSettings == null || dobj == null)
                return;

            var entity = f.Create(dobj.BallPrototype);
            ball = entity;
            if (f.Unsafe.TryGetPointer<Transform3D>(entity, out var transform))
            {
                transform->Position = new Photon.Deterministic.FPVector3(0, 3, 0);
            }
            if (f.Unsafe.TryGetPointer<BallRules>(entity, out var ballRules))
            {
                ballRules->SelfEntity = entity;

                var data = new BallData()
                {
                    BallStartSpeed = ingameDataSettings.ballDefaultData.BallStartSpeed,
                    BallMinSpeed = ingameDataSettings.ballDefaultData.BallMinSpeed,
                    BallIncreaseSpeed = ingameDataSettings.ballDefaultData.BallIncreaseSpeed,
                    BallMaxSpeed = ingameDataSettings.ballDefaultData.BallMaxSpeed,

                    BallRotationSpeed = ingameDataSettings.ballDefaultData.BallRotationSpeed,
                    BallRotationMinSpeed = ingameDataSettings.ballDefaultData.BallRotationMinSpeed,
                    BallRotationIncreaseSpeed = ingameDataSettings.ballDefaultData.BallRotationIncreaseSpeed,
                    BallRotationMaxSpeed = ingameDataSettings.ballDefaultData.BallRotationMaxSpeed,

                    DefaultAttackDamage = ingameDataSettings.ballDefaultData.DefaultAttackDamage,

                    BallCollisionCooltime = ingameDataSettings.ballDefaultData.BallCollisionCooltime,
                };

                ballRules->SetData(f, data);
            }


            isBallSpawned = true;

            f.Events.BallSpawned(entity);
        }

        public void ChangeBallTarget(Frame f, EntityRef requestEntity, FPVector3 direction)
        {
            if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager))
            {
                var list = f.ResolveList(gameManager->ListOfPlayers_All);

                if (f.Unsafe.TryGetPointer<BallRules>(ball, out var ballRules))
                {
                    if (requestEntity != EntityRef.None)
                    {
                        var newTarget = GetBallTarget(f, GetBallTargetType.ClosestPlusClosestToFront, requestEntity);
                        if (newTarget != EntityRef.None)
                            ballRules->SetTarget(f, newTarget, direction);
                    }
                    else
                    {
                        //최초로 공 생성시
                        var newTarget = GetBallTarget(f, GetBallTargetType.Random, EntityRef.None);
                        if (newTarget != EntityRef.None)
                            ballRules->SetTarget(f, newTarget, direction);
                    }

                    f.Events.BallTargetChanged(ballRules->TargetEntity, ballRules->PreviousEntity);
                }
            }
        }

        public EntityRef GetBallTarget(Frame f, GetBallTargetType type, EntityRef requestEntity)
        {
            EntityRef target = EntityRef.None;

            if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager))
            {
                var list = f.ResolveList(gameManager->ListOfPlayers_All);

                if(list.Count <= 0)
                    return target;

                switch (type)
                {
                    case GetBallTargetType.FirstInList:
                        {
                            target = list.FirstOrDefault(x => x != EntityRef.None);
                        }
                        break;

                    case GetBallTargetType.Random: //랜덤 아무나
                        {
                            int randomIndex = RngSession.Next(0, list.Count);

                            for (int i = 0; i < list.Count; i++)
                            {
                                if (f.Unsafe.TryGetPointer<PlayerRules>(list[randomIndex], out var playerRules)
                                    && f.Unsafe.TryGetPointer<PlayerRules>(requestEntity, out var playerRules_requester))
                                {
                                    bool isFail = false;

                                    if (playerRules->SelfEntity.Equals(requestEntity))
                                        isFail = true;

                                    if (playerRules->isDead)
                                        isFail = true;

                                    //팀모드에선 같은 팀Target 제외
                                    if (gameManager->CurrentInGamePlayMode == InGamePlayMode.TeamMode
                                        && playerRules->teamID.Equals(playerRules_requester->teamID))
                                        isFail = true;

                                    if (isFail)
                                    {
                                        // 조건에 부합하지 않으면 index 바꿔주자
                                        if (randomIndex >= list.Count - 1)
                                            randomIndex = 0;
                                        else
                                            ++randomIndex;

                                        continue;
                                    }
                                }

                                if (list[randomIndex] != EntityRef.None)
                                {
                                    target = list[randomIndex];
                                    break;
                                }
                            }
                        }
                        break;

                    case GetBallTargetType.Closest: //가장 가까운 플레이어
                        {
                            FP minDist = FP.MaxValue;
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (f.Unsafe.TryGetPointer<PlayerRules>(list[i], out var playerRules)
                                    && f.Unsafe.TryGetPointer<PlayerRules>(requestEntity, out var playerRules_requester))
                                {
                                    if (playerRules->SelfEntity.Equals(requestEntity))
                                        continue;

                                    if (playerRules->isDead)
                                        continue;

                                    //팀모드에선 같은 팀Target 제외
                                    if (gameManager->CurrentInGamePlayMode == InGamePlayMode.TeamMode
                                        && playerRules->teamID.Equals(playerRules_requester->teamID))
                                        continue;

                                    var dist = FPVector3.Distance(playerRules->GetPlayerPosition(f), playerRules_requester->GetPlayerPosition(f));
                                    if (dist < minDist)
                                    {
                                        target = list[i];
                                        minDist = dist;
                                    }
                                }
                            }
                        }
                        break;

                    case GetBallTargetType.ClosestOrRandom: //일정거리 안에 있으면 가까운 놈 Target 나머지는 랜덤
                        {
                            if (f.Unsafe.TryGetPointer<BallRules>(gameManager->ball, out BallRules* br))
                            {
                                var closestTarget = GetBallTarget(f, GetBallTargetType.Closest, requestEntity);
                                if (requestEntity != EntityRef.None && closestTarget != EntityRef.None)
                                {
                                    if (f.Unsafe.TryGetPointer<PlayerRules>(closestTarget, out PlayerRules* closestPR)
                                        && f.Unsafe.TryGetPointer<PlayerRules>(requestEntity, out PlayerRules* requestPR))
                                    {
                                        var dist = FPVector3.Distance(closestPR->GetPlayerPosition(f), requestPR->GetPlayerPosition(f));
                                        if (dist < 15)
                                        {
                                            return closestTarget;
                                        }
                                    }

                                }
                            }

                            return GetBallTarget(f, GetBallTargetType.Random, requestEntity);
                        }

                    case GetBallTargetType.ClosestToFront: //시야각이 가장 좁은, 즉 정면에 가장 가까운 상대
                        {
                            FP minAngle = FP.MaxValue;
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (f.Unsafe.TryGetPointer<PlayerRules>(list[i], out var playerRules)
                                    && f.Unsafe.TryGetPointer<Transform3D>(list[i], out var playerTrans)
                                    && f.Unsafe.TryGetPointer<PlayerRules>(requestEntity, out var playerRules_requester)
                                    && f.Unsafe.TryGetPointer<Transform3D>(requestEntity, out var trans_requester))
                                {
                                    if (playerRules->SelfEntity.Equals(requestEntity))
                                        continue;

                                    if (playerRules->isDead)
                                        continue;

                                    //팀모드에선 같은 팀Target 제외
                                    if (gameManager->CurrentInGamePlayMode == InGamePlayMode.TeamMode
                                        && playerRules->teamID.Equals(playerRules_requester->teamID))
                                        continue;

                                    FPVector3 dir = (playerTrans->Position - trans_requester->Position).Normalized;
                                    dir.Y = 0; //높이차이는 무시
                                    var angle = FPVector3.Angle(trans_requester->Forward, dir);
                                    if (angle < minAngle)
                                    {
                                        minAngle = angle;
                                        target = list[i];
                                    }
                                }
                            }
                        }
                        break;

                    case GetBallTargetType.ClosestPlusClosestToFront: //일정 거리내에는 가까운 플레이어 && 시야각 좁은 플레이어
                        {
                            if (f.Unsafe.TryGetPointer<BallRules>(gameManager->ball, out BallRules* br))
                            {
                                var closestTarget = GetBallTarget(f, GetBallTargetType.Closest, requestEntity);
                                if (requestEntity != EntityRef.None && closestTarget != EntityRef.None)
                                {
                                    if (f.Unsafe.TryGetPointer<PlayerRules>(closestTarget, out PlayerRules* closestPR)
                                           && f.Unsafe.TryGetPointer<Transform3D>(closestTarget, out var closestTargetTrans)
                                        && f.Unsafe.TryGetPointer<PlayerRules>(requestEntity, out PlayerRules* requestPR)
                                        && f.Unsafe.TryGetPointer<Transform3D>(requestEntity, out var requestTrans))
                                    {
                                        FPVector3 dir = (closestTargetTrans->Position - requestTrans->Position).Normalized;
                                        dir.Y = 0; //높이차이는 무시
                                        var angle = FPVector3.Angle(requestTrans->Forward, dir);
                                        var dist = FPVector3.Distance(closestPR->GetPlayerPosition(f), requestPR->GetPlayerPosition(f));
                                        if (dist < 15 && angle < 120) //일정 거리(15) 내에 + 120도 각도 안에 들어왔을 경우
                                        {
                                            return closestTarget;
                                        }
                                    }

                                }
                            }

                            //나머지는 시야각 기준으로 타켓 잡자
                            return GetBallTarget(f, GetBallTargetType.ClosestToFront, requestEntity);
                        }
                }
            }
                
            return target;
        }

        private int GetSkillCooltime_AI(Frame frame, int activeSkillID)
        {
            //AI의 경우 전달은 additionalReferenceData 토대로 데이터 세팅하자

            int cooltime = 0;
            var ingameDataSettings = frame.FindAsset<InGameDataSettings>(frame.RuntimeConfig.InGameDataSettingsRef.Id);

            if (ingameDataSettings != null)
            {
                cooltime = ingameDataSettings.playerDefaultData.Input_SkillCooltime;
                switch ((PlayerActiveSkill)activeSkillID)
                {
                    case PlayerActiveSkill.None:
                    default:
                        cooltime = ingameDataSettings.playerDefaultData.Input_SkillCooltime;
                        break;

                    case PlayerActiveSkill.Dash:
                        cooltime = ingameDataSettings.aiDefaultData.CoolTime_Dash;
                        break;
                    case PlayerActiveSkill.FreezeBall:
                        cooltime = ingameDataSettings.aiDefaultData.CoolTime_FreezeBall;
                        break;
                    case PlayerActiveSkill.FastBall:
                        cooltime = ingameDataSettings.aiDefaultData.CoolTime_FastBall;
                        break;
                    case PlayerActiveSkill.Shield:
                        cooltime = ingameDataSettings.aiDefaultData.CoolTime_Shield;
                        break;
                    case PlayerActiveSkill.TakeBallTarget:
                        cooltime = ingameDataSettings.aiDefaultData.CoolTime_TakeBallTarget;
                        break;
                    case PlayerActiveSkill.BlindZone:
                        cooltime = ingameDataSettings.aiDefaultData.CoolTime_BlindZone;
                        break;
                    case PlayerActiveSkill.ChangeBallTarget:
                        cooltime = ingameDataSettings.aiDefaultData.CoolTime_ChangeBallTarget;
                        break;
                    case PlayerActiveSkill.CurveBall:
                        cooltime = ingameDataSettings.aiDefaultData.CoolTime_CurveBall;
                        break;
                    case PlayerActiveSkill.SkyRocketBall:
                        cooltime = ingameDataSettings.aiDefaultData.CoolTime_SkyRocketBall;
                        break;
                }
            }

            return cooltime;
        }
    }
}
