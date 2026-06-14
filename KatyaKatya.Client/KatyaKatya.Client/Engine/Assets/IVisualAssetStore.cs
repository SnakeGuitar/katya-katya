using Avalonia.Media.Imaging;
using SkiaSharp;

namespace KatyaKatya.Engine.Assets;

public interface IVisualAssetStore
{
    int CachedBitmapCount { get; }
    int CachedSkiaImageCount { get; }
    long EstimatedBytes { get; }

    Bitmap GetBitmap(VisualAssetId id, AssetLoadOptions options = default);
    SKImage? GetSkiaImage(VisualAssetId id);
    void PreloadBitmaps(IEnumerable<VisualAssetId> ids, AssetLoadOptions options = default);
    string DebugMetrics { get; }
}

