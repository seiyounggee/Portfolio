using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SubCamera_Base : MonoBehaviour
{
    [SerializeField] public Camera subCam = null;
    [SerializeField] public Animator animator = null;

    public enum AnimationType
    { 
        None,
        PlayIntro,
        PlayOutro_Ceremony,
    }

    private const string ANIM_NAME_INTRO = "isPlayIntro";
    private const string ANIM_NAME_OUTRO_CEREMONY = "isPlayOutroCeremony";


    public void Awake()
    {
        if (subCam == null)
            subCam = GetComponent<Camera>();

        if(animator == null)
            animator = GetComponent<Animator>();
    }

    public void OnEnable()
    {
        if (UIRoot_Base.Instance.uiCam != null && subCam != null)
        {
            var cameraData = subCam.GetUniversalAdditionalCameraData();

            if (cameraData.cameraStack.Contains(UIRoot_Base.Instance.uiCam) == false)
                cameraData.cameraStack.Add(UIRoot_Base.Instance.uiCam);
        }
    }


    public void SetAnimation(AnimationType cameraType)
    {
        if (animator == null)
        {
            Debug.Log("Error...! animator is null");
            return;
        }

        InitializeAllAnimation();

        switch (cameraType)
        {
            case AnimationType.None:
                break;
            case AnimationType.PlayIntro:
                {
                    animator.SetBool(ANIM_NAME_INTRO, true);
                }
                break;
            case AnimationType.PlayOutro_Ceremony:
                {
                    animator.SetBool(ANIM_NAME_OUTRO_CEREMONY, true);
                }
                break;
            default:
                break;
        }
    }

    private void FixedUpdate()
    {
        if (subCam == null)
            return;

        if (CameraManager.Instance.currentCamPriority != CameraManager.CamPriority.Sub)
            subCam.enabled = false;
        else
            subCam.enabled = true;
    }

    public void InitializeAllAnimation()
    {
        if (animator == null)
            return;

        animator.SetBool(ANIM_NAME_INTRO, false);
        animator.SetBool(ANIM_NAME_OUTRO_CEREMONY, false);
    }
}
