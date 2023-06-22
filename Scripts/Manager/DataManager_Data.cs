using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PNIX.ReferenceTable;
using UnityEngine.Networking.Types;

public partial class DataManager
{
    public static int GameMapID
    {
        get
        {
            //지정해준게 없으면 DB에서 지정한 ID 값으로...
            if (gameMapID == -1)
            {
                int mapID = DataManager.Instance.GetCommonConfig<int>("defaultMapID");
                if (mapID > 0)
                    return mapID;
                else
                    return 1;
            }
            else
                return gameMapID;
        }
    }

    private static int gameMapID = -1;

    public bool SetMapID(int id)
    {
        bool isSet = false;

        var mapRef = CReferenceManager.Instance.FindRefMap(id);

        if (mapRef != null)
        {
            isSet = true;
            gameMapID = mapRef.ID;
        }
        else
        {
            int mapID = DataManager.Instance.GetCommonConfig<int>("defaultMapID");
            if (mapID != 0)
            {
                isSet = true;
                gameMapID = mapID;
            }
        }


        return isSet;
    }

    public List<int> GetListOfAICarIDs()
    {
        //추후..... 서버에서 처리
        List<int> list = new List<int>();
        return null;
    }

    public CRefCar GetAICar(int carID)
    {
        return GetRandomCar();
    }

    public static float GAME_TIME_WAITING_BEFORE_START
    {
        get
        {
            //임시...
            return 5f;
        }
    }
    public static float GAME_INPUT_SWIPE_MIN_LENGHT
    {
        get
        {
            return DataManager.Instance.GetGameConfig<float>("gameInputSwipeMinLength");
        }
    }

    public static float GAME_INPUT_DELAY
    {
        get
        {
            return DataManager.Instance.GetGameConfig<float>("gameInputDelay");
        }
    }

    public static float GAME_INPUT_RESET_COOLTIME
    {
        get
        {
            var resetTime = DataManager.Instance.GetGameConfig<float>("gameInputResetCooltime");
            if (resetTime > GAME_INPUT_DELAY)
                return resetTime;
            else
                return GAME_INPUT_DELAY;
        }
    }

    public static float GAME_INPUT_RESET_STAYDIST_RADIUS
    {
        get
        {
            return DataManager.Instance.GetGameConfig<float>("gameInputResetStayDistRadius");
        }
    }

    public static bool GAME_INPUT_USE_CONTINUOUSINPUT
    {
        get
        {
            var input = DataManager.Instance.GetGameConfig<int>("inputUseContinuousInput");

            if (input == 1)
                return true;
            else
                return false;
        }
    }

    public static float GAME_AUTO_LEAVE_ROOM_TIME
    {
        get
        {
            //추후 서버에서 관리...?
            return 100f;
        }
    }

    public static float PLAYER_INDICATOR_CHECK_BEHIND_DIST
    {
        get
        {
            return DataManager.Instance.GetGameConfig<float>("playerIndicatorCheckBehindDist");
        }
    }

    public static int END_COUNTDOWN_TIME//1위 통과 후 countdown
    {
        get
        {
            //추후 서버에서 관리...?
            return 10;
        }
    }


    public static int GetMaxPlayer() //Data 기반으로 찾기...
    {
        var refs = CReferenceManager.Instance.GetRefMaps();

        if (refs.ContainsKey(GameMapID))
        {
            return refs[GameMapID].playerlimit;
        }
        else
        {
            Debug.Log("<color=red>Erorr...!map ref not found! gameMapID:" + gameMapID  + "</color>");
        }

        return 0;
    }

    public int GetSessionRealPlayerCount() //현재 session 기반으로 찾기....
    {
        if (PhotonNetworkManager.Instance.MySessionInfo != null && PhotonNetworkManager.Instance.MySessionInfo.Properties != null)
        {
            if (PhotonNetworkManager.Instance.MySessionInfo.Properties.TryGetValue(PhotonNetworkManager.SESSION_PROPERTY_REALPLAYERCOUNT, out var realPlayers) && realPlayers.IsInt)
            {
                return realPlayers;
            }
        }

        return GetMaxPlayer();
    }

    public int GetSessionAIPlayerCount() //현재 session 기반으로 찾기....
    {
        if (PhotonNetworkManager.Instance.MySessionInfo != null && PhotonNetworkManager.Instance.MySessionInfo.Properties != null)
        {
            if (PhotonNetworkManager.Instance.MySessionInfo.Properties.TryGetValue(PhotonNetworkManager.SESSION_PROPERTY_AIPLAYERCOUNT, out var aiPlayers) && aiPlayers.IsInt)
            {
                return aiPlayers;
            }
        }

        return GetMaxPlayer() - GetSessionRealPlayerCount();
    }

    public int GetSessionTotalCount()
    {
        return GetSessionRealPlayerCount() + GetSessionAIPlayerCount();
    }


    public static int FinalLapCount
    {
        get
        {
            return finalLapCount;
        }

    }
    private static int finalLapCount = -1;

    public void SetFinalLapCount()
    {
        var refs = CReferenceManager.Instance.GetRefMaps();

        if (refs.ContainsKey(gameMapID))
        {
            finalLapCount = refs[gameMapID].lapCount;
        }
    }

    public CRefMap GetMapRef(int id)
    {
        var refs = CReferenceManager.Instance.GetRefMaps();
        if (refs.ContainsKey(id))
            return refs[id];
        else
            return null;
    }

    public List<string> GetAllMapAssetName()
    {
        var list = new List<string>();
        var refs = CReferenceManager.Instance.GetRefMaps();

        foreach(var refMap in refs) 
        {
            if (refMap.Value != null)
            {
                list.Add(refMap.Value.assetName);
            }
        }

        return list;
    }

    public CRefCar GetRandomCar()
    {
        var refs = CReferenceManager.Instance.GetRefCars();

        System.Random rand = new System.Random();
        return refs.ElementAt(rand.Next(0, refs.Count)).Value;
    }

    public CRefCharacter GetRandomCharacter()
    {
        var refs = CReferenceManager.Instance.GetRefCharacters();

        System.Random rand = new System.Random();
        return refs.ElementAt(rand.Next(0, refs.Count)).Value;
    }


    public CRefCar GetCarRef(int id)
    {
        var refs = CReferenceManager.Instance.GetRefCars();

        if (refs.ContainsKey(id))
            return refs[id];
        else
            return null;
    }

    public CRefCar GetCarRefByIndex(int index)
    {
        var refs = CReferenceManager.Instance.GetRefCars();

        if (index < refs.Count && refs.Values.ElementAt(index) != null)
            return refs.Values.ElementAt(index);
        else
            return null;
    }

    public int GetCarIndex(int id)
    {
        var refs = CReferenceManager.Instance.GetRefCars();

        int index = 0;
        foreach (var r in refs)
        {
            if (r.Value != null && r.Value.ID.Equals(id))
                return index;

            ++index;
        }

        return -1;
    }

    public List<string> GetAllCarAssetName()
    {
        var list = new List<string>();
        var refs = CReferenceManager.Instance.GetRefCars();

        foreach (var refCar in refs)
        {
            if (refCar.Value != null)
            {
                list.Add(refCar.Value.prefabID);
            }
        }

        return list;
    }

    public CRefCarStatGroup GetCarStat(int id, int lv)
    {        
        var carRef = GetCarRef(id);

        if (carRef != null)
        {
            int groupID = carRef.carStatGroupID;
            var refStatGroup = CReferenceManager.Instance.FindRefCarStatGroup(groupID);

            if (refStatGroup != null)
            {
                foreach (var stat in refStatGroup)
                {
                    if (lv.Equals((int)(stat.level)))
                    {
                        return stat;
                    }
                }
            }
        }

        return null;
    }

    public int GetTotalCarCount()
    {
        var refs = CReferenceManager.Instance.GetRefCars();
        return refs.Count;
    }

    public CRefCharacter GetCharacterRef(int id)
    {
        var refs = CReferenceManager.Instance.GetRefCharacters();

        if (refs.ContainsKey(id))
            return refs[id];
        else
            return null;
    }

    public CRefCharacter GetCharacterRefByIndex(int index)
    {
        var refs = CReferenceManager.Instance.GetRefCharacters();

        if (index < refs.Count && refs.Values.ElementAt(index) != null)
            return refs.Values.ElementAt(index);
        else
            return null;
    }

    public int GetCharacterIndex(int id)
    {
        var refs = CReferenceManager.Instance.GetRefCharacters();

        int index = 0;
        foreach (var r in refs)
        {
            if (r.Value != null && r.Value.ID.Equals(id))
                return index;

            ++index;
        }

        return -1;
    }

    public int GetTotalCharacterCount()
    {
        var refs = CReferenceManager.Instance.GetRefCharacters();
        return refs.Count;
    }

    public List<string> GetAllCharacterAssetName()
    {
        var list = new List<string>();
        var refs = CReferenceManager.Instance.GetRefCharacters();

        foreach (var refChar in refs)
        {
            if (refChar.Value != null)
            {
                list.Add(refChar.Value.prefabID);
            }
        }

        return list;
    }

    public CRefAIType GetAIType(int id)
    {
        var refs = CReferenceManager.Instance.FindRefAIType(id);

        foreach (var refAI in refs)
        { 
            if(refAI.ID.Equals(id))
                return refAI;
        }

        return null;
    }

    public short GetItemSortID(DTR.Shared.EItemType type)
    {
        short id = 0;
        switch (type)
        {
            case DTR.Shared.EItemType.None:
                break;
            case DTR.Shared.EItemType.Coin:
                id = 1;
                break;
            case DTR.Shared.EItemType.FreeGem:
                id = 2;
                break;
            case DTR.Shared.EItemType.PaidGem:
                id = 3;
                break;
            case DTR.Shared.EItemType.UnidentifiedGear:
                id = 4;
                break;
            case DTR.Shared.EItemType.RacingPoint:
                id = 5;
                break;
            case DTR.Shared.EItemType.Energy:
                id = 6;
                break;
            case DTR.Shared.EItemType.Car:
                id = 7;
                break;
            case DTR.Shared.EItemType.Character:
                id = 8;
                break;
            case DTR.Shared.EItemType.Gear:
                id = 9;
                break;
            case DTR.Shared.EItemType.Box:
                id = 10;
                break;
        }

        var itemRef = CReferenceManager.Instance.FindRefItemInfo(id);

        if (itemRef != null)
        {
            return itemRef.sortID;
        }
        else
            return -1;
    }

    public int GetItemSortID(short id)
    {
        var itemRef = CReferenceManager.Instance.FindRefItemInfo(id);

        if (itemRef != null)
        {
            return itemRef.sortID;
        }
        else
            return -1;
    }

    public int GetBoxSortID(DTR.Shared.EItemType type, int id)
    {
        if (type != DTR.Shared.EItemType.Box)
            return -1;

        var itemRef = CReferenceManager.Instance.FindRefBox(id);

        if (itemRef != null)
        {
            return itemRef.Rarity;
        }
        else
            return -1;
    }
}
