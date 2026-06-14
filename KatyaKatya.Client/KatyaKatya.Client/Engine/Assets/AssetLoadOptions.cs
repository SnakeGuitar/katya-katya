namespace KatyaKatya.Engine.Assets;

public readonly record struct AssetLoadOptions(int? DecodeWidth = null, int? DecodeHeight = null)
{
    public static AssetLoadOptions Width(int width) => new(DecodeWidth: width);
}

