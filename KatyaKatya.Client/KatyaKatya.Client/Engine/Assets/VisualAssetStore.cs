using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;

namespace KatyaKatya.Engine.Assets;

public sealed class VisualAssetStore : IVisualAssetStore, IDisposable
{
    private readonly Dictionary<string, Bitmap> _bitmaps = new();
    private readonly Dictionary<string, SKImage> _skiaImages = new();
    private readonly Bitmap _placeholderBitmap;

    public VisualAssetStore()
    {
        _placeholderBitmap = new WriteableBitmap(
            new PixelSize(1, 1),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);
    }

    public int CachedBitmapCount => _bitmaps.Count;
    public int CachedSkiaImageCount => _skiaImages.Count;
    public long EstimatedBytes { get; private set; }

    public string DebugMetrics =>
        $"assets bmp:{CachedBitmapCount} sk:{CachedSkiaImageCount} ~{EstimatedBytes / 1024}KB";

    public Bitmap GetBitmap(VisualAssetId id, AssetLoadOptions options = default)
    {
        var key = BitmapKey(id, options);
        if (_bitmaps.TryGetValue(key, out var cached))
            return cached;

        try
        {
            using var stream = AssetLoader.Open(new Uri(id.Uri));
            Bitmap bitmap;
            if (options.DecodeWidth is { } width)
                bitmap = Bitmap.DecodeToWidth(stream, width);
            else if (options.DecodeHeight is { } height)
                bitmap = Bitmap.DecodeToHeight(stream, height);
            else
                bitmap = new Bitmap(stream);

            _bitmaps[key] = bitmap;
            EstimatedBytes += bitmap.PixelSize.Width * bitmap.PixelSize.Height * 4L;
            return bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VisualAssetStore] bitmap load failed {id.Uri}: {ex.Message}");
            return _placeholderBitmap;
        }
    }

    public SKImage? GetSkiaImage(VisualAssetId id)
    {
        if (_skiaImages.TryGetValue(id.Uri, out var cached))
            return cached;

        try
        {
            using var stream = AssetLoader.Open(new Uri(id.Uri));
            using var managed = new MemoryStream();
            stream.CopyTo(managed);
            using var data = SKData.CreateCopy(managed.ToArray());
            var image = SKImage.FromEncodedData(data);
            if (image is null)
                return null;

            _skiaImages[id.Uri] = image;
            EstimatedBytes += image.Width * image.Height * 4L;
            return image;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VisualAssetStore] skia load failed {id.Uri}: {ex.Message}");
            return null;
        }
    }

    public void PreloadBitmaps(IEnumerable<VisualAssetId> ids, AssetLoadOptions options = default)
    {
        foreach (var id in ids)
            _ = GetBitmap(id, options);
    }

    public void Dispose()
    {
        foreach (var bitmap in _bitmaps.Values)
            bitmap.Dispose();
        foreach (var image in _skiaImages.Values)
            image.Dispose();
        _placeholderBitmap.Dispose();
        _bitmaps.Clear();
        _skiaImages.Clear();
    }

    private static string BitmapKey(VisualAssetId id, AssetLoadOptions options) =>
        $"{id.Uri}|w:{options.DecodeWidth?.ToString() ?? "-"}|h:{options.DecodeHeight?.ToString() ?? "-"}";
}

