using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum
{
    public unsafe partial class GamePlaySettings
    {
        public int InGamePlayMode;
        public int MatchMakingGroup;
        public int Total_PlayerNumber;
        public int Real_PlayerNumber;
        public int AI_PlayerNumber;

        public int PlayerNumberEachTeam; //한 팀당 플레이어 수

        public int RandomSeed;
    }
}
