using UnityEngine;
using System.Collections;

//Time 을 공용으로 제어하기 위해 만듦
//Time.time 대신에 TimeHelper.time 을 사용

public class TimeHelper
{
	public static float time
	{ get { return Time.time; } }

	public static float realtimeSinceStartup
	{ get { return Time.realtimeSinceStartup; } }

	public static float updateDeltaTime
	{ get { return Time.deltaTime; } }

    /* 타임스케일 영향 받도록 */
    public static float updateFixedDeltaTime
    { get { return ( Time.fixedDeltaTime * Time.timeScale ); } }
        

	public static float fixedDeltaTime
	{ get { return Time.fixedDeltaTime; } }

    public static float CheckDeltaTime
    {
        get 
        {
            if( Time.timeScale >= 1f )
                return Time.deltaTime;
            else
                return Time.unscaledDeltaTime;
        }
    }

	public static float timeScale
	{ get { return Time.timeScale; } }

	public static float DurationTime(float _time)
	{
		return time - _time;
	}

	public static void SlowTime( float scale )
	{
        if( scale > 1f )
            scale = 1f;
        else if( scale < 0f )
            scale = 0f;

        Time.timeScale = scale;
	}

    public static void SpeedUpTime(float scale)
    {
        float scaleLimit = 10f;
        if (scale < 1f)
            scale = 1f;
        else if (scale < scaleLimit)
            Time.timeScale = scale;
        else
        {
            Time.timeScale = scaleLimit;
            Debug.LogWarning("Time Scale Limit: " + scaleLimit + " Reached");
        }
    }

	public static void NormalTime()
	{
        Time.timeScale = m_NomalPlayScale;
    }

	public static void Pause()
	{
		Debug.Log("Time Pause");
		Time.timeScale = 0.0f;
	}

	public static void Resume()
	{
		Debug.Log("Time Resume");

		if (Time.timeScale != 0.0f)
			return;

		Time.timeScale = m_fTimeScale;
	}

	public static void Reset()
	{
		Debug.Log("Time Reset");
        RestoreTimeScale( true );
	}

    public static float ConvertDaysToSeconds(int days)
    {
        return days * 60 * 60 * 24;
    }

    public static void RestoreTimeScale( bool isForce = false )
    {
        m_fTimeScale = m_NomalPlayScale;
        Time.timeScale = m_fTimeScale;
    }

//    private static float m_SlowPlayScale = 0.9f;
    private static float m_NomalPlayScale = 1.0f;
    private static float m_fTimeScale = 1.0f;

    public static float NormalPlayScale { get { return m_NomalPlayScale; } }
}
