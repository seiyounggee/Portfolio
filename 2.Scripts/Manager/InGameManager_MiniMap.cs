using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public partial class InGameManager
{
    [ReadOnly] public List<GameObject> minimapLineList = new List<GameObject>();
    [ReadOnly] public List<MiniMapPlayerInfo> minimapPlayerList = new List<MiniMapPlayerInfo>();
    [ReadOnly] public Camera miniMapCam = null; 

    private Vector3 MINIMAP_POSITION_OFFSET = Vector3.right * 2000f;
    private float MINIMAP_CAM_HEIGHT = 200f;

    [SerializeField] Material minimapPlayer_MeMaterial = null;
    [SerializeField] Material minimapPlayer_OtherMaterial = null;

    public class MiniMapPlayerInfo
    {
        public GameObject obj = null;
        public PlayerMovement pm = null;
    }

    public void InitializeMiniMap()
    {
        if (CommonDefine.GAME_MINIMAP_ACTIVATE == false)
            return;

        foreach (var i in minimapLineList)
        {
            Destroy(i);
        }

        foreach (var i in minimapPlayerList)
        {
            if (i.obj != null)
                Destroy(i.obj);
        }

        if (miniMapCam != null)
            Destroy(miniMapCam.gameObject);

        minimapLineList.Clear();
        minimapPlayerList.Clear();
        miniMapCam = null;
    }

    public void SetMiniMap()
    {
        if (CommonDefine.GAME_MINIMAP_ACTIVATE == false)
            return;

        if (wayPoints == null)
            return;

        var list = new List<List<WayPointSystem.Waypoint>>();
        list.Add(wayPoints.waypoints_0);
        list.Add(wayPoints.waypoints_1);
        list.Add(wayPoints.waypoints_2);
        list.Add(wayPoints.waypoints_3);
        list.Add(wayPoints.waypoints_4);
        list.Add(wayPoints.waypoints_6);
        list.Add(wayPoints.waypoints_6);

        for (int i = 0; i < list.Count; i++)
        {
            if (PrefabManager.Instance.MiniMap_LineRenderer == null)
                continue;

            var prefabLr = GameObject.Instantiate(PrefabManager.Instance.MiniMap_LineRenderer);
            minimapLineList.Add(prefabLr);
            prefabLr.gameObject.name = CommonDefine.GetGoNameWithHeader("MiniMap Line") + "  index-" + i;
        }

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null)
                continue;

            var lr = minimapLineList[i].GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.positionCount = list[i].Count + 1;
                Vector3[] positions = new Vector3[list[i].Count + 1];
                for (int j = 0; j < list[i].Count; j++)
                {
                    positions[j] = list[i][j].GetPosition() + MINIMAP_POSITION_OFFSET;
                }

                positions[list[i].Count] = list[i][0].GetPosition() + MINIMAP_POSITION_OFFSET; //마지막에 하나 추가해서 시작점이랑 연결 해주자...!

                lr.SetPositions(positions);
            }
        }




        for (int i = 0; i < CommonDefine.GetMaxPlayer(); i++)
        {
            if (PrefabManager.Instance.MiniMap_Player == null)
                continue;

            var prefabPlayer = GameObject.Instantiate(PrefabManager.Instance.MiniMap_Player);
            minimapPlayerList.Add(new MiniMapPlayerInfo() { obj = prefabPlayer , pm = null});
            prefabPlayer.gameObject.name = CommonDefine.GetGoNameWithHeader("MiniMap Player") + "  index-" + i;
        }

        if (PrefabManager.Instance.MiniMap_Camera != null)
        {
            var Prefabcam = GameObject.Instantiate(PrefabManager.Instance.MiniMap_Camera);
            miniMapCam = Prefabcam.GetComponent<Camera>();

            if (CameraManager.Instance.cam != null && miniMapCam != null)
                miniMapCam.depth = CameraManager.Instance.cam.depth - 1;
        }
    }

    public IEnumerator updateMiniMap = null;
    public IEnumerator UpdateMiniMap()
    {
        if (CommonDefine.GAME_MINIMAP_ACTIVATE == false)
            yield break;

        SetMiniMapPlayer();

        while (true)
        {
            if (gameState == GameState.PlayGame || gameState == GameState.EndCountDown)
            {
                for (int i = 0; i < minimapPlayerList.Count; i++)
                {
                    if (minimapPlayerList[i].obj != null && minimapPlayerList[i].pm != null)
                    {

                        Vector3 newPos = minimapPlayerList[i].pm.transform.position + MINIMAP_POSITION_OFFSET;
                        newPos.y = 0f; //높이는 반영x
                        minimapPlayerList[i].obj.transform.position = newPos;

                        if (miniMapCam != null && minimapPlayerList[i].pm.IsMine)
                        {
                            Vector3 camPos = newPos + Vector3.up * MINIMAP_CAM_HEIGHT;
                            miniMapCam.transform.position = camPos;

                            var waypoint = minimapPlayerList[i].pm.wg.waypoints_3;
                            var currIndex = minimapPlayerList[i].pm.currentMoveIndex;
                            var currLane= minimapPlayerList[i].pm.currentLaneType;
                            int nextIndex = minimapPlayerList[i].pm.GetNextMoveIndex(currLane, currIndex);
                            var currWayDir = (waypoint[nextIndex].GetPosition() - waypoint[currIndex].GetPosition()).normalized;
                            currWayDir.y = 0f;

                            var angle = UtilityCommon.Calculate360Angle(currWayDir, Vector3.forward);
                            Quaternion rot = Quaternion.Euler(90f, angle, 0f);
                            miniMapCam.transform.rotation = Quaternion.Slerp(miniMapCam.transform.rotation, rot, Time.fixedDeltaTime * 5f);
                        }
                    }
                }
            }

            yield return new WaitForFixedUpdate();

            if (gameState == GameState.EndGame)
                break;
        }

        miniMapCam.gameObject.SafeSetActive(false);
    }

    private void SetMiniMapPlayer()
    {
        if (minimapPlayer_MeMaterial == null || minimapPlayer_OtherMaterial == null)
            return;

        for (int i = 0; i < minimapPlayerList.Count; i++)
        {
            if (i < ListOfPlayers.Count && ListOfPlayers[i].pm != null)
                minimapPlayerList[i].pm = ListOfPlayers[i].pm;
        }

            
        for (int i = 0; i < minimapPlayerList.Count; i++)
        {
            if (minimapPlayerList[i].obj != null && minimapPlayerList[i].pm != null)
            {
                var mesh = minimapPlayerList[i].obj.GetComponent<MeshRenderer>();
                if (mesh != null && miniMapCam != null)
                {
                    if (minimapPlayerList[i].pm.IsMine)
                        mesh.material = minimapPlayer_MeMaterial;
                    else
                        mesh.material = minimapPlayer_OtherMaterial;
                }
            }
        }
    }


}
