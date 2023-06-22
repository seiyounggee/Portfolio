using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using System.IO;
using System;

public class PostProcessBuild_Asset
{
    [PostProcessBuild(8900)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        string _assetpath = string.Empty;
        AssetDefines.eOSType _target_os = AssetDefines.eOSType.Invalid;

        switch (buildTarget)
        {
            case BuildTarget.Android:

#if UNITY_2020_3_OR_NEWER
                //2020으로 들어오면서 경로가 바뀜 Golf King 파일 없이 unityLibrary 안에 있는 src파일에 있음
                _assetpath = string.Format( "{0}/{1}/src/main/assets", pathToBuiltProject, "unityLibrary");
#else
                _assetpath = string.Format( "{0}/{1}/src/main/assets", pathToBuiltProject, Application.productName );
#endif
                _target_os = AssetDefines.eOSType.Android;
                break;

            case BuildTarget.iOS:
                _assetpath = pathToBuiltProject + "/Data/Raw";
                _target_os = AssetDefines.eOSType.iOS;
                break;

            case BuildTarget.WebGL:
                _assetpath = pathToBuiltProject + "/StreamingAssets";
                _target_os = AssetDefines.eOSType.WebGL;
                break;

            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                {
                    var _dir = Path.GetDirectoryName( pathToBuiltProject );
                    var _file = Path.GetFileNameWithoutExtension(pathToBuiltProject);
                    _assetpath = string.Format("{0}/{1}_Data/StreamingAssets", _dir, _file);
                    _target_os = AssetDefines.eOSType.Standalone;
                }
                break;
        }

        if( string.IsNullOrEmpty(_assetpath) == true ){
            return;
        }

        string[] _dirs = Directory.GetDirectories (_assetpath);
        for (int i = 0, _max = _dirs.Length; i < _max; ++i) 
        {
            string[] _names = _dirs [i].Split (Path.DirectorySeparatorChar);
            if (Enum.IsDefined (typeof(AssetDefines.eOSType), _names[ _names.Length-1] ) == false) {
                continue;
            }

            if (_dirs [i].Contains( _target_os.ToString () ) == true) {
                continue;
            }

            Directory.Delete (_dirs [i], true);
        }
    }

}
