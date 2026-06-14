namespace KatyaKatya.Engine.Assets;

public readonly record struct VisualAssetId(string Uri)
{
    public override string ToString() => Uri;
}

