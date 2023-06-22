using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using System.IO;

public class ProcessBuild_AttachedVersion : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild( BuildReport report )
      {
        var _os_type = GetOsType(report.summary.platform);

        string _path = "Assets/Resources";

        for (int i = 0, _max = (int)AssetDefines.eOSType.Max; i < _max; ++i)
        {
            if( i == (int)_os_type ){
                continue;
            }

            var _file_name = string.Format("{0}.txt", (AssetDefines.eOSType)i);
            var _file_path = Path.Combine(_path, _file_name );
            if( File.Exists(_file_path) == false ){
                continue;
            }

            if( File.Exists(_file_name) == true ){
                FileUtil.DeleteFileOrDirectory(_file_name);
            }
            
            FileUtil.MoveFileOrDirectory( _file_path, _file_name );
        }

        AssetDatabase.Refresh();

    }

    public void OnPostprocessBuild( BuildReport report )
    {
        var _os_type = GetOsType(report.summary.platform);

        string _path = "Assets/Resources";

        for (int i = 0, _count = (int)AssetDefines.eOSType.Max ; i < _count; ++i)
        {
            if( i == (int)_os_type ){
                continue;
            }

            var _file_name = string.Format("{0}.txt", (AssetDefines.eOSType)i);
            if( File.Exists(_file_name) == false ){
                continue;
            }

            var _file_path = Path.Combine(_path, _file_name );
            FileUtil.MoveFileOrDirectory( _file_name, _file_path );
        }

        AssetDatabase.Refresh();
    }

    AssetDefines.eOSType GetOsType( BuildTarget target )
    {
        AssetDefines.eOSType _target_os = AssetDefines.eOSType.Invalid;

        switch (target)
        {
            case BuildTarget.Android:
                _target_os = AssetDefines.eOSType.Android;
                break;
            case BuildTarget.WebGL:
                _target_os = AssetDefines.eOSType.WebGL;
                break;
            case BuildTarget.iOS:
                _target_os = AssetDefines.eOSType.iOS;
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                _target_os = AssetDefines.eOSType.Standalone;
                break;
        }

        return _target_os;
    }

}
