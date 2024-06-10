using Photon.Deterministic;

namespace Quantum
{
    public unsafe class CommandCheat : DeterministicCommand
    {
        public int cheatType;
        public EntityRef targetEntity;
        public int param;

        public override void Serialize(Photon.Deterministic.BitStream stream)
        {
            stream.Serialize(ref cheatType);
            stream.Serialize(ref targetEntity);
            stream.Serialize(ref param);
        }

        public void Execute(Frame f)
        {
            switch ((CheatType)cheatType)
            {
                case CheatType.None:
                    { 
                    
                    }
                    break;

                case CheatType.KillSelf:
                    {
                        if (f.Unsafe.TryGetPointer<PlayerRules>(targetEntity, out var pr))
                        {
                            pr->SetDead_Cheat(f);
                        }
                    }
                    break;

                case CheatType.InvincibleSelf:
                    {
                        if (f.Unsafe.TryGetPointer<PlayerRules>(targetEntity, out var pr))
                        {
                            pr->SetInvincible_Cheat(f);
                        }
                    }
                    break;

                case CheatType.AutoAttack:
                    {
                        if (f.Unsafe.TryGetPointer<PlayerRules>(targetEntity, out var pr))
                        {
                            pr->SetAutoAttack_Cheat(f);
                        }
                    }
                    break;

                case CheatType.HealHP:
                    {
                        if (f.Unsafe.TryGetPointer<PlayerRules>(targetEntity, out var pr))
                        {
                            pr->SetHealHp_Cheat(f, param);
                        }
                    }
                    break;

                case CheatType.BallDamageZero:
                    {
                        if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gm))
                        {
                            if (f.Unsafe.TryGetPointer<BallRules>(gm->ball, out var br))
                            {
                                br->SetAttackDamage_Cheat(f, param);
                            }
                        }
                    }
                    break;

                case CheatType.BallMaxSpeed:
                    {
                        if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gm))
                        {
                            if (f.Unsafe.TryGetPointer<BallRules>(gm->ball, out var br))
                            {
                                br->SetMaxSpeed_Cheat(f);
                            }
                        }
                    }
                    break;

                case CheatType.AllPlayerHpToOne:
                    {
                        if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gm))
                        {
                            var allList = f.ResolveList(gm->ListOfPlayers_All);
                            for (int i = 0; i < allList.Count; i++)
                            {
                                if (f.Unsafe.TryGetPointer<PlayerRules>(allList[i], out var playerRules))
                                {
                                    playerRules->SetHp_Cheat(f, 1);
                                }
                            }
                        }
                    }
                    break;

                case CheatType.KillAllButMe:
                    {
                        if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gm))
                        {
                            var allList = f.ResolveList(gm->ListOfPlayers_All);
                            for (int i = 0; i < allList.Count; i++)
                            {
                                if (f.Unsafe.TryGetPointer<PlayerRules>(allList[i], out var playerRules))
                                {
                                    if (playerRules->SelfEntity.Equals(targetEntity))
                                        continue;

                                    playerRules->SetDead_Cheat(f);
                                }
                            }
                        }
                    }
                    break;
            }
        }
    }
}
