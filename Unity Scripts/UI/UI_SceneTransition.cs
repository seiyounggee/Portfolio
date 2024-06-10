using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SceneTransition : UIBase
{
    [SerializeField] Animator animator;

    public const string ANIM_TRIGGER_IN = "In";
    public const string ANIM_TRIGGER_OUT = "Out";

    public void Show_In()
    {
        transform.SetAsLastSibling();
        gameObject.SafeSetActive(true);
        animator?.SetTrigger(ANIM_TRIGGER_IN);
    }

    public void Show_Out()
    {
        if (gameObject.SafeIsActive())
        {
            transform.SetAsLastSibling();
            animator?.SetTrigger(ANIM_TRIGGER_OUT);
            UtilityInvoker.Invoke(this, () => gameObject.SafeSetActive(false), 1f);
        }
    }
}
