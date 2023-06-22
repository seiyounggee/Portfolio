using UnityEngine;
using System.Collections;
using PNIX.Engine.NetworkCommon;

public class ClientVersion
{
    public const string playerprefs_builtinresversion = "builtinresversion";
    public const string playerprefs_builtinabversion = "builtinabversion";
    public const string playerprefs_resname = "resfilename";


#if SERVERTYPE_RELEASE
    public static EServerGroupType CurrServer = EServerGroupType.Release;
#elif SERVERTYPE_REVIEW
    public static EServerGroupType CurrServer = EServerGroupType.Review;
#elif SERVERTYPE_QA
    public static EServerGroupType CurrServer = EServerGroupType.QA;
#elif SERVERTYPE_DEV
    public static EServerGroupType CurrServer = EServerGroupType.Dev;
#else
    public static EServerGroupType CurrServer = EServerGroupType.Dev;
#endif

    #region Internal varialbes
    /* 현재 클라에서 사용중인 버전들 */
    private static string ResVersionName = string.Empty;
    private static int CurrResVersion = -1;         /* 이거 직접 쓰지 말자 가져다 쓰지 말자 */
    private static int CurrABVersion = -1;          /* 이거 직접 쓰지 말자 가져다 쓰지 말자 */
    private static int CurrLocalizationVersion = -1;

    private static string m_BuildVersionDetailString = string.Empty;
    private static EServerGroupType m_OriginalServer = EServerGroupType.Dev;
    #endregion

    /* 빌드에 포함된 버전들 */
    public static int BuiltInResVersion
    {
        get
        {
            switch( CurrServer )
            {
            case EServerGroupType.Review:
            case EServerGroupType.Release:
            case EServerGroupType.QA:
            case EServerGroupType.Dev:
            default:
                return 0;
            }
        }
    }

    public static string BuildVersionDetailString
    {
        get 
        {
            if( string.IsNullOrEmpty( m_BuildVersionDetailString ) )
                m_BuildVersionDetailString = GetServerVersion( CurrServer );

            return m_BuildVersionDetailString; 
        }
        set
        {
            m_BuildVersionDetailString = value;
        }
    }

    public static int ABVersion
    {
        get
        {
            return CurrABVersion;
        }
        set
        {
            CurrABVersion = value;
            PlayerPrefs.SetInt( playerprefs_builtinabversion, CurrABVersion );
        }
    }

    public static int ResVersion
    {
        get
        {
            return CurrResVersion;
        }
        set
        {
            CurrResVersion = value;
            PlayerPrefs.SetInt( playerprefs_builtinresversion, CurrResVersion );
        }
    }

    public static string ResFileName
    {
        get
        {
            if( string.IsNullOrEmpty( ResVersionName ) )
                ResVersionName = PlayerPrefs.GetString( playerprefs_resname, "" );
            return ResVersionName;
        }
        set
        {
            ResVersionName = value;
            PlayerPrefs.SetString( playerprefs_resname, ResVersionName );
        }
    }

    public static string ResFilePath
    {
        get
        {
            if (string.IsNullOrEmpty(resFilePath))
                resFilePath = PlayerPrefs.GetString(playerprefs_resFilePath, "");
            return resFilePath;
        }
        set
        {
            resFilePath = value;
            PlayerPrefs.SetString(playerprefs_resFilePath, resFilePath);
        }
    }

    private static string resFilePath = string.Empty;
    private static string playerprefs_resFilePath = "playerprefs_resFilePath";

    public static string RefCachePrivateKey
    {
        get
        {
            if (string.IsNullOrEmpty(refCachePrivateKey))
                refCachePrivateKey = PlayerPrefs.GetString(playerprefs_refCachePrivateKey, "");
            return refCachePrivateKey;
        }
        set
        {
            refCachePrivateKey = value;
            PlayerPrefs.SetString(playerprefs_refCachePrivateKey, refCachePrivateKey);
        }
    }

    private static string refCachePrivateKey = string.Empty;
    private static string playerprefs_refCachePrivateKey = "playerprefs_refCachePrivateKey";

    public static string RefCachName = "RefCache.ref";


    public static int LocalizationVersion
    {
        get
        {
            return CurrLocalizationVersion;
        }
        set
        {
            CurrLocalizationVersion = value;
        }
    }


    public static void Clear()
    {
        ResFileName = "";
        ResVersion = -1;
        ABVersion = -1;
        CurrLocalizationVersion = -1;
        //BuildVersionDetailString = "";
        CurrServer = m_OriginalServer;

    }

    public static void Initialize( bool isServerSelect = false )
    {
        m_OriginalServer = CurrServer;

        ResVersionName = PlayerPrefs.GetString( playerprefs_resname, "" );
        CurrLocalizationVersion = -1;

        /* 0보다 작으면 저장된 값이 있는지 확인후 가져오자 */
        CurrResVersion = PlayerPrefs.GetInt( playerprefs_builtinresversion, -1 );
        /* 저장된값이 없거나 저장된값이 빌드에 포함된것 보다 작은지 확인하자 */
        if( CurrResVersion <= 0 || CurrResVersion < BuiltInResVersion )
        {
            ResVersion = BuiltInResVersion;
        }

        //int _attached_version = GolfVersionManager.Instance.GetAssetFileVersion( true );
        int _attached_version = 0;

        /* 0보다 작으면 저장된 값이 있는지 확인후 가져오자 */
        CurrABVersion = PlayerPrefs.GetInt( playerprefs_builtinabversion, -1 );
        /* 저장된값이 없거나 저장된값이 빌드에 포함된것 보다 작은지 확인하자 */
        if( CurrABVersion <= 0 || CurrABVersion < _attached_version )
        {
            ABVersion = _attached_version;
        }

        Debug.Log("<color=cyan>CurrServer : " + CurrServer + "( " + BuildVersionDetailString + " )" + "    CurrResVersion : " + CurrResVersion + "     ResVersion : " + ResVersion
            + "     CurrABVersion : " + CurrABVersion + "   ABVersion : " + ABVersion + "</color>");
    }

    public static string GetServerVersion( EServerGroupType type )
    {
        /* 받아오는건 항상 개발 버전으로 셋팅 */
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        string versionstring = Version.CSteam.Version;
#elif UNITY_IPHONE || UNITY_IOS
        string versionstring = Version.CIOS.Version;
#else
        string versionstring = Version.CAOS.Version;
#endif

        if( type == EServerGroupType.Dev )
            return versionstring;

        string[] versions = versionstring.Split( new char[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries );
        if( versions == null || versions.Length < 4 )
            return versionstring;

        int detail = 0;
        if( int.TryParse( versions[ 3 ], out detail ) == false )
            return versionstring;

        switch( type )
        {
        case EServerGroupType.QA:
            detail -= 1;
            break;
        case EServerGroupType.Review:
            detail -= 2;
            break;
        case EServerGroupType.Release:
            detail -= 3;
            break;
        }

        versionstring = string.Format( "{0}.{1}.{2}.{3}", versions[ 0 ], versions[ 1 ], versions[ 2 ], detail.ToString( "D6" ) );
        return versionstring;
    }
}