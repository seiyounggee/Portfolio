using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public partial class InGameManager
{
    [Header("[Mini Map]")]
    [ReadOnly] public List<GameObject> minimapLineList = new List<GameObject>();
    [ReadOnly] public List<MiniMapPlayerInfo> minimapPlayerList = new List<MiniMapPlayerInfo>();
    [ReadOnly] public Camera miniMapCam = null; 

    private Vector3 MINIMAP_POSITION_OFFSET = Vector3.right * 3000f;
    private float MINIMAP_CAM_HEIGHT = 200f;

    [SerializeField] Material minimapPlayer_MeMaterial = null;
    [SerializeField] Material minimapPlayer_OtherMaterial = null;

    private GameObject minimapBase = null;

    public class MiniMapPlayerInfo
    {
        public GameObject obj = null;
        public PlayerMovement pm = null;
    }

    public void InitializeMiniMap()
    {
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

        if (minimapBase != null)
            Destroy(minimapBase);

        minimapLineList.Clear();
        minimapPlayerList.Clear();
        miniMapCam = null;
    }

    public void SetMiniMap()
    {
        if (wayPoints == null)
            return;

        minimapBase = new GameObject();
        minimapBase.name = "Minimap_BASE";

        var list = new List<List<WayPointSystem.Waypoint>>();
        list.Add(wayPoints.waypoints_0);
        list.Add(wayPoints.waypoints_1);
        list.Add(wayPoints.waypoints_2);
        list.Add(wayPoints.waypoints_3);
        list.Add(wayPoints.waypoints_4);
        list.Add(wayPoints.waypoints_5);
        list.Add(wayPoints.waypoints_6);

        for (int i = 0; i < list.Count; i++)
        {
            if (PrefabManager.Instance.MiniMap_LineRenderer == null)
                continue;

            var prefabLr = GameObject.Instantiate(PrefabManager.Instance.MiniMap_LineRenderer);
            minimapLineList.Add(prefabLr);
            prefabLr.name = CommonDefine.GetGoNameWithHeader("MiniMap Line") + "  index-" + i;
            prefabLr.transform.parent = minimapBase.transform;
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




        for (int i = 0; i < DataManager.Instance.GetSessionTotalCount(); i++)
        {
            if (PrefabManager.Instance.MiniMap_Player == null)
                continue;

            var prefabPlayer = GameObject.Instantiate(PrefabManager.Instance.MiniMap_Player);
            minimapPlayerList.Add(new MiniMapPlayerInfo() { obj = prefabPlayer , pm = null});
            prefabPlayer.gameObject.name = CommonDefine.GetGoNameWithHeader("MiniMap Player") + "  index-" + i;
            prefabPlayer.transform.position = MINIMAP_POSITION_OFFSET;
            prefabPlayer.transform.parent = minimapBase.transform;
        }

        if (PrefabManager.Instance.MiniMap_Camera != null)
        {
            var Prefabcam = GameObject.Instantiate(PrefabManager.Instance.MiniMap_Camera);
            miniMapCam = Prefabcam.GetComponent<Camera>();
            Prefabcam.transform.parent = minimapBase.transform;

            if (CameraManager.Instance.mainCam != null && miniMapCam != null)
                miniMapCam.depth = CameraManager.Instance.mainCam.depth - 1;
        }
    }

    public IEnumerator updateMiniMap = null;
    public IEnumerator UpdateMiniMap()
    {
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
                        minimapPlayerList[i].obj.transform.position = newPos;

                        if (miniMapCam != null && minimapPlayerList[i].pm.IsMineAndNotAI)
                        {
                            Vector3 camPos = newPos + Vector3.up * MINIMAP_CAM_HEIGHT;
                            miniMapCam.transform.position = camPos;

                            var waypoint = minimapPlayerList[i].pm.wg.waypoints_3;
                            var currIndex = minimapPlayerList[i].pm.client_currentMoveIndex;
                            var currLane = minimapPlayerList[i].pm.client_currentLaneType;
                            int nextIndex = minimapPlayerList[i].pm.GetNextMoveIndex(currLane, currIndex);
                            var currWayDir = (waypoint[nextIndex].GetPosition() - waypoint[currIndex].GetPosition()).normalized;
                            currWayDir.y = 0f;

                            var angle = UtilityCommon.Calculate360Angle(currWayDir, Vector3.forward);
                            Quaternion rot = Quaternion.Euler(90f, angle, 0f);
                            miniMapCam.transform.rotation = Quaternion.Slerp(miniMapCam.transform.rotation, rot, Time.fixedDeltaTime * 5f);
                        }
                    }
                    else if (minimapPlayerList[i].pm == null)
                    {
                        minimapPlayerList[i].obj.SafeSetActive(false);
                    }
                }
            }

            yield return new WaitForFixedUpdate();

            if (gameState == GameState.EndGame)
                break;

            if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
                break;
        }

        if (miniMapCam != null)
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
            else
                minimapPlayerList[i].obj.SafeSetActive(false);
        }

            
        for (int i = 0; i < minimapPlayerList.Count; i++)
        {
            if (minimapPlayerList[i].obj != null && minimapPlayerList[i].pm != null)
            {
                var mesh = minimapPlayerList[i].obj.GetComponent<MeshRenderer>();
                if (mesh != null && miniMapCam != null)
                {
                    if (minimapPlayerList[i].pm.IsMineAndNotAI)
                        mesh.material = minimapPlayer_MeMaterial;
                    else
                        mesh.material = minimapPlayer_OtherMaterial;
                }
            }
        }
    }


}
