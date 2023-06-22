using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LocalizeUtility 
{
    public static string Localize( this string term)
    {
        if( string.IsNullOrEmpty( term ) == true )
            return string.Empty;

        return I2.Loc.LocalizationManager.GetTermTranslation(term);
    }

    public static string LocalizeFormat( this string term, params object[] args )
    {
        if (string.IsNullOrEmpty(term) == true )
            return string.Empty;

        var _format = I2.Loc.LocalizationManager.GetTermTranslation(term);
        if (string.IsNullOrEmpty(_format) == true)
            return string.Empty;
        
        try
        {
            return string.Format(UtilityCommon.CultureInfo, _format, args );
        }
        catch
        {
            Debug.LogError( "LocalizeFormat : " + _format);
        }

        return string.Empty;
    }

    //
    static Dictionary<MonoBehaviour, List<I2.Loc.LocalizationManager.OnLocalizeCallback>> s_dicEventCallbacks = new Dictionary<MonoBehaviour, List<I2.Loc.LocalizationManager.OnLocalizeCallback>>();

    public static void ObserveLocalizeEvent( MonoBehaviour script, bool is_add, I2.Loc.LocalizationManager.OnLocalizeCallback callback = null )
    {
        if (script == null)
            return;

        if( is_add == false )
        {
            if( s_dicEventCallbacks.ContainsKey(script) == true )
            {
                var _list = s_dicEventCallbacks[script];
                if( _list != null )
                {
                    _list.Clear();
                }
            }
        }
        else if( callback != null )
        {
            if( s_dicEventCallbacks.ContainsKey(script) == false ){
                s_dicEventCallbacks.Add(script, new List<I2.Loc.LocalizationManager.OnLocalizeCallback>());
            }

            callback();

            if( s_dicEventCallbacks[script].Contains( callback ) == false ){
                s_dicEventCallbacks[script].Add(callback);
            }

            I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLocalized;
            I2.Loc.LocalizationManager.OnLocalizeEvent += OnLocalized;
        }
    }

    static void OnLocalized()
    {
        var _enum = s_dicEventCallbacks.GetEnumerator();
        while (_enum.MoveNext())
        {
            var _list = _enum.Current.Value;
            if (_list == null)
                continue;

            for (int i = 0, _max = _list.Count; i < _max; ++i)
            {
                if (_list[i] == null)
                    continue;

                _list[i]();
            }
        }
    }
    //

    public static void ObserveLocalizeEvent( I2.Loc.LocalizationManager.OnLocalizeCallback callback, bool is_add )
    {
        if( is_add == true )
        {
            callback();

            I2.Loc.LocalizationManager.OnLocalizeEvent -= callback;
            I2.Loc.LocalizationManager.OnLocalizeEvent += callback;
        }
        else
        {
            I2.Loc.LocalizationManager.OnLocalizeEvent -= callback;
        }
    }
}
