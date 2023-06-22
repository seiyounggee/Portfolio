using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsCounter : MonoSingleton<FpsCounter>
{
    public float updateRateSeconds = 4.0f;
    private int frameCount = 0;
    private float deltatime = 0.0f;
    public static float fps = 0.0f;

    
    void Update()
    {
        frameCount++;
        deltatime += Time.unscaledDeltaTime;
        if (deltatime > 1.0 / updateRateSeconds)
        {
            fps = frameCount / deltatime;
            frameCount = 0;
            deltatime -= 1.0F / updateRateSeconds;
        }
    }
}
