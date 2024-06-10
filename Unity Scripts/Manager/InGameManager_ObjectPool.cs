using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class InGameManager
{
    [Serializable]
    public class PooledObject
    {
        public string name;
        public PooledType type;
        public GameObject obj;
        public Unity_Effect effect;
        public bool isActivated { get { if (obj != null && obj.activeSelf == true) return true; else return false; } }
    }

    private GameObject poolBase = null;
    [ReadOnly] public List<PooledObject> pooledObjList = new List<PooledObject>();

    public enum PooledType 
    {
        None,
        Effect_Slash,
        Effect_PickupExplodeYellow,
        Effect_MagicShieldBlue,
        Effect_ShieldSoftPurple,
        Effect_SoftRadialPunchMedium,
        Effect_SoulGenericDeath,
        Effect_StunExplosion,
        Effect_SwordHitBlueCritical,
        Effect_TargetHitExplosion,
        Effect_Frost,
        Effect_MagicBuffYellow,
        Effect_MagicEnchantYellow,
        Effect_SwordWaveWhite,
        Effect_MagicCircleSimpleYellow,
        Effect_LaserBeamYellow,
        Effect_LaserBeamRed,
        Effect_LightningOrbSharpPink,
        Effect_StormMissile,
        Effect_MysticMissileDark,
    }

    private const int INITIAL_POOL_NUMBER = 10;
    private const int ADDITIONAL_POOL_NUMBER = 5;

    private void SetPoolGameObjects(bool isAdditionalPool = false)
    {
        InitializePoolList();

        if (isAdditionalPool == false) //게임 시작 최초 1번만 실행
        {
            //Inspector창에서 정리하기위해... 
            poolBase = new GameObject();
            poolBase.transform.position = Vector3.zero;
            poolBase.name = "ObjectPooledList_BASE";
        }

        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_Slash, PooledType.Effect_Slash);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_PickupExplodeYellow, PooledType.Effect_PickupExplodeYellow);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_MagicShieldBlue, PooledType.Effect_MagicShieldBlue, 1);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_ShieldSoftPurple, PooledType.Effect_ShieldSoftPurple, 1);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_SoftRadialPunchMedium, PooledType.Effect_SoftRadialPunchMedium, 1);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_SoulGenericDeath, PooledType.Effect_SoulGenericDeath);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_StunExplosion, PooledType.Effect_StunExplosion, 1);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_SwordHitBlueCritical, PooledType.Effect_SwordHitBlueCritical);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_TargetHitExplosion, PooledType.Effect_TargetHitExplosion, 1);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_Frost, PooledType.Effect_Frost, CommonDefine.DEFAULT_MAX_PLAYER);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_MagicBuffYellow, PooledType.Effect_MagicBuffYellow, CommonDefine.DEFAULT_MAX_PLAYER);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_MagicEnchantYellow, PooledType.Effect_MagicEnchantYellow, CommonDefine.DEFAULT_MAX_PLAYER);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_SwordWaveWhite, PooledType.Effect_SwordWaveWhite, CommonDefine.DEFAULT_MAX_PLAYER);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_MagicCircleSimpleYellow, PooledType.Effect_MagicCircleSimpleYellow, CommonDefine.DEFAULT_MAX_PLAYER);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_LaserBeamYellow, PooledType.Effect_LaserBeamYellow, CommonDefine.DEFAULT_MAX_PLAYER);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_LaserBeamRed, PooledType.Effect_LaserBeamRed, CommonDefine.DEFAULT_MAX_PLAYER);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_LightningOrbSharpPink, PooledType.Effect_LightningOrbSharpPink, 1);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_StormMissile, PooledType.Effect_StormMissile, 1);
        PoolObj(isAdditionalPool, PrefabManager.Instance.Effect_MysticMissileDark, PooledType.Effect_MysticMissileDark, 1);
    }

    public void InitializePoolList()
    {
        if (pooledObjList != null)
            pooledObjList.Clear();

        pooledObjList = new List<PooledObject>();

        if (poolBase != null)
            Destroy(poolBase);
    }


    private void PoolObj(bool isAdditionalPool, GameObject prefab, PooledType poolType, int poolCount = 0)
    {
        if (prefab == null)
        {
            Debug.Log("Error.... cant find prefab: " + prefab);
            return;
        }

        if (poolCount == 0)
        {
            if (isAdditionalPool == false)
            {
                poolCount = INITIAL_POOL_NUMBER; //최초 Pool
            }
            else
            {
                poolCount = ADDITIONAL_POOL_NUMBER;  //이후 추가 Pool
            }
        }
        else
        {
            if (poolCount <= 0) //혹시나해서..
                poolCount = ADDITIONAL_POOL_NUMBER;
        }

        if (pooledObjList == null)
            return;

        for (int i = 0; i <= poolCount; i++)
        {
            var go = PrefabManager.InstantiateInGamePrefab(prefab, Vector3.zero);
            go.gameObject.SafeSetActive(false);
            go.gameObject.name = go.gameObject.name + "__" + (pooledObjList.Count); ;

            if (poolBase != null)
                go.gameObject.transform.SetParent(poolBase.transform);

            //SetActiveFalse 이후에 다시 poolBase로 되돌리자...!
            Unity_Effect effectScript = null;
            if (go.GetComponent<Unity_Effect>() == null)
            {
                effectScript = go.AddComponent<Unity_Effect>();
                effectScript.SetCallback(() => { if (go != null && poolBase != null) go.transform.SetParent(poolBase.transform); });
            }
            else
            {
                effectScript = go.GetComponent<Unity_Effect>();
                effectScript.SetCallback(() => { if (go != null && poolBase != null) go.transform.SetParent(poolBase.transform); });
            }

            pooledObjList.Add(new PooledObject() { name = go.name, type = poolType, obj = go, effect = effectScript });
        }
    }


    public GameObject ActivatePooledObj(PooledType type, Vector3 posi, Quaternion rotation, float scale = 1f, Transform parentTransform = null)
    {
        if (pooledObjList != null && pooledObjList.Count > 0)
        {
            int counter = 0;
            foreach (var i in pooledObjList)
            {
                if (i.type == type && i.obj != null && i.obj.activeSelf == false)
                {
                    i.obj.transform.position = posi;
                    i.obj.transform.rotation = rotation;
                    i.obj.transform.localScale = Vector3.one * scale;
                    i.obj.SafeSetActive(true);

                    if (parentTransform == null)
                        i.obj.transform.SetParent(poolBase.transform);
                    else
                        i.obj.transform.SetParent(parentTransform);

                    return i.obj;
                }

                //마지막 index
                if (counter == pooledObjList.Count - 1)
                {
                    if (i.type != type || (i.obj != null && i.obj.activeSelf == true))
                    {
                        //ExtraPool
                        switch (type)
                        {
                            case PooledType.Effect_Slash:
                                PoolObj(true, PrefabManager.Instance.Effect_Slash, type);
                                break;
                            case PooledType.Effect_PickupExplodeYellow:
                                PoolObj(true, PrefabManager.Instance.Effect_PickupExplodeYellow, type);
                                break;
                            case PooledType.Effect_MagicShieldBlue:
                                PoolObj(true, PrefabManager.Instance.Effect_MagicShieldBlue, type);
                                break;
                            case PooledType.Effect_ShieldSoftPurple:
                                PoolObj(true, PrefabManager.Instance.Effect_ShieldSoftPurple, type);
                                break;
                            case PooledType.Effect_SoftRadialPunchMedium:
                                PoolObj(true, PrefabManager.Instance.Effect_SoftRadialPunchMedium, type);
                                break;
                            case PooledType.Effect_SoulGenericDeath:
                                PoolObj(true, PrefabManager.Instance.Effect_SoulGenericDeath, type);
                                break;
                            case PooledType.Effect_StunExplosion:
                                PoolObj(true, PrefabManager.Instance.Effect_StunExplosion, type);
                                break;
                            case PooledType.Effect_SwordHitBlueCritical:
                                PoolObj(true, PrefabManager.Instance.Effect_SwordHitBlueCritical, type);
                                break;
                            case PooledType.Effect_TargetHitExplosion:
                                PoolObj(true, PrefabManager.Instance.Effect_TargetHitExplosion, type);
                                break;
                            case PooledType.Effect_Frost:
                                PoolObj(true, PrefabManager.Instance.Effect_Frost, type);
                                break;
                            case PooledType.Effect_MagicBuffYellow:
                                PoolObj(true, PrefabManager.Instance.Effect_MagicBuffYellow, type);
                                break;
                            case PooledType.Effect_MagicEnchantYellow:
                                PoolObj(true, PrefabManager.Instance.Effect_MagicEnchantYellow, type);
                                break;
                            case PooledType.Effect_SwordWaveWhite:
                                PoolObj(true, PrefabManager.Instance.Effect_SwordWaveWhite, type);
                                break;
                            case PooledType.Effect_MagicCircleSimpleYellow:
                                PoolObj(true, PrefabManager.Instance.Effect_MagicCircleSimpleYellow, type);
                                break;
                            case PooledType.Effect_LaserBeamYellow:
                                PoolObj(true, PrefabManager.Instance.Effect_LaserBeamYellow, type);
                                break;
                            case PooledType.Effect_LaserBeamRed:
                                PoolObj(true, PrefabManager.Instance.Effect_LaserBeamRed, type);
                                break;
                            case PooledType.Effect_LightningOrbSharpPink:
                                PoolObj(true, PrefabManager.Instance.Effect_LightningOrbSharpPink, type);
                                break;
                            case PooledType.Effect_StormMissile:
                                PoolObj(true, PrefabManager.Instance.Effect_StormMissile, type);
                                break;
                            case PooledType.Effect_MysticMissileDark:
                                PoolObj(true, PrefabManager.Instance.Effect_MysticMissileDark, type);
                                break;
                            default:
                                break;
                        }

                        break;
                    }
                }

                ++counter;
            }
        }

        return null;
    }
}
