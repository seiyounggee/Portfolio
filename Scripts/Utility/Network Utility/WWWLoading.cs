using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WWWLoading
{
	public bool IsFullLoading()
	{
		return m_vLoadingList.Count >= m_iMaxLoadingCount;
	}

	public bool IsLoading()
	{
		return m_vLoadingList.Count > 0;
	}

	public void AddWWWObject(WWWManager.WWWObject wwwObject)
	{
		int count = m_vIdleList.Count;

		for (int i = 0; i < count; i++)
		{
			if (wwwObject.priority > m_vIdleList[i].priority)
			{
				m_vIdleList.Insert(i, wwwObject);
				return;
			}
		}

		m_vIdleList.Add(wwwObject);
	}

	public void AddWWWObjectFirstPriority(WWWManager.WWWObject wwwObject)
	{
		wwwObject.priority = int.MaxValue;

		m_vIdleList.Insert(0, wwwObject);
	}

	public void DeleteWWWObject(WWWManager.WWWObject wwwObject)
	{

		if (m_vLoadingList.Remove(wwwObject) == false)
		{
			m_vIdleList.Remove(wwwObject);
		}
	}

    public void ClearWWWObject(bool bIsDispose = true)
	{
		foreach (WWWManager.WWWObject wwwObject in m_vLoadingList)
		{
			if (wwwObject.www != null)
			{
                if (true == bIsDispose)
                    wwwObject.www.Dispose();

                wwwObject.www = null;
			}
		}

		foreach (WWWManager.WWWObject wwwObject in m_vIdleList)
		{
			if (wwwObject.www != null)
			{
                if (true == bIsDispose)
                    wwwObject.www.Dispose();
				
				wwwObject.www = null;
			}
		}
		m_vLoadingList.Clear();
		m_vIdleList.Clear();
	}

	public void StopWWW(string privateKey)
	{
		foreach (WWWManager.WWWObject wwwObject in m_vLoadingList)
		{
			if (wwwObject.privateKey == privateKey)
			{
                wwwObject.onCallback = null;
				wwwObject.isStop = true;
                m_vLoadingList.Remove(wwwObject);

				break;
			}
		}
		foreach (WWWManager.WWWObject wwwObject in m_vIdleList)
		{
			if (wwwObject.privateKey == privateKey)
			{
				wwwObject.www = null;
                wwwObject.onCallback = null;
				m_vIdleList.Remove(wwwObject);

				break;
			}
		}
	}

	public float GetProgressWithPath(string wwwPath)
	{
		float progress = 1.0f;

		foreach (WWWManager.WWWObject wwwObject in m_vLoadingList)
		{
            if (null != wwwObject.www)
            {
                if (wwwObject.www.url == wwwPath)
                {
                    progress = wwwObject.www.downloadProgress;
                }
            }
		}

		return progress;
	}

	public WWWManager.WWWObject NextWWWObject()
	{
		if (IsFullLoading())
			return null;

		if (IdleCount == 0)
			return null;

		WWWManager.WWWObject nextObject = m_vIdleList[0];
		m_vIdleList.RemoveAt(0);
		m_vLoadingList.Add(nextObject);

		m_totalLoadingCount++;

		return nextObject;
	}

	public int MaxLoadingCount
	{
		get { return m_iMaxLoadingCount; }
		set { m_iMaxLoadingCount = value; }
	}

	public int TotalLoadingCount
	{ get { return m_totalLoadingCount; } }

	public int IdleCount
	{ get { return m_vIdleList.Count; } }

	private int m_iMaxLoadingCount = 1;

	private int m_totalLoadingCount = 0;

	private List<WWWManager.WWWObject> m_vLoadingList = new List<WWWManager.WWWObject>();
	private List<WWWManager.WWWObject> m_vIdleList = new List<WWWManager.WWWObject>();
}
