using Fusion.Photon.Realtime;
using PNIX.Engine.NetClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PanelDebugTool : UI_Base
{
    class LogMessage
    {
        public string Message;
        public LogType Type;

        public LogMessage(string msg, LogType type)
        {
            Message = msg;
            Type = type;
        }
    }

    static Queue<LogMessage> queue = new Queue<LogMessage>();

    private bool isErrorDebugOn = true;
    private bool isLogDebugOn = true;
    private bool isWarningDebugOn = true;

    private bool isNewLogAdded = false;

    int maxRows = 20;

    public UILabel logLabel = null;

    public GameObject clearBtn = null;
    public GameObject checkBtn = null;

    public GameObject hideBtn = null;
    public GameObject debugBtn = null;
    public GameObject warningBtn = null;
    public GameObject errorBtn = null;
    public GameObject debugBtn_OnOff = null;
    public GameObject warningBtn_OnOff = null;
    public GameObject errorBtn_OnOff = null;
    public GameObject exitMatchBtn = null;
    public GameObject cheatBtn = null;
    public UILabel cheatLabel = null;
    public GameObject hideMapObjBtn = null;
    public GameObject photonNetworkDebugBtn = null;
    public UILabel photonNetworkDebugBtnOnOff = null;

    public GameObject backBG = null;

    public UIScrollView sv = null;


    public GameObject playSoloBtn = null;
    private bool isPlayBtnDelayOn = false;
    IEnumerator playBtnDelayCoroutine = null;

    private bool hideBool = false;

    //public Photon.Pun.UtilityScripts.PhotonLagSimulationGui photonSimGUI = null;

    public GameObject btn1 = null;
    public GameObject btn2 = null;
    public GameObject btn3 = null;
    public GameObject btn4 = null;

    public GameObject mapIDChangeBase = null;
    public int selectedMapID = -1;
    [SerializeField] UIInput mapIDInputField = null;
    public GameObject btnPlayMapID = null;

    public UILabel labelDebug = null;

    private PhotonNetworkManager photonNetworkManager => PhotonNetworkManager.Instance;
    private NetworkInGameRPCManager myNetworkInGameRPCManager => photonNetworkManager.MyNetworkInGameRPCManager;

    void OnEnable()
    {

#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7
            Application.RegisterLogCallback(LogMessageReceived);
#else
        Application.logMessageReceived += LogMessageReceived;
#endif
    }

    void OnDisable()
    {

#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7
            Application.RegisterLogCallback(null);
#else
        Application.logMessageReceived -= LogMessageReceived;
#endif
    }

    private void Start()
    {
#if !CHEAT
        Destroy(gameObject);
#endif

        SetDepth(UI_Base.Depth.High, 1000);

        hideBtn.SafeSetButton(OnClickBtn);
        clearBtn.SafeSetButton(OnClickBtn);
        checkBtn.SafeSetButton(OnClickBtn);
        debugBtn.SafeSetButton(OnClickBtn);
        warningBtn.SafeSetButton(OnClickBtn);
        errorBtn.SafeSetButton(OnClickBtn);
        exitMatchBtn.SafeSetButton(OnClickBtn);
        cheatBtn.SafeSetButton(OnClickBtn);
        playSoloBtn.SafeSetButton(OnClickBtn);
        hideMapObjBtn.SafeSetButton(OnClickBtn);
        photonNetworkDebugBtn.SafeSetButton(OnClickBtn);
        InvokeRepeating("UpdateText", 1f, 1f);

        btn1.SafeSetButton(OnClickBtn);
        btn2.SafeSetButton(OnClickBtn);
        btn3.SafeSetButton(OnClickBtn);
        btn4.SafeSetButton(OnClickBtn);

        btnPlayMapID.SafeSetButton(OnClickBtn);

        EventDelegate.Add(mapIDInputField.onChange, OnChange_mapIDInputField);
    }

    public bool isCheatModeOn = false;
    private float cheatCounter = 0;

    private void Update()
    {
        while (queue.Count > maxRows)
            queue.Dequeue();

        if (Input.GetKeyDown(KeyCode.C))
            isCheatModeOn = !isCheatModeOn;

        if (isCheatModeOn)
        {
            if (InGameManager.Instance.gameState == InGameManager.GameState.PlayGame
                && InGameManager.Instance.ListOfPlayers != null
                && InGameManager.Instance.ListOfPlayers.Count > 0)
            {
                cheatCounter += Time.deltaTime;
                if (cheatCounter >= 1f)
                {
                    cheatCounter = 0f;

                    foreach (var i in InGameManager.Instance.ListOfPlayers)
                    {
                        if (i != null)
                            myNetworkInGameRPCManager.RPC_SetPlayerBattery(i.photonNetworkID, false, 200);
                    }
                }

            }
        }




        /*
        if (photonSimGUI != null && photonSimGUI.Peer != null)
        {
            if (photonSimGUI.Peer.IsSimulationEnabled)
            {
                photonNetworkDebugBtnOnOff.SafeSetText("PING SIMULATOIN: ON  " + "|  ping: " + PhotonNetworkManager.Instance.GetPing() + "|  avg ping: " + PhotonNetworkManager.Instance.GetAveragePing());
            }
            else
            {
                photonNetworkDebugBtnOnOff.SafeSetText("PING SIMULATOIN: OFF  " + "|  ping: " + PhotonNetworkManager.Instance.GetPing() + "|  avg ping: " + PhotonNetworkManager.Instance.GetAveragePing());
            }
        }
        */
    }

    private void OnChange_mapIDInputField()
    {
        if (mapIDInputField != null && mapIDInputField.value != null && string.IsNullOrEmpty(mapIDInputField.value) == false)
        {
            int mapID = -1;
            bool success = int.TryParse(mapIDInputField.value, out mapID);
            if (success)
            {
                if (mapID != -1 && mapID >= 0)
                {
                    selectedMapID = mapID;
                    //DataManager.Instance.SetMapID(mapID);
                }
            }
        }
    }

    private void LogMessageReceived(string message, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Error:
                if (isErrorDebugOn == false)
                    return;
                break;
            case LogType.Assert:
                break;
            case LogType.Warning:
                if (isWarningDebugOn == false)
                    return;
                break;
            case LogType.Log:
                if (isLogDebugOn == false)
                    return;
                break;
            case LogType.Exception:
                break;
        }

        if (message.Length > 70)
        {
            string newMsg = "";
            for (int i = 0; i < 70; i++)
            {
                newMsg += message[i];
            }

            message = newMsg;
        }

        string[] lines = message.Split(new char[] { '\n' });

        foreach (string l in lines)
        {
            queue.Enqueue(new LogMessage(l, type));
        }

        isNewLogAdded = true;
    }

    private void UpdateText()
    {
        if (isNewLogAdded == false)
            return;

        if (logLabel == null)
            return;

        logLabel.text = "";

        LogMessage[] queArray = queue.ToArray();

        System.Text.StringBuilder logLabelStringBuilder = new System.Text.StringBuilder(logLabel.text);
        for (int i = 0; i < queArray.Length; i++)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder("");

            switch (queArray[i].Type)
            {

                case LogType.Error:
                    if (isErrorDebugOn)
                    {
                        builder.Append("[ff0000][Error]: ");
                        builder.Append(queArray[i].Message);
                        builder.Append("[-]\n");
                        logLabelStringBuilder.Append(builder);
                    }
                    break;
                case LogType.Assert:
                    logLabel.text += "[f00000][Assert]: " + queArray[i].Message + "][-]\n";
                    break;
                case LogType.Warning:
                    if (isWarningDebugOn)
                    {
                        builder.Append("[FFFF00][Warning]: ");
                        builder.Append(queArray[i].Message);
                        builder.Append("[-]\n");
                        logLabelStringBuilder.Append(builder);
                    }
                    break;
                case LogType.Log:
                    if (isLogDebugOn)
                    {
                        builder.Append("[ffffff][Log]: ");
                        builder.Append(queArray[i].Message);
                        builder.Append("[-]\n");
                        logLabelStringBuilder.Append(builder);
                    }
                    break;
                case LogType.Exception:
                    logLabel.text += "[0000ff][Exception]: " + queArray[i].Message + "[-]\n";
                    break;
            }

        }

        logLabel.text = logLabelStringBuilder.ToString();

        isNewLogAdded = false;
    }

    private void OnClickBtn(GameObject go)
    {
        if (go == hideBtn)
        {
            if (sv.gameObject.activeSelf == true)
            {
                sv.gameObject.SafeSetActive(false);
                clearBtn.SafeSetActive(false);
                checkBtn.SafeSetActive(false);
                debugBtn.SafeSetActive(false);
                warningBtn.SafeSetActive(false);
                errorBtn.SafeSetActive(false);

                backBG.SafeSetActive(false);
                cheatBtn.SafeSetActive(false);

                exitMatchBtn.SafeSetActive(false);
                playSoloBtn.SafeSetActive(false);
                hideMapObjBtn.SafeSetActive(false);
                photonNetworkDebugBtn.SafeSetActive(false);

                btn1.SafeSetActive(false);
                btn2.SafeSetActive(false);
                btn3.SafeSetActive(false);
                btn4.SafeSetActive(false);

                mapIDChangeBase.SafeSetActive(false);

                btnPlayMapID.SafeSetActive(false);

                sv.ResetPosition();
            }
            else
            {
                sv.gameObject.SafeSetActive(true);
                clearBtn.SafeSetActive(true);
                checkBtn.SafeSetActive(true);
                debugBtn.SafeSetActive(true);
                warningBtn.SafeSetActive(true);
                errorBtn.SafeSetActive(true);

                backBG.SafeSetActive(true);
                cheatBtn.SafeSetActive(true);

                exitMatchBtn.SafeSetActive(true);
                playSoloBtn.SafeSetActive(true);
                hideMapObjBtn.SafeSetActive(true);
                photonNetworkDebugBtn.SafeSetActive(true);

                mapIDChangeBase.SafeSetActive(true);

                btnPlayMapID.SafeSetActive(true);

                btn1.SafeSetActive(true);
                btn2.SafeSetActive(true);
                btn3.SafeSetActive(true);
                btn4.SafeSetActive(true);

                /*
                if (photonSimGUI != null)
                {
                    if (photonSimGUI.Peer.IsSimulationEnabled)
                    {
                        photonNetworkDebugBtnOnOff.SafeSetText("PING SIMULATOIN: ON");
                        ping50.SafeSetActive(true);
                        ping100.SafeSetActive(true);
                        ping150.SafeSetActive(true);
                        ping200.SafeSetActive(true);
                    }
                    else
                    {
                        photonNetworkDebugBtnOnOff.SafeSetText("PING SIMULATOIN: OFF");
                        ping50.SafeSetActive(false);
                        ping100.SafeSetActive(false);
                        ping150.SafeSetActive(false);
                        ping200.SafeSetActive(false);
                    }
                }
                */


                SetDepth(UI_Base.Depth.High, 1000);
            }
        }
        else if (go == clearBtn)
        {
            queue.Clear();
            logLabel.text = "[NO LOG]";

            UpdateText();
            sv.ResetPosition();
        }
        else if (go == checkBtn)
        {
            /*
            PhaseManager.Instance.ChangePhase(CommonDefine.Phase.Initialize);
            */
            /*
            if (DataManager.Instance.userData != null)
            {
                if (DataManager.Instance.userData.IngameControlType == 0)
                    DataManager.Instance.userData.IngameControlType = 1;
                else if (DataManager.Instance.userData.IngameControlType == 1)
                    DataManager.Instance.userData.IngameControlType = 0;
            }
            */

            /*
            Debug.Log("Check~!" + PhotonAppSettings.Instance.AppSettings.BestRegionSummaryFromStorage);

            if (PhotonNetworkManager.Instance.MyRegion != null)
                Debug.Log("OnClickPlayBtn Photon Region: " + PhotonNetworkManager.Instance.MyRegion);
            */

            /*
            var boxs = PNIX.ReferenceTable.CReferenceManager.Instance.GetRefBoxs();

            if (boxs != null)
            {
                Debug.Log(boxs.Count);
                foreach (var i in boxs)
                    Debug.Log(i.Key + " | " +i.Value);
            }
            else
                Debug.Log("boxs is null");

            if (boxs.ContainsKey(1))
            {
                var refMap = boxs[1];
                if (refMap != null)
                {
                    Debug.Log(refMap.boxID);
                    Debug.Log(refMap.boxName);
                }
                
            }*/

            /*
            var _file_path = string.Format("{0}{1}{2}", DataManager.GetVersionFilePath(), ClientVersion.ResFileName, DataManager.REFSFILE_EXTENTION);

            Debug.Log(_file_path);
            */

            //Debug.Log("CountryCode: " + CNetworkManager.Instance.CountryCode); 

            Debug.Log("Application.dataPath :     " + Application.dataPath);
        }
        else if (go == debugBtn)
        {
            isLogDebugOn = !isLogDebugOn;

            UpdateText();
            sv.ResetPosition();

            if (isLogDebugOn)
                debugBtn_OnOff.SafeSetActive(true);
            else
                debugBtn_OnOff.SafeSetActive(false);
        }
        else if (go == warningBtn)
        {
            isWarningDebugOn = !isWarningDebugOn;

            UpdateText();
            sv.ResetPosition();

            if (isWarningDebugOn)
                warningBtn_OnOff.SafeSetActive(true);
            else
                warningBtn_OnOff.SafeSetActive(false);
        }
        else if (go == errorBtn)
        {
            isErrorDebugOn = !isErrorDebugOn;

            UpdateText();
            sv.ResetPosition();

            if (isErrorDebugOn)
                errorBtn_OnOff.SafeSetActive(true);
            else
                errorBtn_OnOff.SafeSetActive(false);
        }
        else if (go == exitMatchBtn)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGameResult)
                return;

            if (InGameManager.Instance.gameState != InGameManager.GameState.PlayGame)
                return;

            return;


        }
        else if (go == cheatBtn)
        {
            isCheatModeOn = !isCheatModeOn;

            if (isCheatModeOn)
                cheatLabel.text = "Cheat Mode: On";
            else
            {
                cheatLabel.text = "Cheat Mode: Off";
            }
        }
        else if (go == playSoloBtn)
        {
            return;
        }
        else if (go == hideMapObjBtn)
        {
            hideBool = !hideBool;

            if (hideBool)
            {
                foreach (var i in MapObjectManager.Instance.chargePadList)
                    i.gameObject.SafeSetActive(hideBool);

                foreach (var i in MapObjectManager.Instance.chargeZoneList)
                    i.gameObject.SafeSetActive(hideBool);

                foreach (var i in MapObjectManager.Instance.containerBoxList)
                    i.gameObject.SafeSetActive(hideBool);
            }
        }
        else if (go == btnPlayMapID)
        {
            if (selectedMapID != -1)
            {
                var ui = PrefabManager.Instance.UI_PanelLobby_Main;
                ui.DeactivatePlayBtn();
                DataManager.Instance.SetMapID(selectedMapID);
                PnixNetworkManager.Instance.SendJoinRaceReq(PhotonNetworkManager.Instance.MyRegion, selectedMapID, (byte)DataManager.GetMaxPlayer());
            }
            else
                Debug.Log("Not Valid Map ID");
        }
        else if (go == btn1)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGame)
            {
                if (InGameManager.Instance.myPlayer != null)
                    InGameManager.Instance.myPlayer.ForceFinishLap();
            }
        }
        else if (go == btn2)
        {

        }
        else if (go == btn3)
        {

        }
        else if (go == btn4)
        {

        }
    } 


    private IEnumerator PlayBtnDelayCoroutine()
    {
        isPlayBtnDelayOn = true;
        yield return new WaitForSeconds(3f);
        isPlayBtnDelayOn = false;
    }

}
