using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using Quantum;
using DG.Tweening;

public class CameraManager : MonoSingleton<CameraManager>
{
    public Camera MainCamera
    {
        get
        {
            return Camera.main;
        }
    }

    [ReadOnly] public CinemachineVirtualCamera OutGame_MainCinemachineCamera = null;

    [ReadOnly] public Unity_CameraParent InGame_MainCinemachineCameraParent = null;
    [ReadOnly] public CinemachineVirtualCamera InGame_MainCinemachineCameraChild = null;

    public enum InGameCameraMode
    {
        SimpleLookAt, 
        LockIn, 
        Spectator, //관전모드
    }
    [ReadOnly] public InGameCameraMode CurrentIngameCameraMode = InGameCameraMode.SimpleLookAt;

    public InGame_Quantum.PlayerInfo InGameCamera_TargetPlayerInfo = null;
    public GameObject InGameCamera_TargetPlayerGameObj = null;

    public float CameraOffsetDistance { get { return offsetDist_desired; } }

    private bool isCamManuallyRotating = false;
    private Vector2 rotateVector = Vector2.zero;
    Vector3 offsetDir = new Vector3(0f, 1f, -1f);
    float offsetDist_desired = 10f;
    float offsetDist_current = 10f;

    public const float MIN_OFFSET_DISTANCE = 7f;
    public const float MAX_OFFSET_DISTANCE = 15f;

    float xAngle = 0f;
    float yAngle = 0f;

    bool isRotateInput = false;

    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        QuantumEvent.Subscribe<EventPlayerEvents>(this, OnEventPlayerEvents);
    }

    protected void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        QuantumEvent.UnsubscribeListener<EventPlayerEvents>(this);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManagerExtensions.IsSceneActive(SceneManagerExtensions.SceneType.OutGame))
        {
            OutGame_MainCinemachineCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
        }

        if (SceneManagerExtensions.IsSceneActive(SceneManagerExtensions.SceneType.InGame))
        {
            InGame_MainCinemachineCameraParent = FindAnyObjectByType<Unity_CameraParent>();
            InGame_MainCinemachineCameraChild = InGame_MainCinemachineCameraParent.MainCinemachineCameraChild;

            InGameCamera_TargetPlayerInfo = null;
            InGameCamera_TargetPlayerGameObj = null;

            isRotateInput = false;

            rotateVector = Vector3.zero;
            xAngle = 0f;
            yAngle = 0f;

            CurrentIngameCameraMode = (InGameCameraMode)AccountManager.Instance.AccountData.ingameSettings_cameramode;
            offsetDist_desired = AccountManager.Instance.AccountData.ingameSettings_cameraOffsetDistance;
        }
    }


    private void Update()
    {
        if (SceneManagerExtensions.IsSceneActive(SceneManagerExtensions.SceneType.OutGame))
        {
            Update_Outgame();
        }


        if (SceneManagerExtensions.IsSceneActive(SceneManagerExtensions.SceneType.InGame))
        {
            Update_Ingame();
        }
    }

    private void Update_Outgame()
    {
        if (OutGame_MainCinemachineCamera != null)
        {
            var player = OutGameManager.Instance.playerChar;
            if (player != null)
            {
                OutGame_MainCinemachineCamera.transform.position = new Vector3(0f, 1f, -3.5f);
                OutGame_MainCinemachineCamera.transform.rotation = Quaternion.Euler(Vector3.zero);
            }
        }
    }

    private unsafe void Update_Ingame()
    {
        if (InGame_MainCinemachineCameraChild != null && InGame_MainCinemachineCameraParent != null)
        {
            if (InGame_Quantum.Instance != null)
            {
                //CheckForBoundary();

                //없으면 나 Target...! (최초 1번 실행)
                if (InGameCamera_TargetPlayerInfo == null && InGameCamera_TargetPlayerGameObj == null)
                {
                    ChangeIngameTarget_Me();

                    //최초 세팅
                    if (InGameCamera_TargetPlayerGameObj != null) //세팅이 되면...
                    {
                        var initialDir = InGameCamera_TargetPlayerGameObj.transform.forward;
                        InGame_MainCinemachineCameraParent.transform.rotation = Quaternion.LookRotation(initialDir);
                        xAngle = InGame_MainCinemachineCameraParent.transform.rotation.eulerAngles.x;
                        yAngle = InGame_MainCinemachineCameraParent.transform.rotation.eulerAngles.y;
                    }
                }

                if (InGameCamera_TargetPlayerInfo != null && InGameCamera_TargetPlayerGameObj != null)
                {
                    //Position
                    InGame_MainCinemachineCameraParent.transform.position = InGameCamera_TargetPlayerGameObj.transform.position;
                    InGame_MainCinemachineCameraChild.transform.localPosition = offsetDir.normalized * offsetDist_current;

                    //LookAt 
                    Vector3 direction = ((InGameCamera_TargetPlayerGameObj.transform.position + Vector3.up * 2) - InGame_MainCinemachineCameraChild.transform.position).normalized;
                    Quaternion desiredRotation = Quaternion.LookRotation(direction);
                    InGame_MainCinemachineCameraChild.transform.rotation = desiredRotation;

                    //Rotation
                    if (isCamManuallyRotating)
                    {
                        var rotateSpeed = 150f;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                        rotateSpeed = 150f;
#endif
                        rotateSpeed = 150f;
                        yAngle += rotateVector.x * rotateSpeed * Time.deltaTime;
                        xAngle -= rotateVector.y * rotateSpeed * Time.deltaTime;
                        xAngle = Mathf.Clamp(xAngle, -30f, 30f);

                        Quaternion targetRotation = Quaternion.Euler(xAngle, yAngle, 0f);
                        InGame_MainCinemachineCameraParent.transform.rotation = Quaternion.Slerp(InGame_MainCinemachineCameraParent.transform.rotation, targetRotation, Time.deltaTime * 10f);

                        isCamManuallyRotating = false;
                    }
                    else
                    {
                        if (CurrentIngameCameraMode == InGameCameraMode.LockIn || CurrentIngameCameraMode == InGameCameraMode.Spectator)
                        {
                            if (isRotateInput)
                                return;

                            var f = NetworkManager_Client.Instance.GetFramePredicted();

                            Vector3 targetPosition = Vector3.zero;

                            var ball = InGame_Quantum.Instance.ball;

                            if (f == null || ball == null)
                                return;

                            if (f.Unsafe.TryGetPointer<Quantum.BallRules>(ball.enityRef, out Quantum.BallRules* ballRules))
                            {
                                if (ballRules->TargetEntity == InGameCamera_TargetPlayerInfo.entityRef) //내가 타겟이면 나를 공격한 놈 바라보자
                                {
                                    if (f.Unsafe.TryGetPointer<Quantum.Transform3D>(ballRules->PreviousEntity, out Quantum.Transform3D* trans))
                                        targetPosition = trans->Position.ToUnityVector3();
                                }
                                else if (ballRules->PreviousEntity == InGameCamera_TargetPlayerInfo.entityRef) //내가 공격했으면 내가 공격한 상대방 바라보자
                                {
                                    if (f.Unsafe.TryGetPointer<Quantum.Transform3D>(ballRules->TargetEntity, out Quantum.Transform3D* trans))
                                        targetPosition = trans->Position.ToUnityVector3();
                                }
                                else //나머지의 경우는 공을 바라보자
                                {
                                    if (ballRules->GetBallPosition(f).HasValue)
                                        targetPosition = ballRules->GetBallPosition(f).Value.ToUnityVector3();
                                }
                            }

                            Vector3 lookatPos = (InGameCamera_TargetPlayerGameObj.transform.position + Vector3.up * 2);
                            var meToTargetDirection = (targetPosition - lookatPos).normalized;
                            meToTargetDirection.y = 0f;

                            var camDirection = InGame_MainCinemachineCameraChild.transform.forward;
                            camDirection.y = 0f;

                            Vector3 crossProduct = Vector3.Cross(meToTargetDirection, camDirection);
                            float dotProduct = Vector3.Dot(Vector3.up, crossProduct);
                            int angle = 0;
                            float speed = 10f;
                            float maxSpeed = 750f;
                            if (Vector3.Angle(meToTargetDirection, camDirection) > angle)
                            {
                                speed = Mathf.Lerp(1f, maxSpeed, Vector3.Angle(meToTargetDirection, camDirection) / 180f);
                                if (dotProduct > 0)
                                {
                                    yAngle = InGame_MainCinemachineCameraParent.transform.eulerAngles.y - speed * Time.deltaTime;
                                    var dir = new Vector3(InGame_MainCinemachineCameraParent.transform.rotation.eulerAngles.x, yAngle, 0);
                                    Quaternion targetRotation = Quaternion.Euler(dir);
                                    InGame_MainCinemachineCameraParent.transform.rotation = Quaternion.Lerp(InGame_MainCinemachineCameraParent.transform.rotation, targetRotation, Time.deltaTime * Mathf.Lerp(20f, 50f, Vector3.Angle(meToTargetDirection, camDirection) / 180f));
                                }
                                else if (dotProduct < 0)
                                {
                                    yAngle = InGame_MainCinemachineCameraParent.transform.eulerAngles.y + speed * Time.deltaTime;
                                    var dir = new Vector3(InGame_MainCinemachineCameraParent.transform.rotation.eulerAngles.x, yAngle, 0);
                                    Quaternion targetRotation = Quaternion.Euler(dir);
                                    InGame_MainCinemachineCameraParent.transform.rotation = Quaternion.Lerp(InGame_MainCinemachineCameraParent.transform.rotation, targetRotation, Time.deltaTime * Mathf.Lerp(20f, 50f, Vector3.Angle(meToTargetDirection, camDirection) / 180f));
                                }
                            }

                            xAngle = InGame_MainCinemachineCameraParent.transform.rotation.eulerAngles.x;
                            yAngle = InGame_MainCinemachineCameraParent.transform.rotation.eulerAngles.y;

                            if (xAngle > 180f)
                                xAngle -= 360f;
                            else if (xAngle < -180f)
                                xAngle += 360f;
                        }
                    }
                }
            }
        }
    }

    public void RotateCamera_Ingame(Vector2 v)
    {
        isCamManuallyRotating = true;
        isRotateInput = true;
        rotateVector = v;
    }

    public void EndRotateCamera_Ingame()
    {
        isCamManuallyRotating = false;
        isRotateInput = false;
        rotateVector = Vector2.zero;
    }

    public void ZoomInOutCamera_Ingame(float sliderValue)
    {
        //sliderValue는 0~1f
        var newOffset = MIN_OFFSET_DISTANCE + (MAX_OFFSET_DISTANCE - MIN_OFFSET_DISTANCE) * sliderValue;
        newOffset = Mathf.Clamp(newOffset, 7f, 15f);

        offsetDist_desired = newOffset;
        offsetDist_current = offsetDist_desired; //임시...

        AccountManager.Instance.SetCameraOffsetDist(newOffset);
    }

    public unsafe void ChangeIngameCameraMode(InGameCameraMode mode, int param = 0)
    {
        if (IsChangeCameraValid(mode) == false)
            return;

        //set target
        switch (mode)
        {
            case InGameCameraMode.SimpleLookAt:
            case InGameCameraMode.LockIn:
                {
                    ChangeIngameTarget_Me();

                    AccountManager.Instance.SetCameraMode(mode);
                }
                break;

            case InGameCameraMode.Spectator: //관전모드...
                {
                    ChangeIngameTarget_Other(param);
                }
                break;
        }

        CurrentIngameCameraMode = mode;
    }

    public void ChangeIngameTarget_Me()
    {
        InGameCamera_TargetPlayerInfo = InGame_Quantum.Instance.myPlayer;
        InGameCamera_TargetPlayerGameObj = InGameCamera_TargetPlayerInfo?.enityGameObj;
    }

    public unsafe void ChangeIngameTarget_Other(int param = 0)
    {
        if (InGame_Quantum.Instance == null)
            return;

        var targetList = InGame_Quantum.Instance.listOfPlayers;

        if (targetList != null && targetList.Count > 0)
        {
            var currentTargetIndex = targetList.FindIndex(x => x.Equals(InGameCamera_TargetPlayerInfo));
            if (currentTargetIndex >= 0)
            {
                var frame = NetworkManager_Client.Instance.GetFramePredicted();
                int nextIndex = currentTargetIndex;
                if (param == 1) //right click
                {
                    if (frame != null)
                    {
                        for (int i = 0; i < targetList.Count; i++)
                        {
                            if (InGameCamera_TargetPlayerInfo.Equals(targetList[nextIndex]) == false)
                            {
                                if (frame.Unsafe.TryGetPointer<PlayerRules>(targetList[nextIndex].entityRef, out var pr))
                                {
                                    if (pr->isDead == false)
                                        break;
                                }
                            }

                            if (nextIndex == targetList.Count - 1)
                                nextIndex = 0;
                            else
                                ++nextIndex;
                        }
                    }
                }
                else if (param == -1) //left click
                {
                    if (frame != null)
                    {
                        for (int i = targetList.Count - 1; i >= 0; --i)
                        {
                            if (InGameCamera_TargetPlayerInfo.Equals(targetList[nextIndex]) == false)
                            {
                                if (frame.Unsafe.TryGetPointer<PlayerRules>(targetList[nextIndex].entityRef, out var pr))
                                {
                                    if (pr->isDead == false)
                                        break;
                                }
                            }

                            if (nextIndex == 0)
                                nextIndex = targetList.Count - 1;
                            else
                                --nextIndex;
                        }
                    }
                }

                if (nextIndex >= 0 && nextIndex < targetList.Count)
                {
                    InGameCamera_TargetPlayerInfo = targetList[nextIndex];
                    InGameCamera_TargetPlayerGameObj = targetList[nextIndex].enityGameObj;
                }
            }
            else
            {
                InGameCamera_TargetPlayerInfo = targetList.FirstOrDefault();
                InGameCamera_TargetPlayerGameObj = targetList.FirstOrDefault().enityGameObj;
            }
        }
    }

    public (bool, Vector3) IsInsideCameraView(Vector3 pos)
    {
        if (MainCamera != null)
        {
            Vector3 viewPos = MainCamera.WorldToViewportPoint(pos);
            Vector3 screenPos = MainCamera.WorldToScreenPoint(pos);
            if (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0)
                return (true, screenPos);
        }
        return (false, Vector3.zero);
    }

    private unsafe bool IsChangeCameraValid(InGameCameraMode mode)
    { 
        bool isValid = true;

        var myPlayer = InGame_Quantum.Instance.myPlayer;
        if (myPlayer != null)
        {
            var frame = NetworkManager_Client.Instance.GetFramePredicted();
            if (frame != null)
            {
                if (frame.Unsafe.TryGetPointer<PlayerRules>(myPlayer.entityRef, out var pr))
                {
                    if (pr->isDead)
                    {
                        if (mode == InGameCameraMode.LockIn || mode == InGameCameraMode.SimpleLookAt)
                        {
                            //죽었는데 CameraMode 변경 불가...
                            Debug.Log("<color=red>Camera Change to " + mode + " is NOT VALID</color>");
                            return false;
                        }
                    }
                    else
                    {
                        if (mode == InGameCameraMode.Spectator)
                        {
                            //살아 있으면 Spectator로 CameraMode 변경 불가...
                            Debug.Log("<color=red>Camera Change to " + mode + " is NOT VALID</color>");
                            return false;
                        }
                    }
                }
            }
        }

        return isValid;
    }

    //외곽 지역 체크...
    //외곽 지역으로 진입시 offset 자동으로 조절 시켜주자
    private void CheckForBoundary()
    {
        if (InGameCamera_TargetPlayerGameObj == null || InGame_MainCinemachineCameraChild == null || InGame_MainCinemachineCameraParent == null)
            return;

        Vector3 center = Vector3.zero; // 중심점
        float boundarySize_full = Quantum.Custom.CommonDefine.ROOM_OUT_OF_BOUNDARY_XYZ.AsFloat;
        float boundarySize_half = Quantum.Custom.CommonDefine.ROOM_OUT_OF_BOUNDARY_XYZ_HALF.AsFloat;

        // 카메라 위치 확인
        Vector3 dir = (InGame_MainCinemachineCameraChild.transform.position - InGameCamera_TargetPlayerGameObj.transform.position).normalized;
        Vector3 cameraPosition = InGame_MainCinemachineCameraParent.transform.position + dir * offsetDist_current;

        // 경계를 넘었는지 확인
        bool isOutsideBoundary = cameraPosition.x > boundarySize_half ||
                                 cameraPosition.x < -boundarySize_half ||
                                 cameraPosition.z > boundarySize_half ||
                                 cameraPosition.z < -boundarySize_half;

        // 경계 내부에서의 위치 확인
        bool isInsideBoundary = cameraPosition.x < boundarySize_half &&
                                cameraPosition.x > -boundarySize_half &&
                                cameraPosition.z < boundarySize_half &&
                                cameraPosition.z > -boundarySize_half;

        if (isOutsideBoundary)
        {
            // 경계선 교차점 계산
            Vector3 boundaryPoint = CalculateBoundaryIntersection(center, cameraPosition, boundarySize_full);

            // 경계선에서 벗어난 점까지의 거리 계산
            float distanceToBoundary = Vector3.Distance(boundaryPoint, cameraPosition);

            // 거리 감소
            float newOffset = Mathf.Clamp(offsetDist_desired - distanceToBoundary, 1f, offsetDist_desired);
            offsetDist_current = Mathf.Lerp(offsetDist_current, newOffset, Time.deltaTime * 10f);
        }
        else if (isInsideBoundary && Vector3.Distance(cameraPosition, center) <= boundarySize_half)
        {
            // 경계 내부에서 원래 위치로 복귀
            offsetDist_current = offsetDist_desired;
        }
        else if (isInsideBoundary)
        {
            // 경계 내부에서 멀어졌을 때 거리 늘리기
            var newOffset = Mathf.Clamp(offsetDist_current + (boundarySize_half - Vector3.Distance(cameraPosition, center)), 1f, offsetDist_desired);
            offsetDist_current = Mathf.Lerp(offsetDist_current, newOffset, Time.deltaTime * 10f);
        }
    }

    Vector3 CalculateBoundaryIntersection(Vector3 center, Vector3 position, float regionSize)
    {
        Vector3 direction = (position - center).normalized;

        float halfSize = regionSize / 2;

        // 계산된 교차점 초기화
        Vector3 intersection = center;

        // x축과 z축 경계선과의 교차점 계산
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            intersection.x = (direction.x > 0) ? center.x + halfSize : center.x - halfSize;
            intersection.z = center.z + (intersection.x - center.x) * direction.z / direction.x;
        }
        else
        {
            intersection.z = (direction.z > 0) ? center.z + halfSize : center.z - halfSize;
            intersection.x = center.x + (intersection.z - center.z) * direction.x / direction.z;
        }

        return intersection;
    }

    private unsafe void OnEventPlayerEvents(EventPlayerEvents _event)
    {
        if (SceneManagerExtensions.IsSceneActive(SceneManagerExtensions.SceneType.InGame))
        {
            switch (_event.PlayerEvent)
            {
                case PlayerEvent.Event_Die:
                    {
                        if (InGameCamera_TargetPlayerInfo != null)
                        {
                            var frame = NetworkManager_Client.Instance.GetFramePredicted();

                            if (frame != null && frame.Unsafe.TryGetPointer<PlayerRules>(InGameCamera_TargetPlayerInfo.entityRef, out var pr))
                            {
                                //죽으면 자동으로 Spectate Target 바꿔주자
                                if (pr->isDead && CurrentIngameCameraMode == InGameCameraMode.Spectator)
                                {
                                    UtilityInvoker.Invoke(this, () =>
                                    {
                                        ChangeIngameTarget_Other(1);
                                    }, 3f);
                                }
                            }
                                
                        }
                    }
                    break;
            }
        }       
    }

}
