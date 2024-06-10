using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class FirebaseWorkspace : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI topTxt;
    [SerializeField] TextMeshProUGUI statusTxt;
    [SerializeField] Button saveReferenceBtn = null;
    [SerializeField] Button saveRankBtn = null;

    [ReadOnly] public RefData referenceData = new RefData();

    private void Awake()
    {
        saveReferenceBtn.SafeSetButton(OnClickBtn);
        saveRankBtn.SafeSetButton(OnClickBtn);
    }

#if UNITY_EDITOR
    IEnumerator Start()
    { 
        yield return null;

        FirebaseManager.Instance.Init();

        statusTxt.SafeSetText("Setting up FireBase");
        Debug.Log("Setting up FireBase");
        FirebaseManager.Instance.SetupFirebase();

        while (FirebaseManager.Instance.IsFirebaseSetup == false)
            yield return null;

        statusTxt.SafeSetText("Logging in to FireBase Auth");
        Debug.Log("Logging in to FireBase Auth");
        FirebaseManager.Instance.LoginToFirebaseAuthentication();

        while (FirebaseManager.Instance.IsFirebaseLogin == false)
            yield return null;

        Debug.Log("<color=cyan>Everything is READY!!!</color>");
        statusTxt.SafeSetText("Everything is READY!!!");
        topTxt.SafeSetText("userId >> " + FirebaseManager.Instance.firebase_userId);

        referenceData = ReferenceManager.Instance.ReferenceData;
    }
#endif

    private void OnClickBtn(Button btn)
    {
        if (FirebaseManager.Instance.IsFirebaseSetup == false)
        {
            Debug.Log("<color=red>Error!!! Firebase Loading is not ready...</color>");
        }

        if (FirebaseManager.Instance.IsFirebaseLogin == false)
        {
            Debug.Log("<color=red>Error!!! Firebase Loading is not ready...</color>");
            return;
        }

        if (btn == saveReferenceBtn)
        {

#if UNITY_EDITOR
            Debug.Log("Save_ReferenceData!!!!!!");
            statusTxt.SafeSetText("Save_ReferenceData OnClick");
            UtilityInvoker.Invoke(this, () => { statusTxt.SafeSetText("Everything is READY!!!"); }, 1f);

            ReferenceManager.Instance.Save_ReferenceData();
#endif
        }
        else if (btn == saveRankBtn)
        {
            return;


        }
    }
}
