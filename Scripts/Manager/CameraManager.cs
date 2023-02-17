using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
    [ReadOnly] public Camera cam = null;

    public enum CamType { None, OutGame_LookAt, Follow_Type_Default, Follow_Type_Back, Follow_Type_Test, RotateAroundPlayer, CenterLookAt}
    public CamType currentCamType = CamType.None;

    [ReadOnly] public float CAMERA_DIST = 7.5f;
    [ReadOnly] public float CAMERA_HEIGHT = 8f;
    [ReadOnly] public float CAMERA_TARGET_Y_OFFSET = 0f;
    [ReadOnly] public float CAMERA_POSITION_SLERP_SPEED = 3.8f;
    [ReadOnly] public float CAMERA_POSITION_SLERP_SPEED_CHANGINGLINE = 3f;
    [ReadOnly] public float CAMERA_ROTATION_SLERP_SPEED = 17f;
    [ReadOnly] public float CAMERA_ROTATION_SLERP_SPEED_CHANGINGLINE = 5f;
    [ReadOnly] public int CAMERA_FIELD_OF_VIEW = 80;

    private void Start()
    {
        DataManager.Instance.firebaseBasicValueChangedCallback += OnFirebaseValueChangedCallback;
    }

    public void SetOutGameCam()
    {
        cam = Camera_Base.Instance.mainCam;

        if (cam == null)
            Debug.Log("<color=red> OutGame Camera is Missing!!!! </color>");
    }

    public void SetInGameCam()
    {
        cam = Camera_Base.Instance.mainCam;

        if (cam == null)
            Debug.Log("<color=red> InGame Camera is Missing!!!! </color>");

        Camera_Base.Instance.InitializeSettings();
        SetData();
        SetInGameCamSettings();
    }

    private void SetData()
    {
        if (DataManager.Instance.isBasicDataLoaded == true)
        {
            var data = DataManager.Instance.basicData.cameraData;

            CAMERA_HEIGHT = data.CAMERA_HEIGHT;
            CAMERA_DIST = data.CAMERA_DIST;
            CAMERA_POSITION_SLERP_SPEED = data.CAMERA_POSITION_SLERP_SPEED;
            CAMERA_ROTATION_SLERP_SPEED = data.CAMERA_ROTATION_SLERP_SPEED;
            CAMERA_POSITION_SLERP_SPEED_CHANGINGLINE = data.CAMERA_POSITION_SLERP_SPEED_CHANGINGLINE;
            CAMERA_ROTATION_SLERP_SPEED_CHANGINGLINE = data.CAMERA_ROTATION_SLERP_SPEED_CHANGINGLINE;
            CAMERA_TARGET_Y_OFFSET = data.CAMERA_TARGET_Y_OFFSET;
            CAMERA_FIELD_OF_VIEW = data.CAMERA_FIELD_OF_VIEW;
        }
        else
        {
            Debug.Log("Data Loaded is False....! Using Default values....");

            CAMERA_HEIGHT = 8f;
            CAMERA_DIST = 7.5f;
            CAMERA_TARGET_Y_OFFSET = 0f;
            CAMERA_POSITION_SLERP_SPEED = 4f;
            CAMERA_ROTATION_SLERP_SPEED = 17f;
            CAMERA_POSITION_SLERP_SPEED_CHANGINGLINE = 4f;
            CAMERA_ROTATION_SLERP_SPEED_CHANGINGLINE = 17f;
            CAMERA_FIELD_OF_VIEW = 80;
        }
    }

    private void SetInGameCamSettings()
    {
        if (cam != null)
        {
            cam.fieldOfView = CAMERA_FIELD_OF_VIEW;
        }
    }

    public void ChangeCamType(CamType type)
    {
        switch (type)
        {
            case CamType.OutGame_LookAt:
                {
                    var player = OutGameManager.Instance.outGamePlayer;
                    if (player != null && cam != null)
                    {
                        //Vector3 pos = new Vector3(-4.36f, 6.17f, 7.47f);
                        Vector3 pos = new Vector3(0f, 2.88f, -8.83f);
                        cam.transform.position = pos;

                        var target = player.transform.position + new Vector3(0f, 0.8f, 0f);
                        var dir = (target - cam.transform.position ).normalized;
                        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                        cam.transform.rotation = lookRot;

                        cam.fieldOfView = 60f;
                    }
                }
                break;

            case CamType.None:
                { 
                
                }
                break;
            
            case CamType.Follow_Type_Default:
                {
                    var player = InGameManager.Instance.myPlayer;
                    if (player != null)
                    {
                        Vector3 targetPos = player.transform.position + player.transform.TransformDirection(Vector3.forward) * -CAMERA_DIST + new Vector3(0f, CAMERA_HEIGHT, 0f);
                        Vector3 lookTarget = player.transform.position + new Vector3(0f, CAMERA_TARGET_Y_OFFSET, 0f);
                        var dir = (lookTarget - cam.transform.position).normalized;
                        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);

                        cam.transform.position = targetPos;
                        cam.transform.rotation = lookRot;
                    }
                }
                break;

            case CamType.Follow_Type_Back:
                {

                }
                break;

            case CamType.Follow_Type_Test:
                {

                }
                break;

            case CamType.RotateAroundPlayer:
                {
                    var player = InGameManager.Instance.myPlayer;
                    if (cam != null && player != null)
                    {
                        float height = CAMERA_HEIGHT - 3.5f;
                        var pos = player.transform.position + player.transform.forward * 5f + new Vector3(0f, height, 0f);
                        pos = new Vector3(pos.x, player.transform.position.y + height, pos.z);
                        cam.transform.position = pos;
                    }
                }
                break;

            case CamType.CenterLookAt:
                {

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
            case CamType.Follow_Type_Default:
                {
                    var player = InGameManager.Instance.myPlayer;
                    if (cam != null && player != null)
                    {
                        if (player.isFlipped)
                        {
                            float rotSlerpSpeed = CAMERA_ROTATION_SLERP_SPEED;
                            Vector3 lookTarget = player.transform.position + new Vector3(0f, CAMERA_TARGET_Y_OFFSET, 0f);
                            var dir = (lookTarget - cam.transform.position).normalized;
                            var smoothedDir = Vector3.Lerp(cam.transform.forward, dir, Time.fixedDeltaTime * rotSlerpSpeed);
                            Quaternion lookRot = Quaternion.LookRotation(smoothedDir, Vector3.up);

                            cam.transform.rotation = lookRot;
                        }
                        else //꼭 else로 처리하자!
                        {
                            float posSlerpSpeed;
                            if (player.isChangingLane == true)
                                posSlerpSpeed = CAMERA_POSITION_SLERP_SPEED_CHANGINGLINE;
                            else
                                posSlerpSpeed = CAMERA_POSITION_SLERP_SPEED;
                            float rotSlerpSpeed;
                            if (player.isChangingLane == true)
                                rotSlerpSpeed = CAMERA_ROTATION_SLERP_SPEED_CHANGINGLINE;
                            else
                                rotSlerpSpeed = CAMERA_ROTATION_SLERP_SPEED;

                            Vector3 targetPos = player.transform.position + player.transform.TransformDirection(Vector3.forward) * -CAMERA_DIST + new Vector3(0f, CAMERA_HEIGHT, 0f);
                            Vector3 smoothedPos = Vector3.Lerp(cam.transform.position, targetPos, Time.fixedDeltaTime * posSlerpSpeed);
                            smoothedPos = new Vector3(smoothedPos.x, player.transform.position.y + CAMERA_HEIGHT, smoothedPos.z);

                            Vector3 lookTarget = player.transform.position + new Vector3(0f, CAMERA_TARGET_Y_OFFSET, 0f);
                            var dir = (lookTarget - cam.transform.position).normalized;
                            Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);

                            cam.transform.position = smoothedPos;
                            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, lookRot, Time.fixedDeltaTime * rotSlerpSpeed);
                        }
                    }
                }
                break;

            case CamType.Follow_Type_Back:  //테스트중....!
                {

                }
                break;

            case CamType.Follow_Type_Test: //테스트3
                {
                    if (cam != null)
                    {


                    }
                }
                break;




            case CamType.RotateAroundPlayer:
                {
                    var player = InGameManager.Instance.myPlayer;
                    if (cam != null && player != null)
                    {
                        Vector3 lookTarget = player.transform.position;
                        cam.transform.RotateAround(lookTarget, Vector3.up, 50f * Time.deltaTime);
                        cam.transform.LookAt(lookTarget, Vector3.up);
                    }
                }
                break;

            case CamType.CenterLookAt: //TEMP....?
                {
                    var player = InGameManager.Instance.myPlayer;
                    var wp = InGameManager.Instance.wayPoints;
                    if (cam != null && player != null)
                    {
                        Vector3 camPos = wp.GetCenterOfWayPoints() + new Vector3(0f, 20f, 0f);

                        var dir = (player.transform.position - cam.transform.position).normalized;
                        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                        cam.transform.rotation = lookRot;
                        cam.transform.position = camPos;
                    }
                }
                break;
        }
    }


    //Jitter 현상떄문에 꼭 fixedUpdate에 카메라 이동 시켜주자
    private void FixedUpdate()
    {
        switch (currentCamType)
        {
            case CamType.None:
                { 
                
                }
                break;
            case CamType.Follow_Type_Default:
                {

                }
                break;

            case CamType.Follow_Type_Back:  //테스트중....!
                {

                }
                break;

            case CamType.Follow_Type_Test: //테스트3
                {
                    if (cam != null)
                    {


                    }
                }
                break;




            case CamType.RotateAroundPlayer:
                {

                }
                break;

            case CamType.CenterLookAt: //TEMP....?
                {

                }
                break;
        }
    }

    private void OnFirebaseValueChangedCallback()
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
}
