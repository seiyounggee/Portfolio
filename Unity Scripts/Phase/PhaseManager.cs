using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhaseManager : MonoSingleton<PhaseManager>
{
    private List<PhaseInfo> phaseList = new List<PhaseInfo>();
    private CommonDefine.Phase previousPhase = CommonDefine.Phase.None;
    private CommonDefine.Phase currentPhase = CommonDefine.Phase.None;

    public CommonDefine.Phase CurrentPhase { get { return currentPhase; } }

    public CommonDefine.Phase PreviousPhase { get { return previousPhase; } }

    class PhaseInfo
    {
        public CommonDefine.Phase phaseType;
        public GameObject phaseGO;
    }

    public Action OnPhaseChangeCallback = null;

    public override void Awake()
    {
        base.Awake();

        SetPhaseList();
    }

    private void Start()
    {
        Scene currScene = SceneManager.GetActiveScene();

        if (currScene.name.Equals(CommonDefine.OutGameScene))
            ChangePhase(CommonDefine.Phase.Initialize);
    }

    private void SetPhaseList()
    {
        phaseList.Add(new PhaseInfo() { phaseType = CommonDefine.Phase.Initialize });
        phaseList.Add(new PhaseInfo() { phaseType = CommonDefine.Phase.OutGame });
        phaseList.Add(new PhaseInfo() { phaseType = CommonDefine.Phase.InGameReady });
        phaseList.Add(new PhaseInfo() { phaseType = CommonDefine.Phase.InGame });
        phaseList.Add(new PhaseInfo() { phaseType = CommonDefine.Phase.InGameResult });

        foreach (var p in phaseList)
        {
            GameObject go = new GameObject();
            go.SetActive(false); //이걸 해줘야 OnEnable이 작동 안함
            go.transform.parent = this.gameObject.transform;
            go.name = "Phase " + p.phaseType.ToString();
            p.phaseGO = go;

            switch (p.phaseType)
            {
                case CommonDefine.Phase.Initialize:
                    go.AddComponent<PhaseInitialize>();
                    break;

                case CommonDefine.Phase.OutGame:
                    go.AddComponent<PhaseOutGame>();
                    break;

                case CommonDefine.Phase.InGameReady:
                    go.AddComponent<PhaseInGameReady>();
                    break;

                case CommonDefine.Phase.InGame:
                    go.AddComponent<PhaseInGame>();
                    break;

                case CommonDefine.Phase.InGameResult:
                    go.AddComponent<PhaseInGameResult>();
                    break;

                default:
                    break;
            }
        }

        TurnOffAllPhases();
    }

    private void TurnOffAllPhases()
    {
        foreach (var p in phaseList)
        {
            if (p.phaseGO != null)
            {
                p.phaseGO.SetActive(false);
            }

        }
    }

    public void ChangePhase(CommonDefine.Phase nextPhase)
    {
        if (currentPhase == nextPhase)
            return;

        previousPhase = currentPhase;
        currentPhase = nextPhase;

        if (phaseList.Exists(x => x.phaseType.Equals(previousPhase)))
        {
            GameObject prevGo = phaseList.Find(x => x.phaseType.Equals(previousPhase)).phaseGO;
            if (prevGo != null)
                prevGo.SetActive(false);
        }

        if (phaseList.Exists(x => x.phaseType.Equals(currentPhase)))
        {
            GameObject currGo = phaseList.Find(x => x.phaseType.Equals(currentPhase)).phaseGO;
            if (currGo != null)
                currGo.SetActive(true);

            OnPhaseChangeCallback?.Invoke();
        }

#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=magenta>Phase Changed To</color>: <color=white>" + currentPhase.ToString() + "</color>");
#endif
    }

    public T GetPhase<T>(CommonDefine.Phase phase) where T: PhaseBase
    {
        T phaseScript = default;

        if (phaseList.Count <= 0)
            return phaseScript;

        switch (phase)
        {
            case CommonDefine.Phase.None:
                phaseScript = null;
                break;
            case CommonDefine.Phase.Initialize:
                phaseScript = (T)(object)phaseList.Find(x => x.phaseType.Equals(CommonDefine.Phase.Initialize)).phaseGO.GetComponent<PhaseInitialize>();
                break;
            case CommonDefine.Phase.OutGame:
                phaseScript = (T)(object)phaseList.Find(x => x.phaseType.Equals(CommonDefine.Phase.OutGame)).phaseGO.GetComponent<PhaseOutGame>();
                break;
            case CommonDefine.Phase.InGameReady:
                phaseScript = (T)(object)phaseList.Find(x => x.phaseType.Equals(CommonDefine.Phase.InGameReady)).phaseGO.GetComponent<PhaseInGameReady>();
                break;
            case CommonDefine.Phase.InGame:
                phaseScript = (T)(object)phaseList.Find(x => x.phaseType.Equals(CommonDefine.Phase.InGame)).phaseGO.GetComponent<PhaseInGame>();
                break;
            case CommonDefine.Phase.InGameResult:
                phaseScript = (T)(object)phaseList.Find(x => x.phaseType.Equals(CommonDefine.Phase.InGameResult)).phaseGO.GetComponent<PhaseInGameResult>();
                break;

            default:
                phaseScript = null;
                break;
        }


        return phaseScript;
    }
}