using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AssetDownloadChecker 
{
    Dictionary<string,float>  m_dicBundleNamesToLoad = new Dictionary<string,float>();
    Action<float>             m_callbackProgress;
    bool                      m_isProcessing = false;

    //
    public void CheckPreloadState( List<string> listToLoad, Action<float> progress = null )
    {
        m_callbackProgress = progress;

        if( listToLoad.Count > 0 )
        {
            for( int i = 0, _max = listToLoad.Count; i < _max; ++i )
            {
                string _bundle_file_name = AssetManager.GetBundleFileName( listToLoad[ i ] );
                if( m_dicBundleNamesToLoad.ContainsKey( _bundle_file_name ) == true )
                    continue;

             // if( AssetManager.Instance.AttachProgressState( true , _bundle_file_name, ( int )SaveFlag.Caching, OnAssetProgress, false ) == false )
                if (AssetManager.Instance.AttachProgressState(true, _bundle_file_name, OnAssetProgress) == false)
                    continue;

                m_dicBundleNamesToLoad.Add( _bundle_file_name, 0.0f );
            }
        }

        if ( listToLoad.Count == 0 || m_dicBundleNamesToLoad.Count == 0)
        {
            m_isProcessing = false;
            m_callbackProgress.SafeInvoke<float>(1.0f);
        }
        else
        {
            m_isProcessing = true;
        }
    }

    public bool IsProcessing()
    {
        return m_isProcessing;
    }

   
    public void Clear()
    {
        m_callbackProgress = null;

        var _enum = m_dicBundleNamesToLoad.GetEnumerator();
        while (_enum.MoveNext())
        {
            AssetManager.Instance.DetachProgressState(_enum.Current.Key, OnAssetProgress );
        }
        m_dicBundleNamesToLoad.Clear();

        m_isProcessing = false;
    }
	
    //
    void OnAssetProgress( string bundle_name, float rate )
    {
        float _rate = 0.0f;
        if( m_dicBundleNamesToLoad.Count > 0 )
        {
            if( m_dicBundleNamesToLoad.ContainsKey( bundle_name ) == true )
            {
                m_dicBundleNamesToLoad[ bundle_name ] = rate;
            }

            float _sum = 0.0f;
            var _enum = m_dicBundleNamesToLoad.GetEnumerator();
            while( _enum.MoveNext() )
            {
                _sum += _enum.Current.Value;
            }

            _rate = _sum / ( float )m_dicBundleNamesToLoad.Count;
        }
        else
        {
            _rate = 1.0f;
        }

        m_callbackProgress.SafeInvoke<float>( _rate );

        if( _rate >= 1.0f )
        {
            Clear();
        }
    }

}
