using PNIX.ReferenceTable;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    #region ANIMATION PARAMETERS
    public const string ANIM_CHARACTER_IDLE = "ANIM_CHARACTER_IDLE";
    public const string ANIM_CHARACTER_DRIVE = "ANIM_CHARACTER_DRIVE";
    public const string ANIM_CHARACTER_LEFT = "ANIM_CHARACTER_LEFT";
    public const string ANIM_CHARACTER_RIGHT = "ANIM_CHARACTER_RIGHT";
    public const string ANIM_CHARACTER_BOOSTER_1 = "ANIM_CHARACTER_BOOSTER_1";
    public const string ANIM_CHARACTER_BOOSTER_2 = "ANIM_CHARACTER_BOOSTER_2";
    public const string ANIM_CHARACTER_BRAKE = "ANIM_CHARACTER_BRAKE";
    public const string ANIM_CHARACTER_FLIP = "ANIM_CHARACTER_FLIP";
    public const string ANIM_CHARACTER_VICTORY = "ANIM_CHARACTER_VICTORY";
    public const string ANIM_CHARACTER_COMPLETE = "ANIM_CHARACTER_COMPLETE";
    public const string ANIM_CHARACTER_RETIRE = "ANIM_CHARACTER_RETIRE";
    public const string ANIM_CHARACTER_SPIN = "ANIM_CHARACTER_SPIN";

    #endregion

    [SerializeField] public List<PlayerCharacter_Prefab> CharacterList = new List<PlayerCharacter_Prefab>();
    [ReadOnly] public PlayerCharacter_Prefab currentCharacter = null;

    public void SetCharacter(CRefCharacter refChar)
    {
        foreach (var i in CharacterList)
            Destroy(i.gameObject);
        CharacterList.Clear();

        var prefabID = refChar.prefabID;



        var asset = AssetManager.Instance.loadedPrefabAssets.Find(x => x.prefab != null && x.prefab.name.Contains(prefabID));
        if (asset != null)
        {
#if UNITY_EDITOR
            GameObject asssetGo = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/DownloadAsset/Character/" + asset.prefab.name + ".prefab", typeof(GameObject));
            var go = Instantiate(asssetGo, this.transform);
#else
            var go = Instantiate(asset.prefab, this.transform);
#endif


            go.transform.localScale = Vector3.one;
            var script = go.GetComponent<PlayerCharacter_Prefab>();
            CharacterList.Add(script);
            currentCharacter = script;
        }

        if (currentCharacter != null)
        {
            currentCharacter.dataInfo = refChar;
            currentCharacter.SetMaterial();
            currentCharacter.go.gameObject.SafeSetActive(true);
        }
    }

    public string GetString_ANIM_IDLE()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            animName = ANIM_CHARACTER_IDLE;
        }

        return animName;
    }

    public string GetString_ANIM_DRIVE()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            animName = ANIM_CHARACTER_DRIVE;
        }

        return animName;
    }

    public string GetString_ANIM_LEFT()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            animName = ANIM_CHARACTER_LEFT;
        }

        return animName;
    }

    public string GetString_ANIM_RIGHT()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            animName = ANIM_CHARACTER_RIGHT;
        }

        return animName;
    }

    public string GetString_ANIM_BOOSTER_1()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            animName = ANIM_CHARACTER_BOOSTER_1;
        }

        return animName;
    }

    public string GetString_ANIM_BOOSTER_2()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            animName = ANIM_CHARACTER_BOOSTER_2;
        }

        return animName;
    }

    public string GetString_ANIM_BRAKE()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            animName = ANIM_CHARACTER_BRAKE;
        }

        return animName;
    }


    public string GetString_ANIM_FLIP()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            animName = ANIM_CHARACTER_FLIP;
        }

        return animName;
    }

    public string GetString_ANIM_VICTORY()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            animName = ANIM_CHARACTER_VICTORY;
        }

        return animName;
    }

    public string GetString_ANIM_COMPLETE()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            animName = ANIM_CHARACTER_COMPLETE;
        }

        return animName;
    }

    public string GetString_ANIM_RETIRE()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            animName = ANIM_CHARACTER_RETIRE;
        }

        return animName;
    }

    public string GetString_ANIM_SPIN()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            animName = ANIM_CHARACTER_SPIN;
        }

        return animName;
    }

}
