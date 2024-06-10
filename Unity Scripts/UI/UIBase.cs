using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    public Action OnShow = null;
    public Action OnHide = null;

    internal virtual void OnEnable()
    {
        if (transform.parent != null && transform.parent.SafeIsActive())
            transform.SetAsLastSibling();
    }

    internal virtual void OnDisable()
    {
        if (transform.parent != null && transform.parent.SafeIsActive())
            transform.SetAsFirstSibling();
    }

    public virtual void Show()
    {
        transform.SafeSetActive(true);

        OnShow?.Invoke();
    }

    public virtual void Hide()
    {
        transform.SafeSetActive(false);

        OnHide?.Invoke();
    }
}
