using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class RankingManager
{
    [Serializable]
    public class RankData
    {
        #region Ranking Data List
        //실제 랭킹 데이터 관련
        [SerializeField] public List<RankingUserInfo> TotalRP_RankingUserList = new List<RankingUserInfo>();

        [Serializable]
        public class RankingUserInfo
        {
            public string PID;
            public string Nickname;
            public string CountryCode;
            public int RankingPoint;

            public int PassiveSkillID;
            public int ActiveSkillID;
            public int TotalMatchCount; //MatchCount_SoloMode + MatchCount_TeamMode

            public string MatchDateTime;
        }
        #endregion
    }
}
