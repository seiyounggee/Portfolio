using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhaseInitialize : PhaseBase
{
    IEnumerator phaseInitializeCoroutine;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        UtilityCoroutine.StartCoroutine(ref phaseInitializeCoroutine, PhaseInitializeCoroutine(), this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

    }

    IEnumerator PhaseInitializeCoroutine()
    {

        yield return null;

        //Load Local Data...
        //Load Ref
        //Load DB
        //Login

        UIManager.Instance.HideGroup_All();

        Application.runInBackground = true;
        Application.targetFrameRate = 60;

        StringManager.SetInitialLanguge();

        QuantumRunner.Init(true);
        FirebaseManager.Instance.Init();

        UI_Title.Activate("Setting up FireBase");
        FirebaseManager.Instance.SetupFirebase();

        while (FirebaseManager.Instance.IsFirebaseSetup == false)
            yield return null;

        UI_Title.Activate("Logging in to FireBase Auth");
        FirebaseManager.Instance.LoginToFirebaseAuthentication();

        while (FirebaseManager.Instance.IsFirebaseLogin == false)
            yield return null;

        UI_Title.Activate("Loading Database Reference Data");
        ReferenceManager.Instance.LoadData_FirebaseServer();

        //Ref Data 먼저 받아야함...! Account 초기 설정 할 경우 Ref Data 가져옴!
        while (true)
        {
            if (ReferenceManager.Instance.IsReferenceDataLoaded)
                break;

            yield return null;
        }
        ReferenceManager.Instance.InitializeFirebaseCallback();

        UI_Title.Activate("Loading Database Account Data");
        AccountManager.Instance.LoadMyData_FirebaseServer();
        RankingManager.Instance.LoadData_FirebaseServer(); //랭킹 데이터 미리 1번 바다

        //데이터가 없으면 게임 실행x...
        //온라인 상태로 Data를 받지 않으면 여기서 빠져나가지 못함!!
        while (true)
        {
            if (AccountManager.Instance.IsAccountDataLoaded)
                break;

            yield return null;
        }

        RankingManager.Instance.SetAndSaveMyRankingData_Login();

        AccountManager.Instance.InitializeFirebaseCallback();
        FirebaseManager.Instance.LoadFirebaseServerTime(); //서버 시간 세팅해주자...

        ResourceManager.Instance.LoadResourceData();

        UI_Title.Activate("Checking Client Q Version");
        //클라이언트 버전 퀀텀 버전이 맞는 경우에만 플레이 시키자...
        if (CommonDefine.ClientVersion == ReferenceManager.Instance.AppDefines.ClientVersion
            && CommonDefine.QuantumVersion == ReferenceManager.Instance.AppDefines.PhotonQuantumVersion)
        { 
            //성공... 통과
        }
        else
        {
            Debug.Log("<color=red>Version Error..>!</color>");
            if (CommonDefine.ClientVersion != ReferenceManager.Instance.AppDefines.ClientVersion)
                Debug.Log("build CV: " + CommonDefine.ClientVersion + " | server  CV: " + ReferenceManager.Instance.AppDefines.ClientVersion);

            if (CommonDefine.QuantumVersion != ReferenceManager.Instance.AppDefines.PhotonQuantumVersion)
                Debug.Log("build QV: " + CommonDefine.QuantumVersion + " | server  QV: " + ReferenceManager.Instance.AppDefines.PhotonQuantumVersion);

            UI_Title.Activate("Client Q Version Error...!");

            //다른 경우 에러.......
            //게임 실행 x
            while (true)
            {
                yield return null;
            }
        }

        if (ReferenceManager.Instance.AppDefines.IsMaintenanceTime == true)
        {
            UI_Title.Activate("IsMaintenanceTime >> true");

            while (true)
            {
                yield return null;
            }
        }

        yield return new WaitForSeconds(1f);

        PhaseManager.Instance.ChangePhase(CommonDefine.Phase.OutGame);
    }
}
