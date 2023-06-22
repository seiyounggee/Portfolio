using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class PostLoadingAssetInfo
{
    public System.Type asset_type;
    public string      asset_name;

    public object      variable;
    public string      variable_name;
    public int         number;
}

public class AssetIOUtil 
{
    public const string SEPERATOR_ONEPIECE  = "^";
    public const string SEPERATOR_BASEASSET = "@";
    public const string SEPERATOR_SCRIPTDATA = "#";
    public const string SEPERATOR_LOOKUPTABLE = ":";

#if UNITY_EDITOR    
    public static bool IsSaveData( FieldInfo field_info )
    {
        if( field_info.IsStatic == true ){
            return false;
        }

        if( (field_info.Attributes & FieldAttributes.NotSerialized) > 0 ){
            return false;
        }

        if( field_info.IsPublic == true ){
            return true;
        }

        bool _save = field_info.IsDefined(typeof(SerializeField), false);
        return _save;
    }

    static void GetHierachy( GameObject[] roots, Transform target, ref List<Transform> list_path )
    {
        list_path.Add( target );

        for (int i = 0; i < roots.Length; ++i)
        {
            if (roots[i] == target.gameObject){
                return;
            }
        }

        if (target.parent != null)
        {
            GetHierachy(roots, target.parent, ref list_path );
        }
    }

    public static string GetPathAsString( GameObject[] roots, Transform target )
    {
        string _re = string.Empty;

        if (target != null)
        {
            List<Transform> _listPath = new List<Transform>();
            GetHierachy(roots, target, ref _listPath);

            if (_listPath.Count > 0)
            {
                for( int i=0, _max = roots.Length; i < _max; ++i )
                {
                    if (roots[i].transform == _listPath[_listPath.Count - 1] )
                    {
                        _re = i.ToString();
                        break;
                    }
                }

                if (_listPath.Count > 1)
                {
                    for (int i = _listPath.Count - 2; i >= 0; --i)
                    {
                        Transform _parent = _listPath[i + 1];
                        for (int j = 0, _maxj = _parent.childCount; j < _maxj; ++j)
                        {
                            if (_parent.GetChild(j) == _listPath[i])
                            {
                                _re = string.Format("{0}/{1}", _re, j);
                            }
                        }
                    }
                }
            }
        }

        return _re;
    }

    public static void Serialize( GameObject[] roots, System.Type type, string name, object value, Dictionary<string,object> out_dic, ref Dictionary<string,UnityEngine.Object> dic_attach )
    {
        if (type.IsGenericType)
        {
            Dictionary<string,object> _dic_sub = new Dictionary<string, object>();

            System.Type _generic_type = type.GetGenericTypeDefinition();
            if( _generic_type == typeof(List<>) )
            {
                var _data_type = type.GetGenericArguments()[0];

                int _count = 0;
                var _list = value as IList;
                var _enum = _list.GetEnumerator();
                while (_enum.MoveNext())
                {
                    AssetIOUtil.Serialize(roots, _data_type, _count.ToString(), _enum.Current, _dic_sub, ref dic_attach);
                    _count++;
                }

                bool _is_added = false;
                if (_dic_sub.Count > 0)
                {
                    var _enum_data = _dic_sub.GetEnumerator();
                    _enum_data.MoveNext();
                    if (_enum_data.Current.Value.GetType() != typeof(string) ||
                        string.IsNullOrEmpty((string)_enum_data.Current.Value) == false)
                    {
                        _is_added = true;
                        out_dic.Add(name, _dic_sub);
                    }
                }

                if (_is_added == false)
                {
                    out_dic.Add(name, string.Empty);
                }
            }
            else
            {
                Debug.LogError("No supported!!");
            }
        }
        else if (type.IsArray)
        {
            Dictionary<string,object> _dic_sub = new Dictionary<string, object>();

            if( value != null)
            {
                var _ienum = value as IEnumerable;
                var _enum = _ienum.GetEnumerator();

                int _count = 0;
                while( _enum.MoveNext() )
                {
                    object _data = _enum.Current;
                    AssetIOUtil.Serialize( roots, _data.GetType(), _count.ToString(), _data, _dic_sub, ref dic_attach );
                    _count++;
                }
            }

            bool _is_added = false;
            if (_dic_sub.Count > 0)
            {
                var _enum_data = _dic_sub.GetEnumerator();
                _enum_data.MoveNext();
                if (_enum_data.Current.Value.GetType() != typeof(string) ||
                    string.IsNullOrEmpty((string)_enum_data.Current.Value) == false)
                {
                    _is_added = true;
                    out_dic.Add(name, _dic_sub);
                }
            }

            if (_is_added == false)
            {
                out_dic.Add(name, string.Empty);
            }
        }
        else if (type.IsEnum)
        {
            out_dic.Add(name, value.ToString());
        }
#if NGUI
        //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        // check attach to assetbundle 
        else if (type == typeof(UIAtlas))
        {
            Debug.LogWarning("UIAtlas Included!! : " + name);
            UIAtlas _atlas = (UIAtlas)value;
            string _res_name = (_atlas != null) ? _atlas.name : string.Empty;
            out_dic.Add(name, _res_name);

            SetAttachedResource<UIAtlas>(_atlas, _res_name, ref dic_attach);
        }
        //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
#endif
        else if (type.IsSubclassOf(typeof(Component)) == true)
        {
            Component _component = value as Component;
            string _path = GetPathAsString(roots, (_component != null) ? _component.transform : null);
            out_dic.Add(name, _path);
        }
        else if (type.IsSubclassOf(typeof(System.Delegate)) == true)
        {
            if (value != null)
            {
                Debug.LogError("Delegate Exist : " + name);
            }
            //  out_dic.Add(name, value);
            return;
        }
  //    else if (type.IsSerializable && type.IsClass && type.Module.Name.Contains("CSharp"))
        else if (type.IsClass && type.Module.Name.Contains("CSharp"))
        {   // other script..
            Dictionary<string,object> _dic_sub = new Dictionary<string, object>();

            //  FieldInfo[] _infos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] _infos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0, _max = _infos.Length; i < _max; ++i)
            {
                if( AssetIOUtil.IsSaveData(_infos[i]) == false ){
                    continue;
                }

                AssetIOUtil.Serialize(roots, _infos[i].FieldType, _infos[i].Name, _infos[i].GetValue(value), _dic_sub, ref dic_attach);
            }

            bool _is_added = false;
            if (_dic_sub.Count > 0)
            {
                var _enum_data = _dic_sub.GetEnumerator();
                _enum_data.MoveNext();
                if (_enum_data.Current.Value.GetType() != typeof(string) ||
                    string.IsNullOrEmpty((string)_enum_data.Current.Value) == false)
                {
                    _is_added = true;
                    out_dic.Add(name, _dic_sub);
                }
            }

            if (_is_added == false)
            {
                out_dic.Add(name, string.Empty);
            }
        }
        else if (type == typeof(Transform))
        {
            Transform _trans = value as Transform;
            string _path = GetPathAsString(roots, _trans);

            out_dic.Add(name, _path);
        }
        else if (type == typeof(GameObject))
        {
            GameObject _obj = value as GameObject;
            string _path = GetPathAsString(roots, (_obj != null) ? _obj.transform : null);

            out_dic.Add(name, _path);
        }
        else if (type == typeof(Color))
        {
            Color _color = (Color)value;
            out_dic.Add(name, _color.ToString());
        }
        else if (type == typeof(AudioSource))
        {
            AudioSource _source = value as AudioSource;
            string _path = GetPathAsString(roots, (_source != null) ? _source.transform : null);

            out_dic.Add(name, _path);
        }
        else if (type == typeof(Animation))
        {
            Animation _ani = value as Animation;
            string _path = GetPathAsString(roots, (_ani != null) ? _ani.transform : null);

            out_dic.Add(name, _path);
        }
        else if (type == typeof(UnityEngine.Bounds))
        {
            UnityEngine.Bounds _bounds = (UnityEngine.Bounds)value;

            var _sub = new Dictionary<string, object>();
            _sub.Add("Center", _bounds.center.ToString());
            _sub.Add("Extents", _bounds.extents.ToString());

            out_dic.Add(name, _sub);
        }
        else if (type == typeof(Vector2))
        {
            Vector2 _vector = (Vector2)value;
            out_dic.Add(name, _vector.ToString("G"));
        }
        else if (type == typeof(Vector3))
        {
            Vector3 _vector = (Vector3)value;
            out_dic.Add(name, _vector.ToString("G"));
        }
        else if (type == typeof(Vector4))
        {
            Vector4 _vector = (Vector4)value;
            out_dic.Add(name, _vector.ToString("G"));
        }
        //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        // check attach to assetbundle 
        else if (type == typeof(Texture))
        {
            Texture _tex     = value as Texture;
            string _res_name = (_tex != null) ? _tex.name : string.Empty;
            out_dic.Add(name, _res_name);

            SetAttachedResource<Texture>(_tex, _res_name, ref dic_attach);
        }
        else if (type == typeof(AudioClip))
        {
            AudioClip _clip  = value as AudioClip;
            string _res_name = (_clip != null) ? _clip.name : string.Empty;
            out_dic.Add(name, _res_name );

            SetAttachedResource<AudioClip>(_clip, _res_name, ref dic_attach);
        }
        else if (type == typeof(AnimationClip))
        {
            AnimationClip _clip = value as AnimationClip;
            string _res_name    = (_clip != null) ? _clip.name : string.Empty;
            out_dic.Add(name, _res_name );

            SetAttachedResource<AnimationClip>(_clip, _res_name, ref dic_attach);
        }
        else if (type == typeof(Shader))
        {
            Shader _shader   = (Shader)value;
            string _res_name = (_shader != null) ? _shader.name : string.Empty;
            out_dic.Add(name, _res_name );

            SetAttachedResource<Shader>(_shader, _res_name, ref dic_attach);
        }
        else if (type == typeof(Font))
        {
            Font _font       = (Font)value;
            string _res_name = (_font != null) ? _font.name : string.Empty;
            out_dic.Add(name, _res_name );

            SetAttachedResource<Font>(_font, _res_name, ref dic_attach);
        }
        else if (type == typeof(Material))
        {
            Material _mat = (Material)value;
            string _res_name = (_mat != null) ? _mat.name : string.Empty;
            out_dic.Add(name, _res_name );

            SetAttachedResource<Material>(_mat, _res_name, ref dic_attach);
        }
        //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        else
        {
            out_dic.Add( name, value );
        }
    }

    static void SetAttachedResource<T>( T target, string name, ref Dictionary<string,UnityEngine.Object> dic_attach )
    {
        if( string.IsNullOrEmpty( name ) == true || dic_attach == null )
        {
            return;
        }
        
        if( dic_attach.ContainsKey(name) == false )
        {
            dic_attach.Add(name, target as UnityEngine.Object );
        }
        else
        {
            var _prev_type = dic_attach[name].GetType();
            if( _prev_type != typeof(T) )
            {
                Debug.Log( "Same Resourse File Name  : " + name );
            }
        }
    }
#endif

    //
    public static void ApplyLoadedAsset( PostLoadingAssetInfo target, UnityEngine.Object loaded_asset )
    {
        if( target == null || loaded_asset == null )
            return;

        var _type = target.variable.GetType();
        if (_type.IsGenericType)
        {
            var _list = target.variable as IList;
            if (_list.Count <= target.number-1)
            {
                for (int i = 0, _max = target.number - 1 - _list.Count; i < _max; ++i)
                {
                    _list.Add(null);
                }

                _list.Add(loaded_asset);
            }
            else
            {
                _list[ target.number - 1] = loaded_asset;
            }

        }
        else if (_type.IsArray)
        {
            var _array = target.variable as System.Array;
            if (_array != null)
            {
                _array.SetValue(loaded_asset, target.number - 1);
            }
        }
        else
        {
            var _field_info = _type.GetField(target.variable_name, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance );
            if (_field_info != null)
            {
                if( _field_info.FieldType == loaded_asset.GetType() )
                    _field_info.SetValue(target.variable, loaded_asset);
            }
        }
    }

    public static bool CheckScriptData( List<PostLoadingAssetInfo> result_listDataToLoad, GameObject root )
    {
        AssetBundleEtcData _scriptData = root.GetComponent<AssetBundleEtcData>();
        return ApplyScriptData(result_listDataToLoad, _scriptData);
    }

    public static bool CheckScriptData( List<PostLoadingAssetInfo> result_listDataToLoad, GameObject[] roots )
    {
        AssetBundleEtcData _scriptData = null;
        for (int i = 0, _max = roots.Length; i < _max; ++i)
        {
            if( roots[i] == null )
                continue;

            _scriptData = roots[i].GetComponent<AssetBundleEtcData>();
            if( _scriptData != null ){
                break;
            }
        }

        return ApplyScriptData(result_listDataToLoad, _scriptData);
    }

    static bool ApplyScriptData( List<PostLoadingAssetInfo> result_listDataToLoad, AssetBundleEtcData data_script )
    {
        if( data_script == null ){
            return false;
        }

        var _jsonData = MiniJSON.Json.Deserialize( data_script.ScriptData) as Dictionary<string, object >;
        if (_jsonData == default(Dictionary<string,object>) )
        {
            Debug.LogError("Json Deserialize Error !!");
            return false;
        }

        bool isSuccess = false;
        Dictionary<string,UnityEngine.Object> _dicAttachedAsset = null;
        if( data_script.objAttached != null && data_script.objAttached.Length > 0 )
        {
            _dicAttachedAsset = new Dictionary<string, Object>();
            for (int i = 0, _max = data_script.objAttached.Length; i < _max; ++i)
            {
                if( _dicAttachedAsset.ContainsKey( data_script.objAttached[i].name ) == false )
                    _dicAttachedAsset.Add(data_script.objAttached[i].name, data_script.objAttached[i]);
            }
        }

        if( data_script.objLists != null && data_script.objLists.Length > 0 )
        {
            ParseScriptData( result_listDataToLoad, _jsonData, data_script.objLists, _dicAttachedAsset );
            isSuccess = true;
        }
        else
        {
            Debug.LogError( "ParseScriptData RootList Empty" );
            isSuccess = false;
        }

        UnityEngine.Object.Destroy( data_script );
        return isSuccess;
    }

    static void ParseScriptData( List<PostLoadingAssetInfo> result_listDataToLoad, Dictionary<string,object> script_data, GameObject[] roots, Dictionary<string,UnityEngine.Object> dic_attached )
    {
        Debug.Log( "Parse Json Script Data!" );

        List< KeyValuePair< MonoBehaviour, Dictionary<string,object>>>  _loaded_data = new List< KeyValuePair<MonoBehaviour, Dictionary<string,object>>>();

        Dictionary<string,object> _data = null;
        MonoBehaviour _script = null;
        GameObject _target_obj = null;
        string _pos = string.Empty;
        string _script_name = string.Empty;
        System.Type _type;
        // attach script to Scene Asset
        var _enum = script_data.GetEnumerator();
        while (_enum.MoveNext())
        {
            _script_name = _enum.Current.Key.Split(AssetIOUtil.SEPERATOR_SCRIPTDATA[0])[0];

            _type = System.Type.GetType(_script_name);
            if (_type != null)
            {
                _data = (Dictionary<string,object>)_enum.Current.Value;
                if( _data != null && _data.ContainsKey( "pos" ) && _data.ContainsKey( "data" ) )
                {
                    _pos = ( string )_data[ "pos" ];

                    _target_obj = AssetIOUtil.GetTargetObjectByPath( roots, _pos );
                    if( _target_obj != null )
                    {
                        _script = _target_obj.AddComponent( _type ) as MonoBehaviour;
                        if( _script != null )
                            _loaded_data.Add( new KeyValuePair<MonoBehaviour, Dictionary<string, object>>( _script, ( Dictionary<string, object> )_data[ "data" ] ) );
                    }
                }
            }
        }

        object _re = null;
        FieldInfo[] _infos = null;
        // update script data 
        for (int i = 0, _max = _loaded_data.Count; i < _max; ++i)
        {
            _script = _loaded_data[i].Key;
            if( _script == null )
                continue;

            _data = _loaded_data[i].Value;
            if( _data == null )
                continue;

            _infos = _script.GetType().GetFields( BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance );
            if( _infos == null )
                continue;

            for (int m = 0, _maxm = _infos.Length; m < _maxm; ++m)
            {
                _type = _infos[m].FieldType;

                _re = Deserialize(roots, _infos[m].FieldType, _infos[m].Name, _data, _script, result_listDataToLoad, dic_attached );
                if( _re == null ){
                    continue;
                }

                if( IsPostLoadingType(_infos[m].FieldType, _re, dic_attached ) == true )
                {
                    if (result_listDataToLoad == null)
                    {
                        Debug.LogError("Not Exist Asset items : " + _infos[m].Name);
                    }
                    else
                    {
                        result_listDataToLoad.Add(new PostLoadingAssetInfo()
                            {
                                asset_type = _infos[m].FieldType,
                                asset_name = _re as string,

                                variable_name = _infos[m].Name,
                                variable = _script,
                                number = 0,
                            });
                    }
                }
                else
                {
                //  _infos[m].SetValue(_script, _re);
                    try
                    {
                        if( _re.GetType() == typeof( string ) && ((string)_re) == "null"){
                            _re = null;
                        }

                        _infos[m].SetValue(_script, _re);
                    }
                    catch( System.Exception )  
                    { 
                         Debug.LogError( string.Format( "Field SetValue Error : {3} to {2} in {0}( {1} )", _script.name, _script.GetType().ToString(), _infos[m].Name, _re ) );
                        _infos[m].SetValue(_script, null);
                    }
                }
            }
        }
    }

    static object Deserialize( GameObject[] roots, 
        System.Type target_type, string data_name, Dictionary<string,object> script_data, object parent_class, List<PostLoadingAssetInfo> result_listDataToLoad,
        Dictionary<string,UnityEngine.Object> dic_attached )
    {
        if (script_data.ContainsKey(data_name) == false)
            return null;

        if (target_type.IsGenericType)
        {
            System.Type _generic_type = target_type.GetGenericTypeDefinition();

            if (_generic_type == typeof(List<>))
            {
                var _dic_sub = script_data[data_name] as Dictionary<string,object>;
                if (_dic_sub == null) {
                    return null;
                }

                FieldInfo _info = parent_class.GetType().GetField(data_name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var _list_valiable = _info.GetValue(parent_class) as IList;
                if( _list_valiable == default(IList) )
                {
                    _list_valiable = System.Activator.CreateInstance( _info.FieldType ) as IList;    
                    _info.SetValue(parent_class, _list_valiable);
                }

                int _count = 0;
                var _data_type = target_type.GetGenericArguments()[0];

                var _enum = _dic_sub.GetEnumerator();
                while (_enum.MoveNext())
                {
                    var _re = Deserialize(roots, _data_type, _count.ToString(), _dic_sub, _list_valiable, result_listDataToLoad, dic_attached);
                    if (_re != null)
                    {
                        if (IsPostLoadingType(_data_type, _re, dic_attached) == true)
                        {
                            if (result_listDataToLoad == null)
                            {
                                Debug.LogError("Not Exist Asset items : " + data_name);
                            }
                            else
                            {
                                result_listDataToLoad.Add(new PostLoadingAssetInfo(){
                                        asset_type = _data_type,
                                        asset_name = _re as string,

                                        variable_name = data_name,
                                        variable = _list_valiable,
                                        number = _count + 1,
                                });
                            }
                        }
                        else
                        {
                            _list_valiable.Add(_re);
                        }
                    }

                    _count++;
                }

                return null;
            }
            else
            {
                Debug.LogError("No suported!!");
            }
        }
        else if (target_type.IsArray)
        {
            var _dic_sub = script_data[data_name] as Dictionary<string,object>;
            if (_dic_sub == null)
            {
                return null;
            }

            System.Type _child_type = target_type.GetElementType();

            int _count = _dic_sub.Count;
            var _array = System.Array.CreateInstance(_child_type, _count);

            for (int i = 0; i < _count; ++i)
            {
                var _child_data = Deserialize(roots, _child_type, i.ToString(), _dic_sub, _array, result_listDataToLoad, dic_attached);
                if (_child_data != null)
                {
                    if (IsPostLoadingType(_child_type, _child_data, dic_attached) == true)
                    {
                        if (result_listDataToLoad == null)
                        {
                            Debug.LogError("Not Exist Asset items : " + data_name);
                        }
                        else
                        {
                            result_listDataToLoad.Add(new PostLoadingAssetInfo(){
                                    asset_type = _child_type,
                                    asset_name = _child_data as string,

                                    variable_name = data_name,
                                    variable = _array,
                                    number = i + 1,
                            });
                        }
                    }
                    else
                    {
                        _array.SetValue(_child_data, i);
                    }
                }
            }

            return _array;
        }
        else if (target_type.IsEnum)
        {
            return System.Enum.Parse(target_type, (string)script_data[data_name]);
        }
#if NGUI
        //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        // check attach to assetbundle 
        else if (target_type == typeof(UIAtlas))
        {
            string _re = script_data[data_name] as string;
            if (string.IsNullOrEmpty(_re) == false)
            {
                if( dic_attached != null && dic_attached.ContainsKey(_re) == true ){
                    return dic_attached[_re];
                }
                else{
                    return _re;
                }
            }
        }
        //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
#endif
        else if (target_type.IsSubclassOf(typeof(Component)))
        {
            var _target = GetTargetObjectByPath(roots, (string)script_data[data_name]);
            if (_target != null)
            {
                return _target.GetComponent(target_type);
            }
        }
  //    else if (target_type.IsSerializable && target_type.IsClass && target_type.Module.Name.Contains("CSharp"))
        else if( target_type.IsClass && target_type.Module.Name.Contains("CSharp") )
        {
            var _dic_sub = script_data[data_name] as Dictionary<string,object>;
            if (_dic_sub == null)
            {
                return null;
            }

            var _class = System.Activator.CreateInstance(target_type);

            FieldInfo[] _infos = target_type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int m = 0, _maxm = _infos.Length; m < _maxm; ++m)
            {
                var _re = Deserialize(roots, _infos[m].FieldType, _infos[m].Name, _dic_sub, _class, result_listDataToLoad, dic_attached);
                if (_re != null)
                {
                    if (IsPostLoadingType(_infos[m].FieldType, _re, dic_attached) == true)
                    {
                        if (result_listDataToLoad == null)
                        {
                            Debug.LogError("Not Exist Asset items : " + _infos[m].Name);
                        }
                        else
                        {
                            result_listDataToLoad.Add(new PostLoadingAssetInfo(){
                                    asset_type = _infos[m].FieldType,
                                    asset_name = _re as string,

                                    variable_name = _infos[m].Name,
                                    variable = _class,
                                    number = 0,
                            });
                        }
                    }
                    else
                    {
                        _infos[m].SetValue(_class, _re);
                    }
                }
            }

            return _class;
        }
        else if (target_type == typeof(Transform))
        {
            var _target = GetTargetObjectByPath(roots, (string)script_data[data_name]);
            if (_target != null)
            {
                return _target.transform;
            }
        }
        else if (target_type == typeof(GameObject))
        {
            var _target = GetTargetObjectByPath(roots, (string)script_data[data_name]);
            if (_target != null)
            {
                return _target;
            }
        }
        else if (target_type == typeof(Color))
        {
            return GetColor((string)script_data[data_name]);
        }
        else if (target_type == typeof(AudioSource))
        {
            var _target = GetTargetObjectByPath(roots, (string)script_data[data_name]);
            if (_target != null)
            {
                return _target.GetComponent<AudioSource>();
            }
        }
        else if (target_type == typeof(Animation))
        {
            var _target = GetTargetObjectByPath(roots, (string)script_data[data_name]);
            if (_target != null)
            {
                return _target.GetComponent<Animation>();
            }
        }
        else if (target_type == typeof(UnityEngine.Bounds))
        {
            var _data = script_data[data_name] as Dictionary<string,object>;
            if (_data == null)
            {
                return null;
            }

            UnityEngine.Bounds _target = new UnityEngine.Bounds();
            if( _data.ContainsKey( "Center" ) )
                _target.center = GetVector3((string)_data["Center"]);
            
            if( _data.ContainsKey( "Extents" ) )
                _target.extents = GetVector3((string)_data["Extents"]);

            return _target;
        }
        else if (target_type == typeof(Vector2))
        {
            return GetVector2((string)script_data[data_name]);
        }
        else if (target_type == typeof(Vector3))
        {
            return GetVector3((string)script_data[data_name]);
        }
        else if (target_type == typeof(Vector4))
        {
            return GetVector4((string)script_data[data_name]);
        }
        else if (target_type == typeof(Rect))
        {
            return GetRect((string)script_data[data_name]);
        }
        //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        // check attach to assetbundle 
        else if (target_type == typeof(Texture))
        {
            string _re = script_data[data_name] as string;
            if (string.IsNullOrEmpty(_re) == false)
            {
                if( dic_attached != null && dic_attached.ContainsKey(_re) == true ){
                    return dic_attached[_re];
                }
                else{
                    return _re;
                }
            }
        }
        else if (target_type == typeof(AudioClip))
        {
            string _re = script_data[data_name] as string;
            if (string.IsNullOrEmpty(_re) == false)
            {
                if( dic_attached != null && dic_attached.ContainsKey(_re) == true ){
                    return dic_attached[_re];
                }
                else{
                    return _re;
                }
            }
        }
        else if (target_type == typeof(AnimationClip))
        {
            string _re = script_data[data_name] as string;
            if (string.IsNullOrEmpty(_re) == false)
            {
                if( dic_attached != null && dic_attached.ContainsKey(_re) == true ){
                    return dic_attached[_re];
                }
                else{
                    return _re;
                }
            }
        }
        else if (target_type == typeof(Shader))
        {
            string _re = script_data[data_name] as string;
            if( dic_attached != null && dic_attached.ContainsKey(_re) == true ){
                return dic_attached[_re];
            }
            else
            {
                //    return _re;
                return Shader.Find(_re);
            }
        }
        else if (target_type == typeof(Font))
        {
            string _re = script_data[data_name] as string;
            if (string.IsNullOrEmpty(_re) == false)
            {
                if( dic_attached != null && dic_attached.ContainsKey(_re) == true ){
                    return dic_attached[_re];
                }
                else{
                    return _re;
                }
            }
        }
        else if (target_type == typeof(Material))
        {
            string _re = script_data[data_name] as string;
            if (string.IsNullOrEmpty(_re) == false)
            {
                if( dic_attached != null && dic_attached.ContainsKey(_re) == true ){
                    return dic_attached[_re];
                }
                else{
                    return _re;
                }
            }
        }
        //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        else
        {
            return script_data[data_name];
        }

        return null;
    }   

    public static bool IsPostLoadingType( System.Type type, object obj_returned, Dictionary<string,UnityEngine.Object> dic_attached )
    {
        // check get from attached data
        if( dic_attached != null )
        {
            System.Type _returned = obj_returned.GetType();
            if (_returned != typeof(string))
            {
                var _data = obj_returned as UnityEngine.Object;
                if (_data != null)
                {
                    if (dic_attached.ContainsKey(_data.name))
                    {
                        return false;
                    }
                }
            }
        }

        //
        if (type == typeof(AudioClip) || type == typeof(Texture) || type == typeof(AnimationClip) || type == typeof(Font))
        {
            return true;
        }

        return false;
    }

    public static GameObject GetTargetObjectByPath( GameObject[] roots, string path )
    {
        if (string.IsNullOrEmpty(path))
            return null;

        path = path.Replace("\\", "/");
        
        string[] _parts = path.Split("/"[0]);

        Transform _target_root = null;

        for (int i = 0, _max = _parts.Length; i < _max; ++i)
        {
            int _index = int.Parse(_parts[i]);

            if (i == 0)
            {
                if (roots.Length <= _index)
                {
                    Debug.LogError( string.Format( "Cant Find Root Index ({0}): ", path, _index) );
                    return null;
                }
                
                _target_root = roots[_index].transform;
            }
            else
            {
                if (_target_root.childCount <= _index)
                {
                    Debug.LogError( string.Format( "Cant Find Root Index ({0}): ", path, _index) );
                    return null;
                }

                _target_root = _target_root.GetChild(_index);
            }
        }

        if (_target_root != null)
        {
            return _target_root.gameObject;
        }

        return null;
    }

    public static Vector2 GetVector2( string data )
    {
        string[] _temp = data.Substring(1, data.Length - 2).Split(',');
        return new Vector2(float.Parse(_temp[0]), float.Parse(_temp[1]));
    }

    public static Vector3 GetVector3( string data )
    {
        string[] _temp = data.Substring(1, data.Length - 2).Split(',');
        return new Vector3(float.Parse(_temp[0], System.Globalization.CultureInfo.InvariantCulture), 
            float.Parse(_temp[1], System.Globalization.CultureInfo.InvariantCulture), 
            float.Parse(_temp[2], System.Globalization.CultureInfo.InvariantCulture));
    }

    public static Vector4 GetVector4( string data )
    {
        string[] _temp = data.Substring(1, data.Length - 2).Split(',');
        return new Vector4(float.Parse(_temp[0], System.Globalization.CultureInfo.InvariantCulture), 
            float.Parse(_temp[1], System.Globalization.CultureInfo.InvariantCulture), 
            float.Parse(_temp[2], System.Globalization.CultureInfo.InvariantCulture), 
            float.Parse(_temp[3], System.Globalization.CultureInfo.InvariantCulture));
    }

    static Color GetColor( string data )
    {
        if (data.Contains("RGBA") == true)
        {
            string[] _temp = data.Substring( 5, data.Length - 6).Split(',');
            return new Color(float.Parse(_temp[0], System.Globalization.CultureInfo.InvariantCulture), 
                float.Parse(_temp[1], System.Globalization.CultureInfo.InvariantCulture), 
                float.Parse(_temp[2], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(_temp[3], System.Globalization.CultureInfo.InvariantCulture));
        }
        else
        {
            string[] _temp = data.Substring( 4, data.Length - 5).Split(',');
            return new Color(float.Parse(_temp[0], System.Globalization.CultureInfo.InvariantCulture), 
                float.Parse(_temp[1], System.Globalization.CultureInfo.InvariantCulture), 
                float.Parse(_temp[2], System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    static Rect GetRect( string data )
    {
        data = data.Substring(1, data.Length - 2);

        float _x, _y, _width, _height;
        string[] _temp = data.Split(","[0]);

        string[] _split;
        _split = _temp[0].Split(":"[0]);
        _x = float.Parse(_split[1], System.Globalization.CultureInfo.InvariantCulture);

        _split = _temp[1].Split(":"[0]);
        _y = float.Parse(_split[1], System.Globalization.CultureInfo.InvariantCulture);

        _split = _temp[2].Split(":"[0]);
        _width = float.Parse(_split[1], System.Globalization.CultureInfo.InvariantCulture);

        _split = _temp[3].Split(":"[0]);
        _height = float.Parse(_split[1], System.Globalization.CultureInfo.InvariantCulture);

        var _new = new Rect( _x, _y, _width, _height );
        return _new;
    }
	
}
