using System;

public class InheritableEnum : IEquatable<InheritableEnum>
{
    public int Value { protected set; get;}

    //
    public int NameIndex
    {
        get
        {
            System.Reflection.FieldInfo[] _fieldinfos = GetCurrFieldInfos(GetType());
            if( _fieldinfos.Length > 0 )
            {
                System.Array.Sort< System.Reflection.FieldInfo>(_fieldinfos, ( System.Reflection.FieldInfo left, System.Reflection.FieldInfo right) => {
                    var _left  = (InheritableEnum)left.GetValue( left );
                    var _right = (InheritableEnum)right.GetValue( right );

                    return _left.Value.CompareTo( _right.Value );
                });

                for( int i = 0, _max = _fieldinfos.Length; i < _max; ++i )
                {
                    var _target = (InheritableEnum)_fieldinfos[i].GetValue(this);
                    if( Equals( _target ) == true ){
                        return i;
                    }
                }
            }

            return -1;
        }
    }

    public int Max
    {
        get {  return GetNames(this.GetType()).Length;  }
    }

    public int ValidMax
    {
        get
        {
            System.Reflection.FieldInfo[] _fieldinfos = GetCurrFieldInfos(this.GetType());

            int _count = 0;
            for (int i = 0, _max = _fieldinfos.Length; i < _max; ++i)
            {
                var _enum = _fieldinfos[i].GetValue(_fieldinfos[i]) as InheritableEnum;
                if (_enum != null && _enum.Value >= 0)
                {
                    ++_count;
                }
            }

            return _count;
        }
    }

    //
    public InheritableEnum( int value )
    {
        Value = value;
    }

    public override string ToString()
    {
        System.Reflection.FieldInfo[] _fieldinfos = GetCurrFieldInfos(GetType());
        if( _fieldinfos.Length > 0 )
        {
            for( int i = 0, _max = _fieldinfos.Length; i < _max; ++i )
            {
                var _target = (InheritableEnum)_fieldinfos[i].GetValue(this);
                if( Equals( _target ) == true ){
                    return _fieldinfos[i].Name;
                }
            }
        }

        return string.Empty;
    }

    public bool Equals( InheritableEnum other )
    {
        if (other == null){
            return false;
        }

        return this.Value == other.Value;
    }

    protected static System.Reflection.FieldInfo[] GetCurrFieldInfos( Type type )
    {
     //  return type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly );
        return type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy );
    }

    public static string[] GetNames( Type type )
    {
        System.Reflection.FieldInfo[] _fieldinfos = GetCurrFieldInfos(type);

        System.Array.Sort< System.Reflection.FieldInfo>(_fieldinfos, ( System.Reflection.FieldInfo left, System.Reflection.FieldInfo right) => {
            var _left  = (InheritableEnum)left.GetValue( left );
            var _right = (InheritableEnum)right.GetValue( right );
           
            return _left.Value.CompareTo( _right.Value );
        });

        string[] _names = new string[ _fieldinfos.Length];
        for (int i = 0, _max = _names.Length; i < _max; ++i){
            _names[i] = _fieldinfos[i].Name;
        }

        return _names;
    }

    public static implicit operator int( InheritableEnum value )
    {
        return value.Value;
    }
}
