using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public partial class InGameManager
{
    public class FX_PooledObject
    {
        public string name;
        public FX_Type type;
        public GameObject obj;
        public bool isActivated { get { if (obj != null && obj.activeSelf == true) return true; else return false; } }
    }

    private GameObject poolBase = null;
    private List<FX_PooledObject> pooledFxList = new List<FX_PooledObject>();

    public enum FX_Type { None, Hit_01, Hit_02, Hit_03, Charging_Once_Blue, Charging_Once_Yellow, Charging_Once_Red, Waterdrowning }

    private const int INITIAL_POOL_NUMBER = 10;
    private const int ADDITIONAL_POOL_NUMBER = 5;

    public void PoolGameObjects(bool isAdditionalPool = false)
    {
        InitializePoolList();

        if (isAdditionalPool == false) //게임 시작 최초 1번만 실행
        {
            //Inspector창에서 정리하기위해... 
            poolBase = new GameObject();
            poolBase.transform.position = Vector3.zero;
            poolBase.name = "ObjectPooledList_BASE";
        }

        PoolFX(isAdditionalPool, PrefabManager.Instance.FX_Hit_01, FX_Type.Hit_01);
        PoolFX(isAdditionalPool, PrefabManager.Instance.FX_Hit_02, FX_Type.Hit_02);
        PoolFX(isAdditionalPool, PrefabManager.Instance.FX_Hit_03, FX_Type.Hit_03);
        PoolFX(isAdditionalPool, PrefabManager.Instance.FX_Charging_Once_Blue, FX_Type.Charging_Once_Blue);
        PoolFX(isAdditionalPool, PrefabManager.Instance.FX_Charging_Once_Yellow, FX_Type.Charging_Once_Yellow);
        PoolFX(isAdditionalPool, PrefabManager.Instance.FX_Charging_Once_Red, FX_Type.Charging_Once_Red);
        PoolFX(isAdditionalPool, PrefabManager.Instance.FX_Waterdrowning, FX_Type.Waterdrowning);
    }

    public void InitializePoolList()
    {
        if (pooledFxList != null)
            pooledFxList.Clear();

        pooledFxList = new List<FX_PooledObject>();

        if (poolBase != null)
            Destroy(poolBase);
    }


    private void PoolFX(bool isAdditionalPool, GameObject prefab, FX_Type fxType)
    {
        if (prefab == null)
        {
            Debug.Log("Error.... cant find prefab: " + prefab);
            return;
        }

        int poolCount = 0;
        if (isAdditionalPool == false)
        {
            poolCount = INITIAL_POOL_NUMBER; //최초 Pool
        }
        else
        {
            poolCount = ADDITIONAL_POOL_NUMBER;  //이후 추가 Pool
        }

        if (pooledFxList == null)
            return;

        for (int i = 0; i <= poolCount; i++)
        {
            var go = PrefabManager.InstantiateInGamePrefab(prefab, Vector3.zero);
            pooledFxList.Add(new FX_PooledObject() { name = go.name, type = fxType, obj = go });
            go.gameObject.SafeSetActive(false);
            go.gameObject.name = go.gameObject.name + "__" + (pooledFxList.Count); ;

            if (poolBase != null)
                go.gameObject.transform.SetParent(poolBase.transform);

            if (go.GetComponent<SetActiveFalseAfterTime>() == null)
                go.AddComponent<SetActiveFalseAfterTime>();
        }
    }


    public GameObject ActivateFX(FX_Type type, Vector3 posi, float scale = 1f, Transform parentTransform = null)
    {
        if (pooledFxList != null && pooledFxList.Count > 0)
        {
            foreach (var i in pooledFxList)
            {
                if (i.type == type && i.obj != null && i.obj.activeSelf == false)
                {
                    i.obj.transform.position = posi;
                    i.obj.transform.localScale = Vector3.one * scale;
                    i.obj.SafeSetActive(true);

                    if(parentTransform == null)
                        i.obj.transform.SetParent(poolBase.transform);
                    else
                        i.obj.transform.SetParent(parentTransform);

                    return i.obj;
                    break;
                }

                if (i.Equals(pooledFxList.Last()))
                {
                    if (i.type != type || (i.obj != null && i.obj.activeSelf == true))
                    {
                        //ExtraPool
                        switch (type)
                        {
                            case FX_Type.Hit_01:
                                PoolFX(true, PrefabManager.Instance.FX_Hit_01, type);
                                break;
                            case FX_Type.Hit_02:
                                PoolFX(true, PrefabManager.Instance.FX_Hit_02, type);
                                break;
                            case FX_Type.Hit_03:
                                PoolFX(true, PrefabManager.Instance.FX_Hit_03, type);
                                break;
                            case FX_Type.Charging_Once_Blue:
                                PoolFX(true, PrefabManager.Instance.FX_Charging_Once_Blue, type);
                                break;
                            case FX_Type.Charging_Once_Red:
                                PoolFX(true, PrefabManager.Instance.FX_Charging_Once_Red, type);
                                break;
                            case FX_Type.Charging_Once_Yellow:
                                PoolFX(true, PrefabManager.Instance.FX_Charging_Once_Yellow, type);
                                break;
                            case FX_Type.Waterdrowning:
                                PoolFX(true, PrefabManager.Instance.FX_Waterdrowning, type);
                                break;

                            default:
                                break;
                        }

                        break;
                    }
                }
            }
        }

        return null;
    }
}
