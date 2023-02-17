using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PanelLobby_Main : UI_PanelBase
{
    [SerializeField] GameObject playBtn = null;
    [SerializeField] GameObject cancelBtn = null;

    [SerializeField] UILabel playersNumTxt = null;
    IEnumerator updatePlayersNumTxt = null;

    private bool isPlayBtnDelayOn = false;
    IEnumerator playBtnDelayCoroutine = null;

    private bool isCancelBtnDelayOn = false;
    IEnumerator cancelBtnDelayCoroutine = null;

    //TEMP?
    [SerializeField] GameObject leftBtn = null;
    [SerializeField] GameObject rightBtn = null;
    [SerializeField] UILabel playerNickname = null;
    [SerializeField] UILabel labelDebug = null;
    [SerializeField] GameObject cheatBtn = null;
    [SerializeField] GameObject profileBtn = null;

    [SerializeField] UILabel carNameLabel = null;
    [SerializeField] GameObject[] stat_01;
    [SerializeField] GameObject[] stat_02;
    [SerializeField] GameObject[] stat_03;
    [SerializeField] GameObject[] stat_04;

    private int cheatBtnClickCount = 0;

    [SerializeField] UILabel pingTxt = null;

    private void Awake()
    {
        playBtn.SafeSetButton(OnClickPlayBtn);
        cancelBtn.SafeSetButton(OnClickCancelBtn);

        leftBtn.SafeSetButton(OnClickBtn);
        rightBtn.SafeSetButton(OnClickBtn);

        cheatBtn.SafeSetButton(OnClickBtn);
        profileBtn.SafeSetButton(OnClickBtn);
    }

    private void LateUpdate()
    {
        if (pingTxt != null)
        {
            pingTxt.SafeSetText(string.Format("ping: {0} [+/-{1}]ms",
                PhotonNetworkManager.Instance.GetPing(),
                PhotonNetworkManager.Instance.GetPingVariance()));
        }
    }

    private void OnEnable()
    {
        var inGameReadyPhaseScript = PhaseManager.Instance.GetPhase<PhaseInGameReady>(CommonDefine.Phase.InGameReady);
        if (inGameReadyPhaseScript != null)
            inGameReadyPhaseScript.OnFailToFindOtherPlayers += OnFailToFindPlayersCallback;

        PhotonNetworkManager.Instance.PhotonCallback_OnPlayerJoined += PhotonCallback_OnPlayerJoined;
        PhotonNetworkManager.Instance.PhotonCallback_OnPlayerLeft += PhotonCallback_OnPlayerLeft;

        Initialize();
    }

    private void OnDisable()
    {
        var inGameReadyPhaseScript = PhaseManager.Instance.GetPhase<PhaseInGameReady>(CommonDefine.Phase.InGameReady);
        if (inGameReadyPhaseScript != null)
            inGameReadyPhaseScript.OnFailToFindOtherPlayers -= OnFailToFindPlayersCallback;

        PhotonNetworkManager.Instance.PhotonCallback_OnPlayerJoined -= PhotonCallback_OnPlayerJoined;
        PhotonNetworkManager.Instance.PhotonCallback_OnPlayerLeft -= PhotonCallback_OnPlayerLeft;
    }

    public override void Show()
    {
        base.Show();

        if (gameObject.activeSelf == true)
        {
            Initialize();
        }
    }

    public void Initialize()
    {
        isPlayBtnDelayOn = false;
        isCancelBtnDelayOn = false;
        UtilityCoroutine.StopCoroutine(ref playBtnDelayCoroutine, this);
        UtilityCoroutine.StopCoroutine(ref cancelBtnDelayCoroutine, this);
        playersNumTxt.SafeSetText("");

        playBtn.SafeSetActive(true);
        cancelBtn.SafeSetActive(false);

        cheatBtnClickCount = 0;
    }

    public void SetOutGameData()
    {
        SetCarInfo();
        SetTxt();
    }

    private void OnClickPlayBtn(GameObject go)
    {

        if (isPlayBtnDelayOn == true)
        {
            Debug.Log("isPlayBtnDelayOn == true");
            return;
        }

        if (PhotonNetworkManager.matchSuccess == false && PhotonNetworkManager.isJoinedRoom)
        {
            Debug.Log("PhotonNetworkManager.isJoinedRoom");

            return;
        }

        if (DataManager.Instance.userData != null && DataManager.Instance.userData.IsBanned)
        {
            playersNumTxt.SafeSetText("You Are Banned!");
            return;
        }

        if (PhotonNetworkManager.Instance.MyNetworkRunner != null && PhotonNetworkManager.Instance.MyNetworkRunner.IsCloudReady == false)
            return;

        int maxPlayer = CommonDefine.GetMaxPlayer();
        int currentPlayers = PhotonNetworkManager.Instance.ListOfNetworkRunners.Count;
        string msg = currentPlayers + "/" + maxPlayer;
        playersNumTxt.SafeSetText(msg);

        PhotonNetworkManager.Instance.JoinRandomSession();

        isCancelBtnDelayOn = true;

        playBtn.SafeSetActive(false);
        cancelBtn.SafeSetActive(true);

        UtilityCoroutine.StartCoroutine(ref cancelBtnDelayCoroutine, CancelBtnDelayCoroutine(), this);

#if CHEAT
        CommonDefine.isForcePlaySolo = false;
#endif

        SoundManager.Instance.PlaySound(SoundManager.SoundClip.UI_Click);
    }

    private void OnClickCancelBtn(GameObject go)
    {
        if (isCancelBtnDelayOn == true)
            return;

        if (PhotonNetworkManager.matchSuccess == true)
            return;

        if (PhotonNetworkManager.isJoinedRoom == false)
            return;

        UtilityCoroutine.StopCoroutine(ref updatePlayersNumTxt, this);

        isPlayBtnDelayOn = true;
        UtilityCoroutine.StartCoroutine(ref playBtnDelayCoroutine, PlayBtnDelayCoroutine(), this);

        playersNumTxt.SafeSetText("");

        if (PhotonNetworkManager.matchSuccess == true)
            return;

        PhotonNetworkManager.Instance.CancelSearchingRoom(()=> 
        {
            playBtn.SafeSetActive(true);
            cancelBtn.SafeSetActive(false);
        });
    }


    public void OnFailToFindPlayersCallback()
    {
        if (playBtn != null)
            playBtn.gameObject.SetActive(true);
        if (cancelBtn != null)
            cancelBtn.gameObject.SetActive(false);

        playersNumTxt.SafeSetText("");
        UtilityCoroutine.StopCoroutine(ref updatePlayersNumTxt, this);
    }

    private void PhotonCallback_OnPlayerJoined(Fusion.NetworkRunner runner, Fusion.PlayerRef player)
    {
        int maxPlayer = CommonDefine.GetMaxPlayer();
        UpdatePlayersNumTxt(maxPlayer);
    }

    private void PhotonCallback_OnPlayerLeft(Fusion.NetworkRunner runner, Fusion.PlayerRef player)
    {
        int maxPlayer = CommonDefine.GetMaxPlayer();
        UpdatePlayersNumTxt(maxPlayer);
    }

    IEnumerator PlayBtnDelayCoroutine() //1초 후에 플레이 버튼 누를수 있게...
    {
        yield return new WaitForSeconds(2f);
        isPlayBtnDelayOn = false;
    }

    IEnumerator CancelBtnDelayCoroutine() //1초 후에 취소 버튼 누를수 있게...
    {
        yield return new WaitForSeconds(2f);
        isCancelBtnDelayOn = false;
    }

    private void UpdatePlayersNumTxt(int maxPlayers)
    {
        if (playersNumTxt == null)
            return;

        playersNumTxt.text = "";

        int currentPlayers = PhotonNetworkManager.Instance.ListOfNetworkRunners.Count;
        if (PhotonNetworkManager.matchSuccess == false && currentPlayers != maxPlayers)
        {
            if (PhotonNetworkManager.isWaitingForOtherPlayers)
            {
                playersNumTxt.text = currentPlayers + "/" + maxPlayers;
            }
            else
            {
                playersNumTxt.text = "";
            }
        }
        else
        {
            playersNumTxt.text = "MATCH SUCCESS!";
        }
    }


    private void OnClickBtn(GameObject go)
    {
        if (go == leftBtn)
        {
            if (PhotonNetworkManager.isJoinedRoom || PhotonNetworkManager.isJoiningRoom || PhotonNetworkManager.isWaitingForOtherPlayers)
                return;

            var data = DataManager.Instance.userData;

            data.MyCharacterID = (int)DataManager.CHARACTER_DATA.CharacterType.One; //임시

            if (data.MyCarID > (int)DataManager.CAR_DATA.CarID.One)
                data.MyCarID = data.MyCarID - 1;
            else
                data.MyCarID = (int)DataManager.CAR_DATA.CarID.Four;

            SetCarInfo();

            if (OutGameManager.Instance.outGamePlayer != null)
                OutGameManager.Instance.outGamePlayer.SetData();

            DataManager.Instance.SaveUserData();

            SoundManager.Instance.PlaySound(SoundManager.SoundClip.UI_Click);
        }
        else if (go == rightBtn)
        {
            if (PhotonNetworkManager.isJoinedRoom || PhotonNetworkManager.isJoiningRoom || PhotonNetworkManager.isWaitingForOtherPlayers)
                return;

            var data = DataManager.Instance.userData;

            data.MyCharacterID = (int)DataManager.CHARACTER_DATA.CharacterType.One; //임시

            if (data.MyCarID < (int)DataManager.CAR_DATA.CarID.Four)
                data.MyCarID = data.MyCarID + 1;
            else
                data.MyCarID = (int)DataManager.CAR_DATA.CarID.One;

            SetCarInfo();

            if (OutGameManager.Instance.outGamePlayer != null)
                OutGameManager.Instance.outGamePlayer.SetData();

            DataManager.Instance.SaveUserData();

            SoundManager.Instance.PlaySound(SoundManager.SoundClip.UI_Click);
        }
        else if (go == cheatBtn)
        {
#if CHEAT
            cheatBtnClickCount++;

            if (cheatBtnClickCount >= 5)
            {
                var ui = PrefabManager.Instance.UI_PanelDebugTool;
                if (ui.gameObject.activeSelf == false)
                    ui.Show();
                else
                    ui.Close();

                cheatBtnClickCount = 0;
            }
#endif
        }
        else if (go == profileBtn)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Lobby)
                UIManager_NGUI.Instance.ActivateUI(UIManager_NGUI.UIType.UI_PanelNickname, Depth.High);
        }
    }

    public void SetCarInfo() //TEMP
    {
        var data = DataManager.Instance.userData;

        if (DataManager.Instance.carData.CarDataList != null)
        {
            var carId = data.MyCarID;
            var c = DataManager.Instance.carData.CarDataList.Find(x => x.carId.Equals(carId));
            if (c != null)
            {
                carNameLabel.SafeSetText(c.LOBBY_STATS_CARNAME);
                if (stat_01 != null && stat_01.Length >= 5)
                {
                    for (int i = 0; i < stat_01.Length; i++)
                    {
                        if (i <= c.LOBBY_STATS_01 - 1)
                            stat_01[i].SafeSetActive(true);
                        else
                            stat_01[i].SafeSetActive(false);
                    }
                }
                if (stat_02 != null && stat_02.Length >= 5)
                {
                    for (int i = 0; i < stat_02.Length; i++)
                    {
                        if (i <= c.LOBBY_STATS_02 - 1)
                            stat_02[i].SafeSetActive(true);
                        else
                            stat_02[i].SafeSetActive(false);
                    }
                }
                if (stat_03 != null && stat_03.Length >= 5)
                {
                    for (int i = 0; i < stat_03.Length; i++)
                    {
                        if (i <= c.LOBBY_STATS_03 - 1)
                            stat_03[i].SafeSetActive(true);
                        else
                            stat_03[i].SafeSetActive(false);
                    }
                }
                if (stat_04 != null && stat_04.Length >= 5)
                {
                    for (int i = 0; i < stat_04.Length; i++)
                    {
                        if (i <= c.LOBBY_STATS_04 - 1)
                            stat_04[i].SafeSetActive(true);
                        else
                            stat_04[i].SafeSetActive(false);
                    }
                }
            }
        }


            if (labelDebug != null)
        {
            labelDebug.text = "";
            if (DataManager.Instance.carData != null)
            {
                if (DataManager.Instance.carData.CarDataList != null)
                {
                    labelDebug.text += "CarDataList.Length: " + DataManager.Instance.carData.CarDataList.Count;
                    foreach (var i in DataManager.Instance.carData.CarDataList)
                        labelDebug.text += "\n id: " + i.carId + "    speed:" + i.PLAYER_MOVE_SPEED;
                }
                else
                    labelDebug.text = "DataManager.Instance.carData.CarDataList is null";
            }
            else
                labelDebug.text = "DataManager.Instance.carData is null";

            if (DataManager.Instance.userData != null)
            {
                labelDebug.text += "\n NickName: " + DataManager.Instance.userData.NickName;
                labelDebug.text += "\n UserId: " + DataManager.Instance.userData.UserId;
            }
            else
                labelDebug.text += "DataManager.Instance.userData is null";


        }
    }

    public void SetTxt()
    {
        var data = DataManager.Instance.userData;

        if (data != null)
        {
            playerNickname.SafeSetText(data.NickName);
        }
        else
        {
            playerNickname.SafeSetText("NULL");
        }
    }
}
