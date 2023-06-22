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

    public const string RESOURCE_SERVER_URI = "https://resource.dollytheracer.com/CheckClient";
    public const string Refs_Path = "Data/";

    public static bool IsFirstLogin = false;

#if CHEAT
    public static bool isForcePlaySolo = false;
#endif

    public const float DEFAULT_DESTROY_AFTERTIME = 3f;
    public const float DEFAULT_SETACTIVE_FALSE_AFTERTIME = 3f;

    #region TAG
    public const string TAG_NetworkPlayer = "TAG_NetworkPlayer";
    public const string TAG_NetworkPlayer_CollisionChecker = "TAG_NetworkPlayer_CollisionChecker";
    public const string TAG_NetworkPlayer_TriggerChecker = "TAG_NetworkPlayer_TriggerChecker";
    public const string TAG_MainCamera = "TAG_MainCamera";
    public const string TAG_SubCamera = "TAG_SubCamera";

    public const string TAG_ROAD_Normal = "TAG_ROAD_Normal";
    public const string TAG_OutOfBound = "TAG_OutOfBound";

    #endregion


    //Layer Name
    public const string LayerName_Default = "Default";
    public const string LayerName_Hidden = "Hidden";
    public const string LayerName_Ground = "Ground";

    #region Shader
    public const string ShaderName_DTRBasicLitShader = "DTR Basic Lit Shader";
    public const string ShaderName_DTRBasicUnlitShader = "DTR Basic Unlit Shader";
    public const string ShaderName_DTRStandardLit = "DTR_Standard_Lit";
    #endregion

    public const float DEFAULT_LANE_BY_LANE_DIST = 4.2f;

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
        return ControlType.TouchSwipe;
    }
}