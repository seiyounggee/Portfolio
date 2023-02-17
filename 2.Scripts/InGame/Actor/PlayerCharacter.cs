using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    #region ANIMATION PARAMETERS
    public const string ANIM_CHARACTER_01_IDLE = "ANIM_CHARACTER_01_IDLE";
    public const string ANIM_CHARACTER_01_DRIVE = "ANIM_CHARACTER_01_DRIVE";
    public const string ANIM_CHARACTER_01_LEFT = "ANIM_CHARACTER_01_LEFT";
    public const string ANIM_CHARACTER_01_RIGHT = "ANIM_CHARACTER_01_RIGHT";
    public const string ANIM_CHARACTER_01_BOOSTER_1 = "ANIM_CHARACTER_01_BOOSTER_1";
    public const string ANIM_CHARACTER_01_BOOSTER_2 = "ANIM_CHARACTER_01_BOOSTER_2";
    public const string ANIM_CHARACTER_01_BRAKE = "ANIM_CHARACTER_01_BRAKE";
    public const string ANIM_CHARACTER_01_FLIP = "ANIM_CHARACTER_01_FLIP";
    public const string ANIM_CHARACTER_01_VICTORY = "ANIM_CHARACTER_01_VICTORY";
    public const string ANIM_CHARACTER_01_COMPLETE = "ANIM_CHARACTER_01_COMPLETE";
    public const string ANIM_CHARACTER_01_RETIRE = "ANIM_CHARACTER_01_RETIRE";
    public const string ANIM_CHARACTER_01_SPIN = "ANIM_CHARACTER_01_SPIN";

    #endregion
    [System.Serializable]
    public class Character
    {
        public DataManager.CHARACTER_DATA.CharacterType characterType;
        public GameObject go = null;
        public Animator animator = null;
        public Transform headPosition = null;
    }

    [SerializeField] public List<Character> CharacterList = new List<Character>();
    [ReadOnly] public Character currentCharacter = new Character();

    //TEMP
    [SerializeField] public List<GameObject> headObj = new List<GameObject>();

    public void SetCharacter(DataManager.CHARACTER_DATA.CharacterType type)
    {
        var c = CharacterList.Find(x => x.characterType.Equals(type));
        if (c != null)
        {
            currentCharacter = c;

            foreach (var i in CharacterList)
            {
                if (i.go.Equals(currentCharacter.go))
                    i.go.SafeSetActive(true);
                else
                    i.go.SafeSetActive(false);
            }
        }

        foreach (var i in headObj)
        {
            i.SafeSetActive(false);
        }
    }

    public void SetHeadObj(int id)
    {
        //TEMP TEMP!!!!!!!!!!
        if (headObj != null && headObj.Count > 0)
        {
            int equipIndex = id;

            for (int i = 0; i < headObj.Count; i++)
            {
                if (equipIndex == i && equipIndex >= 0 && equipIndex < headObj.Count)
                {
                    headObj[i].SafeSetActive(true);
                    if (currentCharacter != null & currentCharacter.headPosition != null)
                        headObj[i].transform.parent = currentCharacter.headPosition;
                }
                else
                {
                    headObj[i].SafeSetActive(false);
                }
            }

        }
    }



    public string GetString_ANIM_IDLE()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            switch (currentCharacter.characterType)
            {
                case DataManager.CHARACTER_DATA.CharacterType.One:
                    animName = ANIM_CHARACTER_01_IDLE;
                    break;
                case DataManager.CHARACTER_DATA.CharacterType.Two:
                    
                    break;
                case DataManager.CHARACTER_DATA.CharacterType.Three:
                    
                    break;
                case DataManager.CHARACTER_DATA.CharacterType.Four:
                    
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_DRIVE()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            switch (currentCharacter.characterType)
            {
                case DataManager.CHARACTER_DATA.CharacterType.One:
                    animName = ANIM_CHARACTER_01_DRIVE;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_LEFT()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            switch (currentCharacter.characterType)
            {
                case DataManager.CHARACTER_DATA.CharacterType.One:
                    animName = ANIM_CHARACTER_01_LEFT;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_RIGHT()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            switch (currentCharacter.characterType)
            {
                case DataManager.CHARACTER_DATA.CharacterType.One:
                    animName = ANIM_CHARACTER_01_RIGHT;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_BOOSTER_1()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            switch (currentCharacter.characterType)
            {
                case DataManager.CHARACTER_DATA.CharacterType.One:
                    animName = ANIM_CHARACTER_01_BOOSTER_1;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_BOOSTER_2()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            switch (currentCharacter.characterType)
            {
                case DataManager.CHARACTER_DATA.CharacterType.One:
                    animName = ANIM_CHARACTER_01_BOOSTER_2;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_BRAKE()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            switch (currentCharacter.characterType)
            {
                case DataManager.CHARACTER_DATA.CharacterType.One:
                    animName = ANIM_CHARACTER_01_BRAKE;
                    break;
            }
        }

        return animName;
    }


    public string GetString_ANIM_FLIP()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            switch (currentCharacter.characterType)
            {
                case DataManager.CHARACTER_DATA.CharacterType.One:
                    animName = ANIM_CHARACTER_01_FLIP;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_VICTORY()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            switch (currentCharacter.characterType)
            {
                case DataManager.CHARACTER_DATA.CharacterType.One:
                    animName = ANIM_CHARACTER_01_VICTORY;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_COMPLETE()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            switch (currentCharacter.characterType)
            {
                case DataManager.CHARACTER_DATA.CharacterType.One:
                    animName = ANIM_CHARACTER_01_COMPLETE;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_RETIRE()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            switch (currentCharacter.characterType)
            {
                case DataManager.CHARACTER_DATA.CharacterType.One:
                    animName = ANIM_CHARACTER_01_RETIRE;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_SPIN()
    {
        string animName = "";

        if (currentCharacter != null)
        {
            switch (currentCharacter.characterType)
            {
                case DataManager.CHARACTER_DATA.CharacterType.One:
                    animName = ANIM_CHARACTER_01_SPIN;
                    break;
            }
        }

        return animName;
    }

}
