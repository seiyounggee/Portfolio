using UnityEngine;
using System.Collections;
using System.Text;
using System.Collections.Generic;


public class WWWRequest
{
	public void AddPostData(string name, int intValue)
	{
		string stringValue = System.Convert.ToString(intValue);

		AddPostData(name, stringValue);
	}

	public void AddPostData(string name, string stringValue)
	{
		StringBuilder stringBuilder = new StringBuilder(m_postData);

		if (m_postData.Length > 0)
		{
			stringBuilder.Append("&");
		}

		stringBuilder.Append(name);
		stringBuilder.Append("=");
		stringBuilder.Append(stringValue);

		m_postData = stringBuilder.ToString();
	}

	public void AddHeaderData(string headerKey, string headerValue)
	{
		m_headers.Add(headerKey, headerValue);
	}

	public void ClearPostData()
	{
		m_postData = "";
	}

	public void ClearHeaders()
	{
		m_headers.Clear();
	}

	// Debug?
	public string GetStringHeaders()
	{
		return m_headers.ToString();
	}
	
	public byte[] PostData
	{
		get
		{
			return Encoding.UTF8.GetBytes(m_postData);
		}
	}

    public Dictionary<string, string> Headers
	{
		get
		{
			return m_headers;
		}
	}

	private string m_postData = "";
    private Dictionary<string, string> m_headers = new Dictionary<string, string>();

}
