
using System.IO;

public class WriteAttachedVersion
{
    public static void WriteResVersion( AssetDefines.eOSType type, int version )
    {
        int _assetversion = 0;

        string _full_path = Path.GetFullPath( Path.Combine( "Assets/Resources", string.Format( "{0}.txt", type.ToString() ) ) );
        if (File.Exists(_full_path) == true)
        {
            using (FileStream fs = new FileStream(_full_path, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    string _data = reader.ReadLine();
                    if( _data != null )
                    {
                        string[] _split = _data.Split(","[0]);
                        if (_split.Length > 1)
                        {
                            int.TryParse(_split[1], out _assetversion);
                        }
                    }
                }
            }
        }

        using( FileStream fs = new FileStream(_full_path, FileMode.Create) )
        {
            using( StreamWriter sw = new StreamWriter(fs) )
            {
                string _line_data = string.Format("{0},{1}", version, _assetversion);
                sw.WriteLine( _line_data );
            }
        }
    }

    public static void WriteAssetVersion( AssetDefines.eOSType type, int version )
    {
        int _resversion = 0;

        string _full_path = Path.GetFullPath( Path.Combine( "Assets/Resources", string.Format( "{0}.txt", type.ToString() ) ) );
        if (File.Exists(_full_path) == true)
        {
            using (FileStream fs = new FileStream(_full_path, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    string _data = reader.ReadLine();
                    if( _data != null )
                    {
                        string[] _split = _data.Split(","[0]);
                        if( _split.Length > 0 )
                        {
                            int.TryParse(_split[0], out _resversion);
                        }
                    }
                }
            }
        }

        using( FileStream fs = new FileStream(_full_path, FileMode.Create) )
        {
            using( StreamWriter sw = new StreamWriter(fs) )
            {
                string _line_data = string.Format("{0},{1}", _resversion, version);
                sw.WriteLine( _line_data );
            }
        }
    }

}
