using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WWWMemory
{
	public WWWManager.WWWObject GetWWWObjectWithPrivateKey(string privateKey)
	{
		WWWManager.WWWObject findWWWObject = null;
		bool isFind = false;

		isFind = m_dict.TryGetValue(privateKey.ToLower(System.Globalization.CultureInfo.InvariantCulture), out findWWWObject);

		if (!isFind)
		{
			findWWWObject = null;
		}

		return findWWWObject;
	}


	public bool AddWWWObject(WWWManager.WWWObject wwwObject)
	{
		bool isAddComplete = false;

		if (IsExistPrivateKey(wwwObject.privateKey.ToLower(System.Globalization.CultureInfo.InvariantCulture)))
		{
			return isAddComplete;
		}

		m_dict.Add(wwwObject.privateKey.ToLower(System.Globalization.CultureInfo.InvariantCulture), wwwObject);
		isAddComplete = true;

		return isAddComplete;

	}

	public bool DeleteWWWObject(string privateKey)
	{
		WWWManager.WWWObject wwwObject = null;
		bool isDelete = false;

		// No Find
		if (!m_dict.TryGetValue(privateKey.ToLower(System.Globalization.CultureInfo.InvariantCulture), out wwwObject))
		{
			isDelete = false;
		}
		else if (null != wwwObject.www && !wwwObject.www.isDone)
		{
			isDelete = false;
		}
		else // Complete
		{
            if (null != wwwObject.www)
            {
                wwwObject.www.Dispose();
            }			
			wwwObject.www = null;

			m_dict.Remove(privateKey.ToLower(System.Globalization.CultureInfo.InvariantCulture));

			isDelete = true;
		}

		return isDelete;
	}

	public bool ClearWWW()
	{
		bool isClear = true;

		foreach (string dictKey in m_dict.Keys)
		{
			DeleteWWWObject(dictKey);
		}

		return isClear;
	}


	public bool IsExistPrivateKey(string privateKey)
	{
		if (string.IsNullOrEmpty(privateKey))
			return false;

		return (m_dict.ContainsKey(privateKey.ToLower(System.Globalization.CultureInfo.InvariantCulture)));
	}

	private Dictionary<string, WWWManager.WWWObject> m_dict = new Dictionary<string, WWWManager.WWWObject>();
}
