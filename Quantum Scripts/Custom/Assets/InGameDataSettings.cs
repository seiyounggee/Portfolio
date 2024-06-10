using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum
{
    public unsafe partial class InGameDataSettings
    {
        public PlayerDefaultData playerDefaultData;
        public BallDefaultData ballDefaultData;
        public AdditionalReferenceData additionalReferenceData; //client->Quantum 전당시킬 RefData들

        public AIDefaultData aiDefaultData;
        public List<int> AI_AvailableCharacterSkinIDList = new List<int>();
        public List<int> AI_AvailableWeaponSkinIDList = new List<int>();
        public List<int> AI_AvailablePassiveSkillList = new List<int>();
        public List<int> AI_AvailableActiveSkillList = new List<int>();
    }
}
