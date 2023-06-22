using System.Collections.Generic;

public class AssetDefines 
{
    public enum eOSType
    {
        Invalid = -1,

        Android,
        iOS,
        WebGL,
        Standalone,

        Max,
    }

    public static eOSType GetOSType()
    {
#if UNITY_ANDROID
        return eOSType.Android;
#elif UNITY_IOS
        return eOSType.iOS;
#elif UNITY_WEBGL
        return eOSType.WebGL;
#elif UNITY_STANDALONE
        return eOSType.Standalone;
#else
        return eOSType.Invalid;
#endif
    }

#if UNITY_EDITOR
    public static eOSType GetOSType( UnityEditor.BuildTarget target )
    {
        eOSType _re = eOSType.Invalid;
        switch (target)
        {
            case UnityEditor.BuildTarget.Android:
                _re = eOSType.Android;
                break;
            case UnityEditor.BuildTarget.iOS:
                _re = eOSType.iOS;
                break;
            case UnityEditor.BuildTarget.WebGL:
                _re = eOSType.WebGL;
                break;
            case UnityEditor.BuildTarget.StandaloneWindows64:
            case UnityEditor.BuildTarget.StandaloneWindows:
                _re = eOSType.Standalone;
                break;
        }
        return _re;
    }
#endif

    //
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ChildAttribute : System.Attribute
    {
        public ChildAttribute(){}
    }

   // [System.AttributeUsage(System.AttributeTargets.Method)]
   // public class FuncLoadDefaultsAttribute : System.Attribute
   // {
   //     public FuncLoadDefaultsAttribute(){}
   // }


    //
    public class AssetObjectEnum 
    {
        public static List<AssetObjectEnum> ListEnums = new List<AssetObjectEnum>();

        /* 리스트 보다 밑에 와야됨 */
        public static readonly AssetObjectEnum None = new AssetObjectEnum( -1 );

        public int Value
        {
            protected set; get;
        }

        public static int Max
        {
            get
            {
                return ListEnums.Count;
            }
        }

        public AssetObjectEnum( int value )
        {
            Value = value;
            AddEnum( this );
        }

        public static void AddEnum( AssetObjectEnum _enum )
        {
            ListEnums.Add( _enum );
        }

      //public static AssetObjectEnum GetEnum( int index )
      //{
      //    if( index < 0 || index >= ListEnums.Count )
      //        return AssetObjectEnum.None;
      //
      //    return ListEnums[ index ];
      //}

        public static AssetObjectEnum[] GetEnums()
        {
            return ListEnums.ToArray();
        }

        public static implicit operator AssetObjectEnum( int value )
        {
            for (int i = 0, _max = ListEnums.Count; i < _max; ++i)
            {
                if (ListEnums[i].Value == value)
                {
                    return ListEnums[i];
                }
            }

            return AssetObjectEnum.None;
        }

        public static implicit operator int( AssetObjectEnum value )
        {
            return value.Value;
        }

       //public static bool operator ==( AssetObjectEnum a1, AssetObjectEnum a2 )
       //{
       //    if( a1.Value == a2.Value )
       //        return true;
       //    else
       //        return false;
       //}
       //
       //public static bool operator !=( AssetObjectEnum a1, AssetObjectEnum a2 )
       //{
       //    if( a1.Value != a2.Value )
       //        return true;
       //    else
       //        return false;
       //}
       //
       //public override bool Equals(AssetObjectEnum o )
       //{
       //    return Value == o.Value;
       //}
       //
       //public override int GetHashCode()
       //{
       //    return base.GetHashCode();
       //}
    }     


    public class ServerEnum : InheritableEnum
    {
        public static readonly ServerEnum None = new ServerEnum(-1, typeof(ServerEnum));

        //
        protected static System.Type m_EnumType = default(System.Type);

        public ServerEnum( int value, System.Type type ) : base(value){
            m_EnumType = type;
        }

        public static implicit operator ServerEnum( int value )
        {
            System.Reflection.FieldInfo[] _fieldinfos = GetCurrFieldInfos( m_EnumType );
            for (int i = 0, _max = _fieldinfos.Length; i < _max; ++i)
            {
                var _enum = (ServerEnum)_fieldinfos[i].GetValue(_fieldinfos[i]);
                if (_enum.Value == value)
                {
                    return _enum;
                }
            }
            
            return ServerEnum.None;
        }


        public static implicit operator ServerEnum( string name )
        {
            System.Reflection.FieldInfo[] _fieldinfos = GetCurrFieldInfos( m_EnumType );
            for (int i = 0, _max = _fieldinfos.Length; i < _max; ++i)
            {
                if (_fieldinfos[i].Name == name)
                {
                    return (ServerEnum)_fieldinfos[i].GetValue(_fieldinfos[i]);
                }
            }
            
            return ServerEnum.None;
        }
    }

#if UNITY_EDITOR
    public class MakeAssetMenuEnum : InheritableEnum
    {
        public static readonly MakeAssetMenuEnum None = new MakeAssetMenuEnum(-2, typeof(MakeAssetMenuEnum));
        public static readonly MakeAssetMenuEnum All  = new MakeAssetMenuEnum(-1, typeof(MakeAssetMenuEnum));

        //
        protected static System.Type m_EnumType = default(System.Type);

        public MakeAssetMenuEnum( int value, System.Type type ) : base(value){
             m_EnumType = type;
        }


        public static implicit operator MakeAssetMenuEnum( int value )
        {
            System.Reflection.FieldInfo[] _fieldinfos = GetCurrFieldInfos( m_EnumType );
            for (int i = 0, _max = _fieldinfos.Length; i < _max; ++i)
            {
                 var _enum = (MakeAssetMenuEnum)_fieldinfos[i].GetValue(_fieldinfos[i]);
                 if (_enum.Value == value)
                 {
                     return _enum;
                 }
            }

            return MakeAssetMenuEnum.None;
        }

        public static implicit operator MakeAssetMenuEnum( string name )
        {
            System.Reflection.FieldInfo[] _fieldinfos = GetCurrFieldInfos( m_EnumType );
            for (int i = 0, _max = _fieldinfos.Length; i < _max; ++i)
            {
                if (_fieldinfos[i].Name == name)
                {
                     return (MakeAssetMenuEnum)_fieldinfos[i].GetValue(_fieldinfos[i]);
                }
            }

            return MakeAssetMenuEnum.None;
        }
    }
#endif
}
