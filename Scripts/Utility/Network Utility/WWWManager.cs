using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

public enum DownLoadTypes
{
    None = 0,           /* byte[], string */
    Texture,
}

public class WWWManager : MonoBehaviour
{
    public static WWWManager Instance
    {
        get
        {
            if( m_instance == null )
            {
                m_instance = Object.FindObjectOfType( typeof( WWWManager ) ) as WWWManager;

                if( m_instance == null )
                {
                    GameObject go = new GameObject( "_WWWManager" );
                    DontDestroyOnLoad( go );
                    m_instance = go.AddComponent<WWWManager>();
                }
            }

            return m_instance;
        }
    }

    public class WWWObject
    {
        public UnityWebRequest www
        {
            get; set;
        }
        public string wwwPath
        {
            get; set;
        }
        public string privateKey
        {
            get; set;
        }
        public CallbackLoadComplete onCallback
        {
            get; set;
        }
        public bool isSaveFile
        {
            get; set;
        }
        public bool isMemory
        {
            get; set;
        }
        public bool isRequest
        {
            get; set;
        }
        public bool isTimeOut
        {
            get; set;
        }
        public bool isTimeCheck
        {
            get; set;
        }
        //		public byte[] postData { get; set; }
        public Dictionary<string, string> headers
        {
            get; set;
        }
        public System.Action<float> progressCallBack
        {
            get; set;
        }
        public int priority
        {
            get; set;
        }

        public bool isStop
        {
            get; set;
        }

        public DownLoadTypes downdloadType = DownLoadTypes.None;

        public WWWObject( DownLoadTypes type, string wwwPath, string privateKey, CallbackLoadComplete onCallback, bool isSaveFile,
            bool isMemory, bool isRequest, int priority, System.Action<float> onCallbackProgress )
        {
            this.downdloadType = type;
            this.www = null;
            this.wwwPath = wwwPath;
            this.privateKey = privateKey;
            this.onCallback = onCallback;
            this.isSaveFile = isSaveFile;
            this.isMemory = isMemory;
            this.isRequest = isRequest;

            this.priority = priority;
            this.progressCallBack = onCallbackProgress;
        }

        public Texture2D Texture
        {
            get
            {
                if( this.downdloadType == DownLoadTypes.Texture )
                {
                    try
                    {
                        return DownloadHandlerTexture.GetContent( this.www );
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                    return null;
            }
        }
    }

    public float timeout_time = 10.0f;

    void Awake()
    {
        if( m_instance == null )
            m_instance = this;

        m_ignoreTimeScaleTimer = gameObject.GetComponent<IgnoreTimeScaleTimer>();

        if( m_ignoreTimeScaleTimer == null )
            m_ignoreTimeScaleTimer = gameObject.AddComponent<IgnoreTimeScaleTimer>();

        Object.DontDestroyOnLoad( this );
    }

    void Update()
    {
        WWWTitleUpdate();
        WWWUpdate();
    }

    void WWWTitleUpdate()
    {
        if( m_qTitleCallback.Count > 0 )
        {
            WWWObject wwwObject = ( WWWObject )m_qTitleCallback.Dequeue();

            if( true == wwwObject.isTimeCheck )
            {
                //if (true == wwwObject.www.isDone)
                //{
                //    wwwObject.isTimeOut = false;
                //}

                if( false == wwwObject.isTimeOut )
                {
                    if( !string.IsNullOrEmpty( wwwObject.www.error ) )
                    {
                        Debug.Log( wwwObject.www.error );
                    }
                }

                if( wwwObject.onCallback != null )
                    wwwObject.onCallback( wwwObject );

                if( null != wwwObject.www )
                {
                    wwwObject.www.Dispose();
                    wwwObject.www = null;
                }
            }
            else
            {
                if( !string.IsNullOrEmpty( wwwObject.www.error ) )
                {
                    Debug.Log( wwwObject.www.error );
                }

                if( wwwObject.onCallback != null )
                    wwwObject.onCallback( wwwObject );

                if( !wwwObject.isMemory )
                {
                    wwwObject.www.Dispose();
                    wwwObject.www = null;
                }
                else
                {
                    if( !string.IsNullOrEmpty( wwwObject.www.error ) )
                        m_wwwMemory.AddWWWObject( wwwObject );
                }
            }

        }

        WWWObject nextObject = m_wwwTitleLoading.NextWWWObject();
        if( nextObject != null )
        {
            StartCoroutine( StartLoadTimeOutAsync( nextObject ) );
        }
    }

    void WWWUpdate()
    {
        if( m_qCallback.Count > 0 )
        {
            WWWObject wwwObject = ( WWWObject )m_qCallback.Dequeue();

            if( !string.IsNullOrEmpty( wwwObject.www.error ) )
            {
                Debug.Log( wwwObject.www.error );
            }

            if( wwwObject.onCallback != null )
                wwwObject.onCallback( wwwObject );

            if( !wwwObject.isMemory )
            {
                wwwObject.www.Dispose();
                wwwObject.www = null;
            }
            else
            {
                if( wwwObject.www != null && wwwObject.www.error == null )
                    m_wwwMemory.AddWWWObject( wwwObject );
            }

        }

        WWWObject nextObject = m_wwwLoading.NextWWWObject();

        if( nextObject != null )
        {
            StartCoroutine( StartLoadAsync( nextObject ) );
        }

        UpdateWWWStartTime();
    }

    void OnApplicationPause( bool isPause )
    {
        if( m_ignoreTimeScaleTimer.IsPlaying )
        {
            if( isPause )
                m_ignoreTimeScaleTimer.Pause();
            else
                m_ignoreTimeScaleTimer.Resume();
        }
    }

    //Default Base Web Path
    public void SetBaseWebPath( string basePath )
    {
        m_baseWebPath = basePath;
    }
    public string GetBaseWebPath()
    {
        return m_baseWebPath;
    }

    public UnityWebRequest LoadMemoryObject( string privateKey )
    {
        UnityWebRequest www = null;

        if( m_wwwMemory.IsExistPrivateKey( privateKey ) )
        {
            WWWObject wwwMemoryObject = GetWWWObject( privateKey );

            www = wwwMemoryObject.www;

            Debug.Log( "StartLoad From Memory : " + privateKey );
        }

        return www;
    }

    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, CallbackLoadComplete onCallback, System.Action<float> onCallbackProgress )
    {
        return LoadWWWAsync( type, wwwPath, MakePrivateKey( wwwPath ), false, false, false, 0, onCallback, false, onCallbackProgress );
    }

    public bool LoadWWWAsync(DownLoadTypes type, string wwwPath, CallbackLoadComplete onCallback, System.Action<float> onCallbackProgress, bool isSaveFile)
    {
        return LoadWWWAsync(type, wwwPath, MakePrivateKey(wwwPath), isSaveFile, false, false, 0, onCallback, false, onCallbackProgress);
    }

    //no save, no memory, async
    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, CallbackLoadComplete onCallback )
    {
        return LoadWWWAsync( type, wwwPath, MakePrivateKey( wwwPath ), false, false, false, 0, onCallback );
    }

    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, int priority, CallbackLoadComplete onCallback )
    {
        return LoadWWWAsync( type, wwwPath, MakePrivateKey( wwwPath ), false, false, false, priority, onCallback );
    }

    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, string privateKey, CallbackLoadComplete onCallback )
    {
        return LoadWWWAsync( type, wwwPath, privateKey, false, false, false, 0, onCallback );
    }
    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, int priority, string privateKey, CallbackLoadComplete onCallback )
    {
        return LoadWWWAsync( type, wwwPath, privateKey, false, false, false, priority, onCallback );
    }

    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, bool isRequest, CallbackLoadComplete onCallback )
    {
        return LoadWWWAsync( type, wwwPath, MakePrivateKey( wwwPath ), false, false, isRequest, 0, onCallback );
    }
    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, bool isRequest, int priority, CallbackLoadComplete onCallback )
    {
        return LoadWWWAsync( type, wwwPath, MakePrivateKey( wwwPath ), false, false, isRequest, priority, onCallback );
    }

    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, string privateKey, bool isRequest, CallbackLoadComplete onCallback )
    {
        return LoadWWWAsync( type, wwwPath, privateKey, false, false, isRequest, 0, onCallback );
    }
    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, string privateKey, bool isRequest, int priority, CallbackLoadComplete onCallback )
    {
        return LoadWWWAsync( type, wwwPath, privateKey, false, false, isRequest, priority, onCallback );
    }

    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, bool isSaveFile, bool isMemory )
    {
        return LoadWWWAsync( type, wwwPath, MakePrivateKey( wwwPath ), isSaveFile, isMemory, false, 0, null );
    }
    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, bool isSaveFile, bool isMemory, int priority )
    {
        return LoadWWWAsync( type, wwwPath, MakePrivateKey( wwwPath ), isSaveFile, isMemory, false, priority, null );
    }

    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, string privateKey, bool isSaveFile, bool isMemory )
    {
        return LoadWWWAsync( type, wwwPath, privateKey, isSaveFile, isMemory, false, 0, null );
    }
    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, string privateKey, bool isSaveFile, bool isMemory, int priority )
    {
        return LoadWWWAsync( type, wwwPath, privateKey, isSaveFile, isMemory, false, priority, null );
    }

    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, bool isSaveFile, bool isMemory, CallbackLoadComplete onCallback )
    {
        return LoadWWWAsync( type, wwwPath, MakePrivateKey( wwwPath ), isSaveFile, isMemory, false, 0, onCallback );
    }

    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, bool isSaveFile, bool isMemory, int priority, CallbackLoadComplete onCallback )
    {
        return LoadWWWAsync( type, wwwPath, MakePrivateKey( wwwPath ), isSaveFile, isMemory, false, priority, onCallback );
    }

    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, string privateKey, bool isSaveFile, bool isMemory, CallbackLoadComplete onCallback )
    {
        return LoadWWWAsync( type, wwwPath, privateKey, isSaveFile, isMemory, false, 0, onCallback );
    }
    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, string privateKey, bool isSaveFile, bool isMemory, int priority, CallbackLoadComplete onCallback )
    {
        return LoadWWWAsync( type, wwwPath, privateKey, isSaveFile, isMemory, false, priority, onCallback );
    }

    public bool LoadTitleWWWAsync( DownLoadTypes type, string wwwPath, bool isRequest, CallbackLoadComplete onCallback, bool bTimeOut )
    {
        return LoadTitleWWWAsync( type, wwwPath, MakePrivateKey( wwwPath ), false, false, isRequest, 0, onCallback, bTimeOut );
    }

    public bool LoadWWWAsync( DownLoadTypes type, string wwwPath, string privateKey, bool isSaveFile, bool isMemory, bool isRequest, int priority, CallbackLoadComplete onCallback,
        bool bTimeOut = false, System.Action<float> onCallbackProgress = null )
    {
        WWWObject wwwObject = new WWWObject( type, wwwPath, privateKey, onCallback, isSaveFile, isMemory, isRequest, priority, onCallbackProgress );

        wwwObject.isTimeCheck = bTimeOut;

        if( isRequest )
        {
            //wwwObject.postData = m_wwwRequest.PostData;
            wwwObject.headers = new Dictionary<string, string>( m_wwwRequest.Headers );

            // Clear Post / Header
            m_wwwRequest.ClearPostData();
            m_wwwRequest.ClearHeaders();

        }


        if( isMemory && IsExistMemory( privateKey ) )
        {
            m_wwwLoading.AddWWWObjectFirstPriority( wwwObject );
        }
        else
        {
            m_wwwLoading.AddWWWObject( wwwObject );
        }

        //DownloadHandlerTexture
        return true;
    }

    public bool LoadTitleWWWAsync( DownLoadTypes type, string wwwPath, string privateKey, bool isSaveFile, bool isMemory, bool isRequest, int priority, CallbackLoadComplete onCallback,
        bool bTimeOut = false, System.Action<float> onCallbackProgress = null )
    {
        WWWObject wwwObject = new WWWObject( type, wwwPath, privateKey, onCallback, isSaveFile, isMemory, isRequest, priority, onCallbackProgress );

        wwwObject.isTimeCheck = bTimeOut;

        if( isRequest )
        {
            //wwwObject.postData = m_wwwRequest.PostData;
            wwwObject.headers = new Dictionary<string, string>( WWWRequest.Headers );

            // Clear Post / Header
            m_wwwRequest.ClearPostData();
            m_wwwRequest.ClearHeaders();

        }

        m_wwwTitleLoading.AddWWWObject( wwwObject );

        return true;
    }

    public void StopWWW( string privateKey )
    {
        m_wwwLoading.StopWWW( privateKey );
    }

    public void StopAllLoadWWW( bool bIsDispose = true )
    {
        StopAllCoroutines();
        m_wwwLoading.ClearWWWObject( bIsDispose );
        m_wwwTitleLoading.ClearWWWObject( bIsDispose );

        while( m_qCallback.Count > 0 )
        {
            WWWObject wwwObject = m_qCallback.Dequeue();

            wwwObject.onCallback = null;

            if( null != wwwObject.www )
            {
                if( true == bIsDispose )
                    wwwObject.www.Dispose();

                wwwObject.www = null;
            }
        }

        while( m_qTitleCallback.Count > 0 )
        {
            WWWObject wwwObject = m_qTitleCallback.Dequeue();

            wwwObject.onCallback = null;

            if( null != wwwObject.www )
            {
                if( true == bIsDispose )
                    wwwObject.www.Dispose();

                wwwObject.www = null;
            }
        }
    }

    public void DeleteWWW( string privateKey )
    {
        DeleteWWWObject( privateKey );
    }

    public float GetProgressWithPath( string wwwPath )
    {
        return m_wwwLoading.GetProgressWithPath( wwwPath );
    }

    public bool IsWWWLoading()
    {
        return m_wwwLoading.IsLoading();
    }

    public bool IsTimeOut()
    {
        return m_ignoreTimeScaleTimer.time > 0 && IsWWWLoading() && m_ignoreTimeScaleTimer.time > timeout_time;
    }

    public static string MakePrivateKey( string wwwPath )
    {
        return Path.GetFileName( wwwPath );
    }

    protected IEnumerator StartLoadAsync( WWWObject wwwObject )
    {
        if( m_wwwMemory.IsExistPrivateKey( wwwObject.privateKey ) )
        {
            WWWObject wwwMemoryObject = GetWWWObject( wwwObject.privateKey );

            wwwObject.www = wwwMemoryObject.www;

            Debug.Log( "StartLoadAsync From Memory : " + wwwObject.wwwPath );
        }
        else
        {
            string fullPath = "";

            if( m_wwwCache.IsExistFile( wwwObject.privateKey ) )
                fullPath = m_wwwCache.MakeStringCacheWWWPath( wwwObject.privateKey );
            else
                fullPath = MakeWebPath( wwwObject.wwwPath );

#if UNITY_EDITOR
            /*
            bool isWinEditor = Application.platform == RuntimePlatform.WindowsEditor;
            bool isOSXEditor = Application.platform == RuntimePlatform.OSXEditor;

            //프로젝트가 mac 외장하드에서 불러오는 경우 따로 처리해주자 
            if (isOSXEditor && fullPath.Contains("Volumes") && fullPath.Contains("Assets"))
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone);

                var split = fullPath.Split('/', '\\');

                if (split != null && split.Length > 0)
                {
                    var newPath = Application.dataPath;
                    bool start = false;
                    foreach (var i in split)
                    {
                        if (i.Equals("Assets"))
                        {
                            start = true;
                            continue;
                        }

                        if (start)
                        {
                            newPath += "/" + i;
                        }
                    }

                    fullPath = "file:///" + newPath;
                }

                if (Application.HasUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone))
                {

                }
            }
            */
#endif

            Debug.Log("<color=cyan>StartLoadAsync From Path : " + fullPath + "</color>");

            if( wwwObject.isRequest == true )
            {
                wwwObject.www = UnityWebRequest.Post( fullPath, wwwObject.headers ); //new WWW(fullPath, wwwObject.postData, wwwObject.headers);
            }
            else
            {
                wwwObject.www = UnityWebRequest.Get( fullPath );
            }

            switch( wwwObject.downdloadType )
            {
            case DownLoadTypes.Texture:
                {
                    wwwObject.www.downloadHandler = new DownloadHandlerTexture();
                }
                break;
            case DownLoadTypes.None:
            default:
                break;
            }

            wwwObject.www.SendWebRequest();
            while( wwwObject.www.isDone == false )
            {
                if( wwwObject.progressCallBack != null )
                    wwwObject.progressCallBack( wwwObject.www.downloadProgress );
                yield return null;
            }

            if( wwwObject.progressCallBack != null )
            {
                wwwObject.progressCallBack( 1f );
                wwwObject.progressCallBack = null;
            }
        }

        if( wwwObject.isSaveFile && wwwObject.www != null && wwwObject.www.error == null && wwwObject.www.isDone && wwwObject.www.downloadHandler != null
            && wwwObject.www.downloadHandler.data != null && wwwObject.www.downloadHandler.data.Length > 0 )
        {
            if (!IsExistCacheFile(wwwObject.privateKey))
            {
                if (m_wwwCache.SaveCache(ClientVersion.RefCachName, wwwObject.www.downloadHandler.data, true))
                {
                    Debug.Log("<color=cyan>cache saved!!!! " + ClientVersion.RefCachName + "</color>");
                }
            }
        }

        var cacheFullPath = m_wwwCache.MakeStringCachePath(wwwObject.privateKey);
        if (cacheFullPath.Contains(DataManager.REFSFILE_EXTENTION))
        {
            ClientVersion.ResFilePath = cacheFullPath;
        }

        
        m_wwwLoading.DeleteWWWObject( wwwObject );

        if( !wwwObject.isStop )
            AddCallback( wwwObject );

        yield return 0;
    }

    public byte[] GetCacheData_Byte()
    {
        return m_wwwCache.LoadCache(ClientVersion.RefCachName);
    }

    public string GetCacheData_FilePath()
    {
        return m_wwwCache.MakeStringCachePath(ClientVersion.RefCachName);
    }


    protected IEnumerator StartLoadTimeOutAsync( WWWObject wwwObject )
    {
        string fullPath = "";

        fullPath = MakeWebPath( wwwObject.wwwPath );

#if UNITY_EDITOR
        Debug.Log( "StartLoadTimeOutAsync From Path : " + fullPath );
#endif        
        if( wwwObject.isRequest == true )
        {
            wwwObject.www = UnityWebRequest.Post( fullPath, wwwObject.headers );
        }
        else
        {
            wwwObject.www = UnityWebRequest.Get( fullPath );
        }

        switch( wwwObject.downdloadType )
        {
        case DownLoadTypes.Texture:
            {
                wwwObject.www.downloadHandler = new DownloadHandlerTexture();
            }
            break;
        case DownLoadTypes.None:
        default:
            break;
        }

        wwwObject.isTimeOut = false;
        if( true == wwwObject.isTimeCheck )
        {
            float fTimer = Time.realtimeSinceStartup;
            wwwObject.www.SendWebRequest();
            while( false == wwwObject.www.isDone )
            {
                if( _cfTimeOutMaxTime <= ( Time.realtimeSinceStartup - fTimer ) )
                {
                    Debug.Log( "wwwObject.isTimeOut" );

                    //wwwObject.www.Dispose();
                    wwwObject.isTimeOut = true;
                    wwwObject.www = null;

                    break;
                }

                yield return null;
                //fTimer += Time.deltaTime;
            }
        }
        else
        {
            yield return wwwObject.www.SendWebRequest();
        }

        m_wwwTitleLoading.DeleteWWWObject( wwwObject );

        if( !wwwObject.isStop )
        {
            if( wwwObject.onCallback != null )
            {
                m_qTitleCallback.Enqueue( wwwObject );
            }
        }

        yield return 0;
    }

    protected void DeleteWWWObject( string privateKey )
    {
        m_wwwMemory.DeleteWWWObject( privateKey );
    }

    protected WWWObject GetWWWObject( string privateKey )
    {
        WWWObject wwwObject = null;

        wwwObject = m_wwwMemory.GetWWWObjectWithPrivateKey( privateKey );

        return wwwObject;
    }


    protected void ClearWWW()
    {
        m_wwwMemory.ClearWWW();
    }

    private void UpdateWWWStartTime()
    {
        if( m_lastTotalLoadinCount < m_wwwLoading.TotalLoadingCount )
        {
            m_ignoreTimeScaleTimer.Stop();
            m_ignoreTimeScaleTimer.Play();
        }
        else if( !IsWWWLoading() )
        {
            m_ignoreTimeScaleTimer.Stop();
        }

        m_lastTotalLoadinCount = m_wwwLoading.TotalLoadingCount;
    }

    private bool IsExistMemory( string privateKey )
    {
        return m_wwwMemory.IsExistPrivateKey( privateKey );
    }

    private bool IsExistCacheFile( string privateKey )
    {
        return m_wwwCache.IsExistFile( privateKey );
    }

    public string GetCachPath()
    {
        if( m_wwwCache != null )
            return m_wwwCache.MakeStringCacheDirectoryPath();

        return string.Empty;
    }

    private void AddCallback( WWWObject wwwObject )
    {
        if( wwwObject.onCallback != null )
        {
            m_qCallback.Enqueue( wwwObject );
        }
    }
    private string MakeWebPath( string filePath )
    {
        string fullPath = "";

        if( IsProtocolPath( filePath ) )
        {
            fullPath = filePath;
        }

        else
        {
            fullPath = m_baseWebPath + filePath;
        }

        return ( fullPath );
    }

    public static bool IsProtocolPath( string wwwPath )
    {
        bool isProtocol = false;

        if (wwwPath.Contains("http://"))
        {
            isProtocol = true;
        }

        else if (wwwPath.Contains("https://"))
        {
            isProtocol = true;
        }

        else if (wwwPath.Contains("file://"))
        {
            isProtocol = true;
        }
        else if (wwwPath.Contains("file:///"))
        {
            isProtocol = true;
        }
        else if (wwwPath.Contains("cloudfront"))
        {
            isProtocol = true;
        }

        return isProtocol;
    }

    public WWWRequest WWWRequest
    {
        get
        {
            return m_wwwRequest;
        }
    }

    public static bool IsSuccessWWWObject( WWWObject wwwObject )
    {
        return ( wwwObject != null && wwwObject.www != null && string.IsNullOrEmpty( wwwObject.www.error ) );
    }

    public delegate void CallbackLoadComplete( WWWObject wwwObject );
    public static float _cfTimeOutMaxTime = 30.0f;

    private WWWMemory m_wwwMemory = new WWWMemory();
    private WWWCache m_wwwCache = new WWWCache();
    private WWWLoading m_wwwLoading = new WWWLoading();
    private WWWRequest m_wwwRequest = new WWWRequest();
    private WWWLoading m_wwwTitleLoading = new WWWLoading();
    private Queue<WWWObject> m_qTitleCallback = new Queue<WWWObject>();

    private Queue<WWWObject> m_qCallback = new Queue<WWWObject>();

    private string m_baseWebPath;

    private int m_lastTotalLoadinCount = 0;

    private IgnoreTimeScaleTimer m_ignoreTimeScaleTimer;

    private static WWWManager m_instance = null;
}
