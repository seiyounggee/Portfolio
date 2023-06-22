using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class WWWCache
{
	public delegate void CacheCallback(byte[] cache);

	public bool SaveCache(string fileName, byte[] bytes, bool isOverlap)
	{
#if UNITY_WEBPLAYER
        return false;
#endif
		FileMode fileMode = FileMode.Create;
		bool isSaveSuccess = false;

		string fullPath = this.MakeStringCachePath(fileName);

		// Check Exist File
		if (isOverlap == false)
		{
			fileMode = FileMode.CreateNew;
		}
		else
		{
			fileMode = FileMode.Create;
		}

		try
		{
			this.MakeCacheDirectory();

			FileStream fileStream = new FileStream(fullPath, fileMode);

			if (fileStream != null)
			{
				fileStream.Write(bytes, 0, bytes.Length);
				fileStream.Close();
			}

            isSaveSuccess = true;
		}

		catch (IOException)
		{
			isSaveSuccess = false;
		}

		return isSaveSuccess;
	}

	public byte[] LoadCache(string fileName)
	{
#if UNITY_WEBPLAYER
        return null;
#else
		byte[] readBuffer;
		string fullPath = this.MakeStringCachePath(fileName);

		try
		{
			readBuffer = File.ReadAllBytes(fullPath);
		}

		catch (FileNotFoundException)
		{
			readBuffer = null;
		}

		return readBuffer;
#endif
	}

	public IEnumerator LoadCacheAsync(string fileName, CacheCallback cacheCallback)
	{
		byte[] readBuffer = null;
		string fullPath = this.MakeStringCachePath(fileName);

		byte[] buffer = new byte[512];

		int fileSize = 0;
		FileStream fileStream = File.OpenRead(fullPath);
		readBuffer = new byte[fileStream.Length];
		int readSize = 0;

		while ((readSize = fileStream.Read(buffer, 0, buffer.Length)) > 0)
		{
			System.Array.Copy(buffer, 0, readBuffer, fileSize, readSize);
			fileSize += readSize;
			yield return null;
		}

		fileStream.Close();
	}

	public bool IsExistFile(string filePath)
	{
		if (string.IsNullOrEmpty(filePath))
			return false;

#if UNITY_WEBPLAYER
        return false;
#else
		if (File.Exists(this.MakeStringCachePath(filePath)))
		{
			return true;
		}

		return false;
#endif
	}

    public void ClearCacheFiles()
    {
#if UNITY_WEBPLAYER
#else
        DirectoryInfo directoryInfo = new DirectoryInfo( this.MakeStringCacheDirectoryPath() );

        foreach( FileInfo fileInfo in directoryInfo.GetFiles() )
        {
            fileInfo.Delete();
        }
#endif
    }

	public string MakeStringCachePath(string fileName)
	{
		string cachePath = Path.Combine(this.MakeStringCacheDirectoryPath(), fileName.ToLower(System.Globalization.CultureInfo.InvariantCulture));

		return cachePath;
	}

	public string MakeStringCacheWWWPath(string fileName)
	{
		string cacheWWWPath = string.Empty;

        cacheWWWPath = MakeStringCachePath(fileName);

        return cacheWWWPath;
	}

	private void MakeCacheDirectory()
	{
		if (Directory.Exists(m_cacheDirectoryPath) == false)
		{
			Directory.CreateDirectory(m_cacheDirectoryPath);
		}
	}

	public string MakeStringCacheDirectoryPath()
	{
		if (m_cacheDirectoryPath == null)
			m_cacheDirectoryPath = Path.Combine(Application.persistentDataPath, "cache");

		return m_cacheDirectoryPath;
	}

	string m_cacheDirectoryPath;
}
