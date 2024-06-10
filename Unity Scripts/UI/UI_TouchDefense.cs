using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_TouchDefense : UIBase
{
    public override void Show()
    {
        base.Show();

        transform.SetAsLastSibling();
    }
}
