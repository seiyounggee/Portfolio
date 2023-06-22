using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DtrVersionManager : BaseVersionManager<DtrVersionManager>
{
    /* 다운 받은 파일이 없으면 다시 다운받도록 만들어주자 */
    public void CheckDownLoadVersion()
    {
        string filePath = string.Empty;
        /* 클라이언트 버전이 틀릴때 */

        if (string.IsNullOrEmpty(ClientVersion.ResFileName) == false)
        {
            filePath = string.Format("{0}{1}{2}", BaseVersionManager.GetVersionFilePath(), ClientVersion.ResFileName, BaseVersionManager.REFSFILE_EXTENTION);
            if (System.IO.File.Exists(filePath) == false)
                ClientVersion.ResVersion = ClientVersion.BuiltInResVersion;
        }
    }

    // 
    protected override void SendWWW(string url, Action<UnityEngine.Networking.UnityWebRequest> callback, Action<float> progress, bool isSaveFile = false)
    {
        WWWManager.Instance.LoadWWWAsync(DownLoadTypes.None, url, (WWWManager.WWWObject obj) => {
            callback.SafeInvoke(obj.www);
        }, progress, isSaveFile);
    }

    protected override void CompleteDownload(VersionType type, int target_version, string file_name)
    {
        if (type == VersionType.Resource)
        {
            ClientVersion.ResVersion = target_version;
            ClientVersion.ResFileName = file_name;
        }
        else if (type == VersionType.Asset)
        {
            ClientVersion.ABVersion = target_version;
        }
    }

    protected override bool DownloadError(string url, bool retry, Action<UnityEngine.Networking.UnityWebRequest> callback, Action<float> progress)
    {
        WWWManager.Instance.DeleteWWW(WWWManager.MakePrivateKey(url));

        if (retry == true)
        {
            SendWWW(url, callback, progress);
        }

        return retry;
    }
}
