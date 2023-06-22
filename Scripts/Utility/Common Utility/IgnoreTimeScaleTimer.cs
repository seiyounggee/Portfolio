using UnityEngine;
using System.Collections;

public class IgnoreTimeScaleTimer : MonoBehaviour
{
	public void Play()
	{
		m_fStartTime = TimeHelper.realtimeSinceStartup;
		m_fLastPauseTime = 0;
	}

	public void Stop()
	{
		m_fStartTime = 0;
		m_fLastPauseTime = 0;
	}

	public void Pause()
	{
		m_fLastPauseTime = TimeHelper.realtimeSinceStartup;
	}

	public void Resume()
	{
		if (m_fLastPauseTime > 0)
			m_fStartTime += (TimeHelper.realtimeSinceStartup - m_fLastPauseTime);
	}

	public bool IsPlaying
	{ get { return m_fStartTime > 0; } }

	public float time
	{ get { return (m_fStartTime > 0) ? (TimeHelper.realtimeSinceStartup - m_fStartTime) : 0.0f; } }

	private float m_fStartTime;
	private float m_fLastPauseTime;
}
