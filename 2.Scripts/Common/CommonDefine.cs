using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CommonDefine
{
    public const string ProjectName = "DOLLY TR";

    public const string OutGameScene = "OutGameScene";
    public const string InGameScene = "InGameScene";
    public const string Track_Sample01 = "Track_Sample01";
    public const string Track_Sample02 = "Track_Sample02";
    public const string Track_Proto_A1_YS = "Track_Proto_A1_YS";

    public static bool IsFirstLogin = false;

    #region PHOTON PARAMETERS

    public static int GAME_MAP_ID
    {
        get
        {
            //지정해준게 없으면 DB에서 지정한 ID 값으로...
            if (mapID == -1)
            {
                if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.gameData != null)
                    return DataManager.Instance.basicData.gameData.GAME_MAP_ID;
                else
                    return 0;
            }
            else
                return mapID;
        }
        set
        {
            mapID = value;
        }
    }
    private static int mapID = -1;

    public static float GAME_TIME_WAITING_BEFORE_START
    {
        get
        {
            if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.gameData != null)
                return DataManager.Instance.basicData.gameData.GAME_TIME_WAITING_BEFORE_START;
            else
                return 10f;
        }
    }

    #endregion

    //UI
    public static float GAME_INPUT_SWIPE_MIN_LENGHT
    {
        get
        {
            if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.gameData != null)
                return DataManager.Instance.basicData.gameData.GAME_INPUT_SWIPE_MIN_LENGHT;
            else
                return 0.15f;
        }
    }

    public static float GAME_INPUT_DELAY
    {
        get
        {
            if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.gameData != null)
                return DataManager.Instance.basicData.gameData.GAME_INPUT_DELAY;
            else
                return 0.3f;
        }
    }

    public static bool GAME_INPUT_USE_NETWORK_RESPONSE
    {
        get
        {
            if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.gameData != null)
                return DataManager.Instance.basicData.gameData.GAME_INPUT_USE_NETWORK_RESPONSE;
            else
                return false;
        }
    }

    public static bool GAME_FOODTRUCK_ACTIVATE
    {
        get
        {
            if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.gameData != null)
                return DataManager.Instance.basicData.gameData.GAME_FOODTRUCK_ACTIVATE;
            else
                return false;
        }
    }

    public static bool GAME_MINIMAP_ACTIVATE
    {
        get
        {
            if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.gameData != null)
                return DataManager.Instance.basicData.gameData.GAME_MINIMAP_ACTIVATE;
            else
                return false;
        }
    }


    public static float GAME_AUTO_LEAVE_ROOM_TIME
    {
        get
        {
            if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.gameData != null)
                return DataManager.Instance.basicData.gameData.GAME_AUTO_LEAVE_ROOM_TIME;
            else
                return 100f;
        }
    }

    public static float PLAYER_INDICATOR_CHECK_BEHIND_DIST
    {
        get
        {
            if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.playerData != null)
                return DataManager.Instance.basicData.playerData.PLAYER_INDICATOR_CHECK_BEHIND_DIST;
            else
                return 15f;
        }
    }

    //InGame

#if CHEAT
    public static bool isForcePlaySolo = false;
#endif

    public static int TOTAL_LANE_NUMBER = 5; //레인 개수
    public static int SPAWN_INDEX_TEMP = 5; //index 0~2까지 spawn 하자...
    public static int START_COUNTDOWN_TIME //시작전 countdown
    {
        get
        {
            if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.gameData != null)
                return DataManager.Instance.basicData.gameData.GAME_START_COUNTDOWN_TIME;
            else
                return 3;
        }
    }
    public static int END_COUNTDOWN_TIME//1위 통과 후 countdown
    {
        get
        {
            if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.gameData != null)
                return DataManager.Instance.basicData.gameData.GAME_END_COUNTDOWN_TIME;
            else
                return 10;
        }
    }

    public const float DEFAULT_DESTROY_AFTERTIME = 3f;
    public const float DEFAULT_SETACTIVE_FALSE_AFTERTIME = 3f;



    public static bool SetMapID(int id)
    {
        //if id == -1 Random....!!!

        bool isSet = false;
        var mapData = DataManager.Instance.mapData;
        var basicData = DataManager.Instance.basicData;

        var list = mapData.MapDataList.ToList();

        if (mapData != null && basicData != null && list != null)
        {
            if (id == -1)
            {
                //랜덤
                id = UnityEngine.Random.Range(0, list.Count);
            }

            if (list.Exists(x => x.id.Equals(id)))
            {
                mapID = id;
                isSet = true;
            }
            else
            {
                Debug.Log("<color=red>Not Valid MapID....!!!!!</color>");
            }
        }

        return isSet;
    }

    public static int GetMaxPlayer()
    {
        var mapData = DataManager.Instance.mapData;
        var basicData = DataManager.Instance.basicData;

        int maxPlayer = 0;
        if (mapData != null && basicData != null)
        {
            var list = mapData.MapDataList.ToList();
            var map = list.Find(x => x.id.Equals(GAME_MAP_ID));
            if (map != null)
            {
                maxPlayer = (int)map.MAX_PLAYER;
            }
        }

        return maxPlayer;
    }

    public static byte GetMinPlayer()
    {
        var mapData = DataManager.Instance.mapData;
        var basicData = DataManager.Instance.basicData;

        byte minPlayer = 0;
        if (mapData != null && basicData != null)
        {
            var list = mapData.MapDataList.ToList();
            var map = list.Find(x => x.id.Equals(GAME_MAP_ID));
            if (map != null)
            {
                minPlayer = (byte)map.MIN_PLAYER;
            }
        }

        return minPlayer;
    }

    public static int GetFinalLapCount()
    {
        var mapData = DataManager.Instance.mapData;
        var basicData = DataManager.Instance.basicData;

        int lapCount = 0;
        if (mapData != null && basicData != null)
        {
            var list = mapData.MapDataList.ToList();
            var map = list.Find(x => x.id.Equals(GAME_MAP_ID));
            if (map != null)
            {
                if (map.LAP_COUNT != -1)
                    lapCount = map.LAP_COUNT;
                else
                    return -1;
            }
            else
                return -1;
        }
        else
            return -1;

        return lapCount;
    }




    //GameObject Name



    #region TAG
    public const string TAG_NetworkPlayer = "TAG_NetworkPlayer";
    public const string TAG_NetworkPlayer_CollisionChecker = "TAG_NetworkPlayer_CollisionChecker";
    public const string TAG_NetworkPlayer_TriggerChecker = "TAG_NetworkPlayer_TriggerChecker";
    public const string TAG_MainCamera = "TAG_MainCamera";

    public const string TAG_ROAD_Normal = "TAG_ROAD_Normal";
    public const string TAG_OutOfBound = "TAG_OutOfBound";

    #endregion


    //Layer Name
    public const string LayerName_Default = "Default";
    public const string LayerName_Hidden = "Hidden";


    public enum Phase
    { 
        None = -1,
        Initialize,
        Lobby,
        InGameReady,
        InGame,
        InGameResult
    }

    public static int FIXED_UPDATE_FRAMERATE_PER_SECOND = 50;

    public static string GetGoNameWithHeader(string name)
    {
        return "[" + name + "] ";
    }

    public static float DragMinLength()
    {
        return Screen.width * 0.1f; //최소 화면의 10% 이상 drag
    }
    public static float DragMaxLength()
    {
        return Screen.width * 0.5f; //최대 화면의 50%까지만 drag가능
    }

    public static float ScreenSwipeLength(float dist)
    {
        //화면기준으로 계산을 해주는 이유는 디스플레이 크기와 관계없이 길이를 측정해야하기 때문
        return (dist / (float)Screen.width); //drag의 크기 값
    }


    public enum ControlType { TouchSwipe = 0 , VirtualPad = 1 , MaxCount}
    public static ControlType CurrentControlType()
    {
        if (DataManager.Instance.userData != null)
        {
            if (DataManager.Instance.userData.IngameControlType < (int)ControlType.MaxCount)
                return (ControlType)DataManager.Instance.userData.IngameControlType;
        }


        return ControlType.TouchSwipe;
    }
}