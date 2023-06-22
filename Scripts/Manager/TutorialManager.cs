using DTR.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoSingleton<TutorialManager>
{
    public enum TutorialStep
    { 
        Ingame_1 = 0,
        Ingame_2,
        Ingame_3, 
        Ingame_4,

        Outgame_1,
        Outgame_2,
    }

    public void SetTutorialStepInfo(GS2CProtocolData.CNotifyTutorialStep ack)
    {

    }

}
