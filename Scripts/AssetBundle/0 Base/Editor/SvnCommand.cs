
using System.Diagnostics;
using System.IO;

#if UNITY_EDITOR    
using UnityEngine;
#endif

public class SvnCommand 
{
	public static string CheckSvnStatusNormal( string full_path )
	{
		return systemCommand( "svn status", full_path );
	}

	public static bool UpdateSvnRevision( string full_path, int revision = -1 )
	{
		string _command = "svn update";
		if (revision > 0) {
			_command += string.Format (" -r{0}", revision);
		}

		string _data    = systemCommand( _command, full_path );
		if( string.IsNullOrEmpty( _data ) == true )
		{
#if UNITY_EDITOR			
            UnityEngine.Debug.LogError( "Error Update Revision  : " + full_path + " To " + revision );	
#endif
			return false;
		}

		string _keyword = string.Format( "revision {0}.", revision);

		string[] _split = _data.Split( "\n"[0] );
		for( int i=0, _max = _split.Length; i < _max; ++i )
		{
			if( _split[i].Contains( _keyword ) == true ){
				return true;
			}
		}

		return false;
	}
	
	public static int GetSvnRevision( string full_path )
	{
		var ext = Path.GetExtension( full_path );
		if( string.IsNullOrEmpty( ext ) == true )
		{
			if( Directory.Exists( full_path ) == false )
			{
#if UNITY_EDITOR			
                UnityEngine.Debug.LogError( "Not Exist Directory : " + full_path );	
#endif				
				return -1;
			}
		}
		else if( File.Exists( full_path ) == false )
		{
#if UNITY_EDITOR			
            UnityEngine.Debug.LogError( "Not Exist File Data : " + full_path );	
#endif
			return -1;
		}

		string _data    = systemCommand( "svn info", full_path );
		if( string.IsNullOrEmpty( _data ) == true )
		{
#if UNITY_EDITOR			
            UnityEngine.Debug.LogError( "Error Get Revision Data : " + full_path );	
#endif
			return -1;
		}

		_data = _data.Replace ("\r\n", "\n");

		string _result  = "";

		const string _keyword = "Last Changed Rev:";

		string[] _split = _data.Split( "\n"[0] );
		for( int i=0, _max = _split.Length; i < _max; ++i )
		{
			if( _split[i].Contains( _keyword ) == false ){
				continue;
			}

			_result = _split[i].Replace( _keyword, "" );
			_result.Trim();
			break;
		}

		int _value;
		if( int.TryParse( _result, out _value ) == true ){
			return _value;
		}
		else{
			return -1;
		}
	}
	
	static string systemCommand( string command, string path )
	{
		string output = "";

		using( Process p = new Process() )
		{
#if UNITY_EDITOR_WIN
			p.StartInfo.FileName  = "cmd";
			p.StartInfo.Arguments = string.Format( " /c {0} \"{1}\"", command, path );
#else
			p.StartInfo.FileName = "/bin/bash";

			// string _command = string.Format("/usr/bin/{0}", command );
			string _command = string.Format("/usr/local/bin/{0}", command );
			string _path = path.Replace (" ", "\\ ");
			p.StartInfo.Arguments = string.Format( "-c \"export LANG=en_US\n{0} {1}\"", _command, _path );
#endif

			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.CreateNoWindow = true;

			p.Start();
			p.WaitForExit();

			output = p.StandardOutput.ReadToEnd ();

			string _err = p.StandardError.ReadToEnd ();
			if( string.IsNullOrEmpty( _err ) == false ){
#if UNITY_EDITOR
                UnityEngine.Debug.LogError (_err);
#endif
			}
		}

		return output;
	}
}
