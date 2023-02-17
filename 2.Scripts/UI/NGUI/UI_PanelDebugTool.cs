using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PanelDebugTool : UI_PanelBase
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

    public GameObject ping50 = null;
    public GameObject ping100 = null;
    public GameObject ping150 = null;
    public GameObject ping200 = null;

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

        SetDepth(UI_PanelBase.Depth.High, 1000);

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

        ping50.SafeSetButton(OnClickBtn);
        ping100.SafeSetButton(OnClickBtn);
        ping150.SafeSetButton(OnClickBtn);
        ping200.SafeSetButton(OnClickBtn);
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
                            myNetworkInGameRPCManager.RPC_SetPlayerBattery(i.playerID, false, 200);
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

                ping50.SafeSetActive(false);
                ping100.SafeSetActive(false);
                ping150.SafeSetActive(false);
                ping200.SafeSetActive(false);

                ping50.SafeSetActive(false);
                ping100.SafeSetActive(false);
                ping150.SafeSetActive(false);
                ping200.SafeSetActive(false);

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


                SetDepth(UI_PanelBase.Depth.High, 1000);
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

            if (DataManager.Instance.userData != null)
            {
                if (DataManager.Instance.userData.IngameControlType == 0)
                    DataManager.Instance.userData.IngameControlType = 1;
                else if (DataManager.Instance.userData.IngameControlType == 1)
                    DataManager.Instance.userData.IngameControlType = 0;
            }
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

            PhotonNetworkManager.Instance.LeaveSession(() => { PhaseManager.Instance.ChangePhase(CommonDefine.Phase.Lobby); });
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
            /*
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Lobby)
            {
                if (Photon.Pun.PhotonNetwork.IsConnected == false)
                {
                    Debug.Log("Photon.Pun.PhotonNetwork.IsConnected == false");
                    PhotonNetworkManager.Instance.ConnectToPhotonNetwork();
                    return;
                }

                if (PhotonNetworkManager.IsConnectedToMaster == false)
                {
                    Debug.Log("PhotonNetworkManager.IsConnectedToMaster == false");
                    PhotonNetworkManager.Instance.DisconnectPhotonNetwork();
                    return;
                }

                if (isPlayBtnDelayOn == true)
                {
                    Debug.Log("isPlayBtnDelayOn == true");
                    return;
                }

                if (PhotonNetworkManager.matchSuccess == false && PhotonNetworkManager.isJoinedRoom)
                {
                    Debug.Log("PhotonNetworkManager.isJoinedRoom");

                    Photon.Pun.PhotonNetwork.LeaveRoom();
                    return;
                }

                if (PhotonNetworkManager.matchSuccess == false && Photon.Pun.PhotonNetwork.InRoom == true)
                {
                    Photon.Pun.PhotonNetwork.LeaveRoom();
                    return;
                }

#if CHEAT
                CommonDefine.isForcePlaySolo = true;
#endif

                byte maxPlayer = CommonDefine.GetMaxPlayer();
                int currentPlayers = PhotonNetworkManager.Instance.ListOfNetworkPlayers.Count;

                PhotonNetworkManager.Instance.JoinRandomRoom(CommonDefine.GAME_MAP_ID, CommonDefine.GetMaxPlayer());

                UtilityCoroutine.StartCoroutine(ref playBtnDelayCoroutine, PlayBtnDelayCoroutine(), this);

                if (sv.gameObject.activeSelf == true)
                    OnClickBtn(hideBtn);
            }
            */
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

                /*
                foreach (var i in InGameManager.Instance.ListOfFoodTrucks)
                    i.gameObject.SafeSetActive(hideBool);
                */
            }
        }
        /*
        else if (go == photonNetworkDebugBtn)
        {
            if (photonSimGUI != null && photonSimGUI.Peer != null)
            {
                photonSimGUI.Peer.IsSimulationEnabled = !photonSimGUI.Peer.IsSimulationEnabled;
                if (photonSimGUI.Peer.IsSimulationEnabled)
                {
                    ping50.SafeSetActive(true);
                    ping100.SafeSetActive(true);
                    ping150.SafeSetActive(true);
                    ping200.SafeSetActive(true);
                }
                else
                {
                    ping50.SafeSetActive(false);
                    ping100.SafeSetActive(false);
                    ping150.SafeSetActive(false);
                    ping200.SafeSetActive(false);
                }
            }
        }
        else if (go == ping50)
        {
            if (photonSimGUI != null)
            {
                photonSimGUI.Peer.IsSimulationEnabled = true;
                photonSimGUI.Peer.NetworkSimulationSettings.IncomingLag = 25;
                photonSimGUI.Peer.NetworkSimulationSettings.OutgoingLag = 25;
            }
        }
        else if (go == ping100)
        {
            if (photonSimGUI != null)
            {
                photonSimGUI.Peer.IsSimulationEnabled = true;
                photonSimGUI.Peer.NetworkSimulationSettings.IncomingLag = 50;
                photonSimGUI.Peer.NetworkSimulationSettings.OutgoingLag = 50;
            }
        }
        else if (go == ping150)
        {
            if (photonSimGUI != null)
            {
                photonSimGUI.Peer.IsSimulationEnabled = true;
                photonSimGUI.Peer.NetworkSimulationSettings.IncomingLag = 75;
                photonSimGUI.Peer.NetworkSimulationSettings.OutgoingLag = 75;
            }
        }
        else if (go == ping200)
        {
            if (photonSimGUI != null)
            {
                photonSimGUI.Peer.IsSimulationEnabled = true;
                photonSimGUI.Peer.NetworkSimulationSettings.IncomingLag = 100;
                photonSimGUI.Peer.NetworkSimulationSettings.OutgoingLag = 100;
            }
        }
        */
    } 


    private IEnumerator PlayBtnDelayCoroutine()
    {
        isPlayBtnDelayOn = true;
        yield return new WaitForSeconds(3f);
        isPlayBtnDelayOn = false;
    }

}
