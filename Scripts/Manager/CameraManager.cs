using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Fusion.Editor;

public class CameraManager : MonoSingleton<CameraManager>
{
    [ReadOnly] public Camera_Base mainCam_Script = null;
    [ReadOnly] public Camera mainCam = null;

    [ReadOnly] public SubCamera_Base subCam_Script = null;
    [ReadOnly] public Camera subCam = null;

    public enum CamType 
    { 
        None, 
        OutGame_MainCam_LookAt, 

        InGame_SubCam_MapIntro,
        InGame_SubCam_MapOutroCeremony,

        InGame_MainCam_LookAtPlayerIntro,
        InGame_MainCam_FollowPlayerBack_Default, 
        InGame_MainCam_FollowPlayerFront,
        InGame_MainCam_RotateAroundPlayer, 
        InGame_MainCam_CenterLookAtPlayer

    }
    public CamType currentCamType = CamType.None;

    public enum CamPriority { Main, Sub}
    public CamPriority currentCamPriority = CamPriority.Main;

    [ReadOnly] public float CAMERA_DIST = 7.5f;
    [ReadOnly] public float CAMERA_HEIGHT = 8f;
    [ReadOnly] public float CAMERA_TARGET_Y_OFFSET = 0f;
    [ReadOnly] public float CAMERA_POSITION_SLERP_SPEED = 3.8f;
    [ReadOnly] public float CAMERA_POSITION_SLERP_SPEED_CHANGINGLINE = 3f;
    [ReadOnly] public float CAMERA_ROTATION_SLERP_SPEED = 17f;
    [ReadOnly] public float CAMERA_ROTATION_SLERP_SPEED_CHANGINGLINE = 5f;
    [ReadOnly] public int CAMERA_FIELD_OF_VIEW = 80;

    [ReadOnly] public float CAMREA_ADJUSTMENT_AT_CURVED_POSITION_LENGTH = 3f;
    [ReadOnly] public float CAMREA_ADJUSTMENT_AT_CURVED_POSITION_ADDITIONAL_ANGLE = 1f;

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    private void FixedUpdate()
    {
        switch (PhaseManager.Instance.CurrentPhase)
        {
            case CommonDefine.Phase.None:
            case CommonDefine.Phase.Initialize:
            case CommonDefine.Phase.Lobby:
                { 
                
                }
                break;

            case CommonDefine.Phase.InGameReady:
            case CommonDefine.Phase.InGame:
            case CommonDefine.Phase.InGameResult:
                {
                    //FixedUpdate_Cam 활용
                }
                break;
        }
    }

    public void SetOutGameCam()
    {
        mainCam = Camera_Base.Instance.mainCam;
        mainCam_Script = Camera_Base.Instance;

        if (mainCam == null)
            Debug.Log("<color=red> OutGame Camera is Missing!!!! </color>");

        Camera_Base.Instance.InitializeSettings();

        if (GameObject.FindObjectOfType<SubCamera_Base>() != null)
        {
            var sub = GameObject.FindObjectOfType<SubCamera_Base>();
            subCam = sub.subCam;
            subCam_Script = sub;
        }
        else
        {
            subCam = null;
            subCam_Script = null;
        }
    }

    public void SetInGameCam()
    {
        mainCam = Camera_Base.Instance.mainCam;

        if (mainCam == null)
            Debug.Log("<color=red> InGame Camera is Missing!!!! </color>");

        Camera_Base.Instance.InitializeSettings();
        SetData();
        SetInGameCamSettings();

        if (GameObject.FindObjectOfType<SubCamera_Base>() != null)
        {
            var sub = GameObject.FindObjectOfType<SubCamera_Base>();
            subCam = sub.subCam;
            subCam_Script = sub;
        }
        else
        {
            subCam = null;
            subCam_Script = null;
        }
    }

    public void ChangeCamPriority(CamPriority priority)
    {
        switch (priority)
        {
            case CamPriority.Main:
            default:
                {
                    if (mainCam != null)
                        mainCam.depth = 1;
                    if (subCam != null)
                        subCam.depth = 0;

                    currentCamPriority = CamPriority.Main;
                }
                break;
            case CamPriority.Sub:
                {
                    if (mainCam != null)
                        mainCam.depth = 0;
                    if (subCam != null)
                        subCam.depth = 1;

                    currentCamPriority = CamPriority.Sub;
                }
                break;
        }
    }

    private void SetData()
    {
        CAMERA_HEIGHT = DataManager.Instance.GetGameConfig<float>("cameraHeight");
        CAMERA_DIST = DataManager.Instance.GetGameConfig<float>("cameraDist");
        CAMERA_POSITION_SLERP_SPEED = DataManager.Instance.GetGameConfig<float>("cameraPositionSlerpSpeed");
        CAMERA_ROTATION_SLERP_SPEED = DataManager.Instance.GetGameConfig<float>("cameraRotationSlerpSpeed");
        CAMERA_POSITION_SLERP_SPEED_CHANGINGLINE = DataManager.Instance.GetGameConfig<float>("cameraPositionSlerpSpeedChangingLine");
        CAMERA_ROTATION_SLERP_SPEED_CHANGINGLINE = DataManager.Instance.GetGameConfig<float>("cameraRotationSlerpSpeedChangingLine");
        CAMERA_TARGET_Y_OFFSET = DataManager.Instance.GetGameConfig<float>("cameraTargetYOffset");
        CAMERA_FIELD_OF_VIEW = DataManager.Instance.GetGameConfig<int>("cameraFieldOfView");
        CAMREA_ADJUSTMENT_AT_CURVED_POSITION_LENGTH = DataManager.Instance.GetGameConfig<float>("cameraAdjustmentCurvedPositionLength");
        CAMREA_ADJUSTMENT_AT_CURVED_POSITION_ADDITIONAL_ANGLE = DataManager.Instance.GetGameConfig<float>("cameraAdjustmentCurvedPositionAdditionalAngle");
    }

    private void SetInGameCamSettings()
    {
        if (mainCam != null)
        {
            mainCam.fieldOfView = CAMERA_FIELD_OF_VIEW;
        }
    }

    public void ChangeCamType(CamType type)
    {
        switch (type)
        {
            case CamType.None:
            default:
                {

                }
                break;

            case CamType.OutGame_MainCam_LookAt:
                {
                    ChangeCamPriority(CamPriority.Main);

                    var player = OutGameManager.Instance.outGamePlayer;
                    if (player != null && mainCam != null)
                    {
                        //Vector3 pos = new Vector3(-4.36f, 6.17f, 7.47f);
                        //Vector3 pos = new Vector3(-1.36f, 2.61f, -13.82f);
                        Vector3 pos = new Vector3(0f, 4.35f, -8.42f);
                        mainCam.transform.position = pos;

                        Quaternion rot = new Quaternion();
                        //rot.eulerAngles = new Vector3(3.94f, 5.83f, 0.499f);
                        rot.eulerAngles = new Vector3(22f, 0f, 0f);
                        mainCam.transform.rotation = rot;
                        mainCam.fieldOfView = 60f;
                    }
                }
                break;

            case CamType.InGame_SubCam_MapIntro:
                {
                    ChangeCamPriority(CamPriority.Sub);

                    if (subCam != null)
                    {
                        if (subCam_Script != null)
                        {
                            var player = InGameManager.Instance.myPlayer;
                            if (player != null)
                            {
                                float camDist = CAMERA_DIST * 4f;
                                float camHeight = 10f;
                                Vector3 targetPos = player.transform.position + player.transform.TransformDirection(Vector3.forward) * camDist + new Vector3(0f, camHeight, 0f);

                                Vector3 lookTarget = player.transform.position + new Vector3(0f, CAMERA_TARGET_Y_OFFSET, 0f);
                                var dir = (lookTarget - targetPos).normalized;
                                Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);

                                mainCam.transform.position = targetPos;
                                mainCam.transform.rotation = lookRot;
                            }


                            subCam_Script.SetAnimation(SubCamera_Base.AnimationType.PlayIntro);
                        }
                        else
                            ChangeCamType(CamType.InGame_MainCam_LookAtPlayerIntro); //만약 intro 애니메이션이 없으면... 그냥 차 보여주자
                    }
                    else
                        ChangeCamType(CamType.InGame_MainCam_LookAtPlayerIntro); //만약 intro 애니메이션이 없으면... 그냥 차 보여주자
                }
                break;

            case CamType.InGame_SubCam_MapOutroCeremony:
                {
                    ChangeCamPriority(CamPriority.Sub);

                    if (subCam != null)
                    {
                        if (subCam_Script != null)
                        {
                            subCam_Script.SetAnimation(SubCamera_Base.AnimationType.PlayOutro_Ceremony);
                        }
                    }
                    else
                    {
                        ChangeCamType(CamType.InGame_MainCam_FollowPlayerBack_Default);
                    }
                }
                break;

            case CamType.InGame_MainCam_LookAtPlayerIntro:
                {
                    ChangeCamPriority(CamPriority.Main);

                    var player = InGameManager.Instance.myPlayer;
                    if (player != null)
                    {
                        float camDist = CAMERA_DIST * 4f;
                        float camHeight = 10f;
                        Vector3 targetPos = player.transform.position + player.transform.TransformDirection(Vector3.forward) * camDist + new Vector3(0f, camHeight, 0f);

                        Vector3 lookTarget = player.transform.position + new Vector3(0f, CAMERA_TARGET_Y_OFFSET, 0f);
                        var dir = (lookTarget - targetPos).normalized;
                        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);

                        mainCam.transform.position = targetPos;
                        mainCam.transform.rotation = lookRot;
                    }
                }
                break;


            case CamType.InGame_MainCam_FollowPlayerBack_Default:
                {
                    ChangeCamPriority(CamPriority.Main);

                    var player = InGameManager.Instance.myPlayer;
                    if (player != null)
                    {
                        Vector3 targetPos = player.transform.position + player.transform.TransformDirection(Vector3.forward) * -CAMERA_DIST + new Vector3(0f, CAMERA_HEIGHT, 0f);
                        Vector3 lookTarget = player.transform.position + new Vector3(0f, CAMERA_TARGET_Y_OFFSET, 0f);
                        var dir = (lookTarget - mainCam.transform.position).normalized;
                        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);

                        mainCam.transform.position = targetPos;
                        mainCam.transform.rotation = lookRot;
                    }
                }
                break;

            case CamType.InGame_MainCam_FollowPlayerFront:
                {
                    ChangeCamPriority(CamPriority.Main);

                    var player = InGameManager.Instance.myPlayer;
                    if (player != null)
                    {
                        float camDist = CAMERA_DIST * 3f;
                        Vector3 targetPos = player.transform.position + player.transform.TransformDirection(Vector3.forward) * camDist + new Vector3(0f, CAMERA_HEIGHT, 0f);
                        Vector3 lookTarget = player.transform.position + new Vector3(0f, CAMERA_TARGET_Y_OFFSET, 0f);
                        var dir = (lookTarget - mainCam.transform.position).normalized;
                        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);

                        mainCam.transform.position = targetPos;
                        mainCam.transform.rotation = lookRot;
                    }
                }
                break;


            case CamType.InGame_MainCam_RotateAroundPlayer:
                {
                    ChangeCamPriority(CamPriority.Main);

                    var player = InGameManager.Instance.myPlayer;
                    if (mainCam != null && player != null)
                    {
                        float height = CAMERA_HEIGHT - 3.5f;
                        var pos = player.transform.position + player.transform.forward * 5f + new Vector3(0f, height, 0f);
                        pos = new Vector3(pos.x, player.transform.position.y + height, pos.z);
                        mainCam.transform.position = pos;
                    }
                }
                break;

            case CamType.InGame_MainCam_CenterLookAtPlayer:
                {
                    ChangeCamPriority(CamPriority.Main);
                }
                break;
        }

        currentCamType = type;
    }

    public void FixedUpdate_Cam()
    {
        switch (currentCamType)
        {
            case CamType.None:
                {

                }
                break;

            case CamType.InGame_MainCam_LookAtPlayerIntro:
                {
                    var player = InGameManager.Instance.myPlayer;
                    if (player != null)
                    {
                        float camDist = CAMERA_DIST * 1.5f;
                        float camHeight = 4f;
                        float camPositionSpeed = CAMERA_POSITION_SLERP_SPEED * 0.5f;
                        Vector3 targetPos = player.transform.position + player.transform.TransformDirection(Vector3.forward) * camDist + new Vector3(0f, camHeight, 0f);
                        Vector3 smoothedPos = Vector3.Slerp(mainCam.transform.position, targetPos, player.Runner.DeltaTime * camPositionSpeed);

                        Vector3 lookTarget = player.transform.position + new Vector3(0f, CAMERA_TARGET_Y_OFFSET, 0f);
                        var dir = (lookTarget - mainCam.transform.position).normalized;
                        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);

                        mainCam.transform.position = smoothedPos;
                        mainCam.transform.rotation = Quaternion.Slerp(mainCam.transform.rotation, lookRot, player.Runner.DeltaTime * CAMERA_ROTATION_SLERP_SPEED);
                    }
                }
                break;

            case CamType.InGame_MainCam_FollowPlayerBack_Default:
                {
                    var player = InGameManager.Instance.myPlayer;
                    if (mainCam != null && player != null)
                    {
                        if (player.isFlipped)
                        {
                            float rotSlerpSpeed = CAMERA_ROTATION_SLERP_SPEED;
                            Vector3 lookTarget = player.transform.position + new Vector3(0f, CAMERA_TARGET_Y_OFFSET, 0f);
                            var dir = (lookTarget - mainCam.transform.position).normalized;
                            var smoothedDir = Vector3.Lerp(mainCam.transform.forward, dir, player.Runner.DeltaTime * rotSlerpSpeed);
                            Quaternion lookRot = Quaternion.LookRotation(smoothedDir, Vector3.up);

                            mainCam.transform.rotation = lookRot;
                        }
                        else //꼭 else로 처리하자!
                        {
                            float posSlerpSpeed;
                            if (player.network_isChangingLane == true)
                                posSlerpSpeed = CAMERA_POSITION_SLERP_SPEED_CHANGINGLINE;
                            else
                                posSlerpSpeed = CAMERA_POSITION_SLERP_SPEED;
                            float rotSlerpSpeed;
                            if (player.network_isChangingLane == true)
                                rotSlerpSpeed = CAMERA_ROTATION_SLERP_SPEED_CHANGINGLINE;
                            else
                                rotSlerpSpeed = CAMERA_ROTATION_SLERP_SPEED;

                            Vector3 targetPos = player.transform.position + player.transform.TransformDirection(Vector3.forward) * -CAMERA_DIST + new Vector3(0f, CAMERA_HEIGHT, 0f);
                            var camAdjustPos = IngameCameraPositionAdjusmentAtCurvedPoint();
                            if (camAdjustPos.HasValue)
                            {
                                targetPos -= camAdjustPos.Value;
                                posSlerpSpeed = Mathf.Clamp(posSlerpSpeed, 1f, 3.1f); //slerp speed를 늦추자... 곡선->직전 구간에서 자연스러움을 위해...
                            }


                            Vector3 smoothedPos = Vector3.Lerp(mainCam.transform.position, targetPos, player.Runner.DeltaTime * posSlerpSpeed);
                            smoothedPos = new Vector3(smoothedPos.x, player.transform.position.y + CAMERA_HEIGHT, smoothedPos.z);

                            Vector3 lookTarget = player.transform.position + new Vector3(0f, CAMERA_TARGET_Y_OFFSET, 0f);
                            var dir = (lookTarget - mainCam.transform.position).normalized;
                            Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);

                            mainCam.transform.position = smoothedPos;
                            mainCam.transform.rotation = Quaternion.Lerp(mainCam.transform.rotation, lookRot, player.Runner.DeltaTime * rotSlerpSpeed);
                        }
                    }
                }
                break;

            case CamType.InGame_MainCam_FollowPlayerFront:
                {
                    var player = InGameManager.Instance.myPlayer;
                    if (mainCam != null && player != null)
                    {
                        float camDist = CAMERA_DIST * 3f;
                        Vector3 targetPos = player.transform.position + player.transform.TransformDirection(Vector3.forward) * camDist + new Vector3(0f, CAMERA_HEIGHT, 0f);
                        Vector3 smoothedPos = Vector3.Slerp(mainCam.transform.position, targetPos, player.Runner.DeltaTime * CAMERA_POSITION_SLERP_SPEED);

                        Vector3 lookTarget = player.transform.position + new Vector3(0f, CAMERA_TARGET_Y_OFFSET, 0f);
                        var dir = (lookTarget - mainCam.transform.position).normalized;
                        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);

                        mainCam.transform.position = smoothedPos;
                        mainCam.transform.rotation = Quaternion.Slerp(mainCam.transform.rotation, lookRot, player.Runner.DeltaTime * CAMERA_ROTATION_SLERP_SPEED);
                    }
                }
                break;


            case CamType.InGame_MainCam_RotateAroundPlayer:
                {
                    var player = InGameManager.Instance.myPlayer;
                    if (mainCam != null && player != null)
                    {
                        Vector3 lookTarget = player.transform.position;
                        mainCam.transform.RotateAround(lookTarget, Vector3.up, 50f * player.Runner.DeltaTime);
                        mainCam.transform.LookAt(lookTarget, Vector3.up);
                    }
                }
                break;

            case CamType.InGame_MainCam_CenterLookAtPlayer:
                {
                    var player = InGameManager.Instance.myPlayer;
                    var wp = InGameManager.Instance.wayPoints;
                    if (mainCam != null && player != null)
                    {
                        Vector3 camPos = wp.GetCenterOfWayPoints() + new Vector3(0f, 20f, 0f);

                        var dir = (player.transform.position - mainCam.transform.position).normalized;
                        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                        mainCam.transform.rotation = lookRot;
                        mainCam.transform.position = camPos;
                    }
                }
                break;
        }
    }

    private void FirebaseBasicValueChanged_SubscriptionEvent()
    {
        SetData();

        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGame)
            SetInGameCamSettings();
    }


    public Camera GetMinimapCamera()
    {
        Camera cam = null;

        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGame)
        {
            if (InGameManager.Instance.miniMapCam != null)
                cam = InGameManager.Instance.miniMapCam;
        }

        return cam;
    }

    //곡선 구간에서 보정...!
    private Vector3? IngameCameraPositionAdjusmentAtCurvedPoint()
    {
        Vector3? v = null;
        var player = InGameManager.Instance.myPlayer;
        if (mainCam != null && player != null)
        {
            if (CAMREA_ADJUSTMENT_AT_CURVED_POSITION_LENGTH == 0 && CAMREA_ADJUSTMENT_AT_CURVED_POSITION_ADDITIONAL_ANGLE == 0)
                return null;

            if (player.network_isMoving && player.network_isEnteredTheFinishLine == false)
            {
                var prevIndex = player.previousMoveIndex;
                var currentIndex = player.client_currentMoveIndex;
                var currentLaneType = player.client_currentLaneType;
                var nextIndex = player.GetNextMoveIndex(currentLaneType, currentIndex);
                var nextnextIndex = player.GetNextMoveIndex(currentLaneType, nextIndex);

                var currentWaypointDir = (player.GetDestination(currentIndex, currentLaneType) - player.GetDestination(prevIndex, currentLaneType)).normalized;
                var nextWaypointDir = (player.GetDestination(nextIndex, currentLaneType) - player.GetDestination(currentIndex, currentLaneType)).normalized;
                var nextnextWaypointDir = (player.GetDestination(nextnextIndex, currentLaneType) - player.GetDestination(nextIndex, currentLaneType)).normalized;

                if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(currentWaypointDir, nextWaypointDir)), 1f)
                    || Mathf.Approximately(Mathf.Abs(Vector3.Dot(nextWaypointDir, nextnextWaypointDir)), 1f))
                {
                    //평행인 경우 추가 보정할 필요 x
                    v = null;
                    return v;
                }

                var currentWaypointDir_ortho = Vector3.Cross(currentWaypointDir, Vector3.up).normalized;
                float angle = Vector3.Angle(currentWaypointDir_ortho, nextWaypointDir);
                var currPlayerDir_ortho = Vector3.Cross(player.transform.forward, Vector3.up).normalized;

                //angle의 크기에 따라서 보정값을 다르게 주자...! angle이 크면 보정 크게
                v = nextWaypointDir;

                if (UtilityCommon.IsRight(currentWaypointDir, player.GetDestination(currentIndex, currentLaneType), player.GetDestination(nextIndex, currentLaneType)))
                {
                    v = nextWaypointDir * CAMREA_ADJUSTMENT_AT_CURVED_POSITION_LENGTH * (1 -(float)(90 - angle) / 90f);
                    v += -1f * CAMREA_ADJUSTMENT_AT_CURVED_POSITION_ADDITIONAL_ANGLE * currPlayerDir_ortho * (1 - (float)(90 - angle) / 90f);
                }
                else //IsLeft
                {
                    v = nextWaypointDir * CAMREA_ADJUSTMENT_AT_CURVED_POSITION_LENGTH * (1 - (float)(90 - angle) / 90f);
                    v += +1f * CAMREA_ADJUSTMENT_AT_CURVED_POSITION_ADDITIONAL_ANGLE * currPlayerDir_ortho * (1 - (float)(90 - angle) / 90f);
                }
            }
        }

        return v;
    }
}
