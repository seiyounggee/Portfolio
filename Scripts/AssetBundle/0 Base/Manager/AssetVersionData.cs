using System.Collections;
using System.Collections.Generic;

using System.IO;

#if UNITY_EDITOR
using UnityEngine;
#endif

public class AssetVersionData 
{
    public int version;
    public string filename;
    public int revision;
#if UNITY_2017_1_OR_NEWER
    public string hash;
#endif
    public string lookuptable;

	//
	public static AssetVersionData GetData( string line )
	{
		if( string.IsNullOrEmpty( line ) == true ){
			return null;
		}

        int _index = 0;

        string[] _split = line.Split( ","[0] );

		AssetVersionData _new = new AssetVersionData();
		
        _new.version      = int.Parse( _split[_index++] );
        _new.filename     = _split[_index++];
        _new.revision     = int.Parse( _split[_index++] );
#if UNITY_2017_1_OR_NEWER
        _new.hash         = _split[_index++];
#endif
        if( _index < _split.Length ){
            _new.lookuptable = _split[_index++];
        }

		return _new;
	}

#if UNITY_EDITOR
	public static string Serialize( AssetVersionData data )
	{
    #if UNITY_2017_1_OR_NEWER
        return string.Format( "{0},{1},{2},{3},{4}", data.version, data.filename, data.revision, data.hash, data.lookuptable );
    #else
        return string.Format( "{0},{1},{2},{3}", data.version, data.filename, data.revision, data.lookuptable );
    #endif
	}
#endif
}

public class AssetVersions
{
	public const string FILE_EXTENTION     = ".av";

    //
	public static Dictionary< string, AssetVersionData > Load( byte[] data )
	{
		if( data == null ){
			return null;
		}

		Dictionary< string, AssetVersionData > _dic = null;

		using( MemoryStream ms = new MemoryStream( data ) )
		{
			using( StreamReader reader = new StreamReader( ms ) )
			{
				_dic = new Dictionary<string, AssetVersionData>();

				while( true )
				{
					string _line_data = reader.ReadLine();
					if( _line_data == null ){
						break;
					}

					var _loaded = AssetVersionData.GetData( _line_data );
					if( _loaded == null )
						continue;

                    string _file_name = Path.GetFileNameWithoutExtension(_loaded.filename);
					
                    if( _dic.ContainsKey( _file_name ) == true ){
#if UNITY_EDITOR						
                        Debug.LogError( "Same File Name Exist : " + _file_name );
#endif
					}
					else
					{
                        _dic.Add( _file_name, _loaded );			
					}
				}
			}
		}

		return _dic;
	}

#if UNITY_EDITOR
    public static List< AssetVersionData > ReadAssetVersionFile( string path, int version )
    {
        List<AssetVersionData> _result = new List<AssetVersionData>();

        string _full_path = Path.GetFullPath( Path.Combine( path, string.Format("{0}{1}", version, FILE_EXTENTION) ) );
        if( File.Exists(_full_path) == false ){
            return _result;
        }

        using( FileStream fs = new FileStream(_full_path, FileMode.Open) )
        {
            using( StreamReader reader = new StreamReader(fs) )
            {
                while( true )
                {
                    string _data = reader.ReadLine();
                    if( _data == null )
                        break;

                    _result.Add( AssetVersionData.GetData(_data) );
                }
            }
        }

        return _result;
    }

    public static void WriteAssetVersionFile( string path, int version, List<AssetVersionData> list_data )
    {
        // check if exist same file name
        List<string> _list_temp = new List<string>();
        for( int i = 0, _max = list_data.Count; i < _max; ++i )
        {
            if( _list_temp.Contains( list_data[i].filename ) == true )
            {
                Debug.LogError("Exist Same File Name >> " + list_data[i].filename);
                return;
            }

            _list_temp.Add(list_data[i].filename);
        }

        // 
        try
        {
            string _file_name = string.Format( "{0}{1}", version, FILE_EXTENTION );
            string _full_path = Path.GetFullPath( Path.Combine( path, _file_name ) );

            using( FileStream fs = new FileStream( _full_path, FileMode.Create ) )
            {
                using(  StreamWriter sw = new StreamWriter( fs ) )
                {
                    for( int i=0, _max = list_data.Count; i < _max; ++i )
                    {
                        string _line_data = AssetVersionData.Serialize( list_data[i] );
                        sw.WriteLine( _line_data );
                    }
                }
            }
        }
        catch( System.Exception e )
        {
            Debug.LogError(e.Message);
        }
    }
#endif
}
