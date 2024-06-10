using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PhaseInGame : PhaseBase
{
    IEnumerator phaseInGameCoroutine;

    protected override void OnEnable()
    {
        base.OnEnable();

        UtilityCoroutine.StartCoroutine(ref phaseInGameCoroutine, PhaseInGameCoroutine(), this);
    }

    IEnumerator PhaseInGameCoroutine()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        yield return null;

        //혹시나 현재 점검 시간이면...
        if (ReferenceManager.Instance.AppDefines.IsMaintenanceTime == true)
        {
            Debug.Log("<color=red>ERROR!!!>>> IsMaintenanceTime == true</color>");
            NetworkManager_Client.Instance.LeaveRoom();
        }

        //일단 룸에 Max 인원이 들어왔으니 취소 버튼은 제거 하자
        PrefabManager.Instance.UI_InGameReady.ShowOrHideCancelButton(false);

        if (true) //모든 사람이 보내자....!
        {
            var allRefMapData = ReferenceManager.Instance.MapData?.MapInfoList;

            if (allRefMapData == null || allRefMapData.Count <= 0)
            {
                //이럴수가 없는데...? 혹시나 해서... 
                NetworkManager_Client.Instance.RaiseEvent(NetworkManager_Client.PhotonEventCode.SceneLoadFailed);
                yield break;
            }

            //플레이 할수 있는 맵 목록 가져오자... 현재는 임시로 groupid 1인 경우만...!
            var mapListToPlay = allRefMapData;

            if(NetworkManager_Client.Instance.CurrentPlayMode == Quantum.InGamePlayMode.SoloMode
                || NetworkManager_Client.Instance.CurrentPlayMode == Quantum.InGamePlayMode.TeamMode)
                mapListToPlay = allRefMapData.FindAll(x => x.isOpen.Equals(true) && x.GroupID.Equals(CommonDefine.MAP_GROUP_ID_SOLO_OR_TEAM_MODE));
            else
                mapListToPlay = allRefMapData.FindAll(x => x.isOpen.Equals(true) && x.GroupID.Equals(CommonDefine.MAP_GROUP_ID_PRACTICE_MODE));

            if (mapListToPlay == null || mapListToPlay.Count <= 0)
            {
                //이럴수가 없는데...? 혹시나 해서... 
                NetworkManager_Client.Instance.RaiseEvent(NetworkManager_Client.PhotonEventCode.SceneLoadFailed);
                yield break;
            }

            NetworkManager_Client.RoomData roomData = new NetworkManager_Client.RoomData();
            var randomIndex = Random.Range(0, mapListToPlay.Count);
            roomData.mapID = mapListToPlay[randomIndex].MapID;
            roomData.ingamePlayMode = (int)NetworkManager_Client.Instance.CurrentPlayMode;
            roomData.randomShuffledListOfPlayerForTeamMatch = new List<Photon.Realtime.Player>();
            roomData.randomSeed = UnityEngine.Random.Range(-1000, 1000);
            foreach(var i in NetworkManager_Client.QuantumClient.CurrentRoom.Players)
            {
                roomData.randomShuffledListOfPlayerForTeamMatch.Add(i.Value);
            }
            UtilityCommon.ShuffleList(ref roomData.randomShuffledListOfPlayerForTeamMatch);
            string jsonRoomData = JsonUtility.ToJson(roomData);
            NetworkManager_Client.Instance.RaiseEvent(NetworkManager_Client.PhotonEventCode.SendRoomData, jsonRoomData);
        }

        float waitTimer = 5f;
        //내 플레이어가 Room Data 받을 때까지 기다려주자...!
        while (true)
        {
            if (NetworkManager_Client.Instance != null && NetworkManager_Client.Instance.Quantum_RoomData != null 
                && NetworkManager_Client.Instance.Quantum_RoomData.mapID != -1)
                break;

            if (waitTimer <= 0)
            {
                Debug.Log("<color=red>ERROR!!!>>> Failed Waiting Other Players...</color>");
                NetworkManager_Client.Instance.LeaveRoom();
            }
            waitTimer -= Time.deltaTime;

            yield return null;
        }

        // UI에 선택된 맵 표시해주자
        PrefabManager.Instance.UI_InGameReady.SetFinalSelectMap();

        //Room Data 받았다고 다른 클라이언트에게 알리자
        NetworkManager_Client.Instance.RaiseEvent(NetworkManager_Client.PhotonEventCode.RoomDataReady, AccountManager.Instance.PID);

        waitTimer = 5f;
        //모든 플레이어가 Room Data 받을 때까지 기다려주자...!
        while (true)
        {
            if (NetworkManager_Client.Instance == null || NetworkManager_Client.Instance.Quantum_IsAllPlayerRoomDataReceived)
                break;

            if (waitTimer <= 0)
            {
                Debug.Log("<color=red>ERROR!!!>>> Failed Waiting Other Players...</color>");
                NetworkManager_Client.Instance.LeaveRoom();
            }
            waitTimer -= Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);
        PrefabManager.Instance.UI_SceneTransition.Show_In();
        yield return new WaitForSecondsRealtime(1f);

        //인게임 씬 + 맵 씬 로드 해주자
        SceneManagerExtensions.Instance.LoadScene(CommonDefine.InGameScene, LoadSceneMode.Single);
        SceneManagerExtensions.Instance.LoadMapScene(NetworkManager_Client.Instance.Quantum_RoomData.mapID, LoadSceneMode.Additive);

        yield return null;

        //인게임씬 + 맵 씬이 둘다 로드 될때까지 기달려주자
        while (true)
        {
            if (NetworkManager_Client.Instance.Quantum_RoomData.isInGameSceneLoaded
                && NetworkManager_Client.Instance.Quantum_RoomData.isMapSceneLoaded)
                break;

            yield return null;
        }
        SceneManagerExtensions.Instance.SetActiveScene(CommonDefine.InGameScene);

        //씬이 로드 되었다고 Event를 보내주자...!
        NetworkManager_Client.Instance.RaiseEvent(NetworkManager_Client.PhotonEventCode.SceneLoadReady, AccountManager.Instance.PID);

        waitTimer = 5f;
        //모든 플레이어가 씬 로드 될때까지 기다려주자
        while (true)
        {
            if (NetworkManager_Client.Instance == null || NetworkManager_Client.Instance.Quantum_IsAllPlayerSceneLoaded)
                break;

            if (waitTimer <= 0)
            {
                Debug.Log("<color=red>ERROR!!!>>> Failed Waiting Other Players...</color>");
                NetworkManager_Client.Instance.LeaveRoom();
            }
            waitTimer -= Time.deltaTime;

            yield return null;
        }

        //게임 시작
        InGameManager.Instance.StartGame_Unity();
        InGame_Quantum.Instance.StartGame_Quantum();
    }
}