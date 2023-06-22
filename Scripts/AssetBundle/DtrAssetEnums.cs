
public class DtrAssetEnums : AssetDefines.AssetObjectEnum
{
    public static int AssetIndexValue = 0;

    public static readonly DtrAssetEnums Car = new DtrAssetEnums(AssetIndexValue++);
    public static readonly DtrAssetEnums Character = new DtrAssetEnums(AssetIndexValue++);
    public static readonly DtrAssetEnums Map = new DtrAssetEnums(AssetIndexValue++);

    public DtrAssetEnums(int value) : base(value)
    {
    }
}

public class DtrServerEnum : AssetDefines.ServerEnum
{
    public static readonly DtrServerEnum Dev = new DtrServerEnum(0);
    public static readonly DtrServerEnum QA = new DtrServerEnum(1);
    public static readonly DtrServerEnum Review = new DtrServerEnum(2);
    public static readonly DtrServerEnum Release = new DtrServerEnum(3);

    //
    public DtrServerEnum(int value) : base(value, typeof(DtrServerEnum))
    {
    }
}

#if UNITY_EDITOR

public class DtrAssetMenuEnum : AssetDefines.MakeAssetMenuEnum
{
    private static int AssetIndexValue = 0;

    public static readonly DtrAssetMenuEnum Car = new DtrAssetMenuEnum(AssetIndexValue++);
    public static readonly DtrAssetMenuEnum Character = new DtrAssetMenuEnum(AssetIndexValue++);
    public static readonly DtrAssetMenuEnum Map = new DtrAssetMenuEnum(AssetIndexValue++);

    public DtrAssetMenuEnum(int value) : base(value, typeof(DtrAssetMenuEnum))
    {
    }
}



#endif
