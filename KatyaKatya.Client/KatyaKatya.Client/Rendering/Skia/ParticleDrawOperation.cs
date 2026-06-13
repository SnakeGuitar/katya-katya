using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using KatyaKatya.Controls;
using SkiaSharp;

namespace KatyaKatya.Rendering.Skia;

/// <summary>
/// Long-lived Skia resources shared by every particle frame: the vector shapes, their
/// gradient shaders, the score font, and a scratch paint. Created once per
/// <see cref="ParticleCanvas"/> and reused across frames (zero per-frame allocation).
/// The scratch paint is only ever touched on the render thread inside
/// <see cref="ParticleDrawOperation.Render"/>, which is serialized, so sharing it is safe.
/// </summary>
internal sealed class ParticleResources : IDisposable
{
    public readonly SKPath Heart;
    public readonly SKPath Star;
    public readonly SKPath Sparkle;
    public readonly SKShader HeartShader;
    public readonly SKShader StarShader;
    public readonly SKShader SparkleShader;
    public readonly SKTypeface Typeface;
    public readonly SKFont Font;
    public readonly SKPaint Paint;

    public ParticleResources()
    {
        Heart = CreateHeartPath(16f);
        Star = CreateStarPath(16f, 6.5f);
        Sparkle = CreateSparklePath(16f);

        HeartShader = CreateGradient(new SKColor(255, 105, 180), new SKColor(255, 182, 193)); // Rose gold
        StarShader = CreateGradient(new SKColor(255, 215, 0), new SKColor(255, 140, 0));       // Magical gold
        SparkleShader = CreateGradient(new SKColor(0, 255, 255), new SKColor(0, 128, 255));    // Electric blue

        Typeface = SKTypeface.FromFamilyName("Nunito")
                   ?? SKTypeface.FromFamilyName("Segoe UI")
                   ?? SKTypeface.Default;
        Font = new SKFont(Typeface, 22f);
        Paint = new SKPaint { IsAntialias = true };
    }

    public (SKPath Path, SKShader Shader) GetShape(ParticleCanvas.ShapeKind kind) => kind switch
    {
        ParticleCanvas.ShapeKind.Star => (Star, StarShader),
        ParticleCanvas.ShapeKind.Sparkle => (Sparkle, SparkleShader),
        _ => (Heart, HeartShader),
    };

    public void Dispose()
    {
        Heart.Dispose();
        Star.Dispose();
        Sparkle.Dispose();
        HeartShader.Dispose();
        StarShader.Dispose();
        SparkleShader.Dispose();
        Font.Dispose();
        Typeface.Dispose();
        Paint.Dispose();
    }

    private static SKShader CreateGradient(SKColor from, SKColor to) =>
        SKShader.CreateLinearGradient(
            new SKPoint(-16, -16), new SKPoint(16, 16),
            [from, to], null, SKShaderTileMode.Clamp);

    private static SKPath CreateHeartPath(float size)
    {
        var path = new SKPath();
        path.MoveTo(0, -size * 0.35f);
        path.CubicTo(-size * 0.5f, -size * 0.8f, -size * 0.9f, -size * 0.2f, 0, size * 0.55f);
        path.CubicTo(size * 0.9f, -size * 0.2f, size * 0.5f, -size * 0.8f, 0, -size * 0.35f);
        path.Close();
        return path;
    }

    private static SKPath CreateStarPath(float radius, float innerRadius)
    {
        var path = new SKPath();
        const int points = 5;
        var angleStep = Math.PI / points;
        var angle = -Math.PI / 2;

        path.MoveTo((float)(Math.Cos(angle) * radius), (float)(Math.Sin(angle) * radius));
        for (var i = 0; i < points * 2; i++)
        {
            angle += angleStep;
            var r = i % 2 == 0 ? innerRadius : radius;
            path.LineTo((float)(Math.Cos(angle) * r), (float)(Math.Sin(angle) * r));
        }
        path.Close();
        return path;
    }

    private static SKPath CreateSparklePath(float radius)
    {
        var path = new SKPath();
        path.MoveTo(0, -radius);
        path.QuadTo(0, 0, radius, 0);
        path.QuadTo(0, 0, 0, radius);
        path.QuadTo(0, 0, -radius, 0);
        path.QuadTo(0, 0, 0, -radius);
        path.Close();
        return path;
    }
}

/// <summary>
/// Immutable per-frame snapshot of everything the particle canvas wants drawn. Filled on
/// the UI thread (where the simulation lists live) and read on the render thread. The
/// canvas keeps two of these and alternates, so the render thread never reads a buffer the
/// UI thread is concurrently writing. Arrays grow on demand and are then reused.
/// </summary>
internal sealed class ParticleScene
{
    public struct Petal { public float X, Y, Size; public SKColor Color; }

    public ParticleCanvas.Shockwave[] Shockwaves = new ParticleCanvas.Shockwave[16];
    public int ShockwaveCount;
    public Petal[] Petals = new Petal[24];
    public int PetalCount;
    public ParticleCanvas.BurstParticle[] Bursts = new ParticleCanvas.BurstParticle[64];
    public int BurstCount;
    public ParticleCanvas.FloatingText[] Texts = new ParticleCanvas.FloatingText[8];
    public int TextCount;

    public void Clear()
    {
        ShockwaveCount = 0;
        PetalCount = 0;
        BurstCount = 0;
        TextCount = 0;
    }

    public void AddShockwave(in ParticleCanvas.Shockwave s)
    {
        if (ShockwaveCount == Shockwaves.Length) Array.Resize(ref Shockwaves, Shockwaves.Length * 2);
        Shockwaves[ShockwaveCount++] = s;
    }

    public void AddPetal(float x, float y, float size, SKColor color)
    {
        if (PetalCount == Petals.Length) Array.Resize(ref Petals, Petals.Length * 2);
        Petals[PetalCount++] = new Petal { X = x, Y = y, Size = size, Color = color };
    }

    public void AddBurst(in ParticleCanvas.BurstParticle b)
    {
        if (BurstCount == Bursts.Length) Array.Resize(ref Bursts, Bursts.Length * 2);
        Bursts[BurstCount++] = b;
    }

    public void AddText(in ParticleCanvas.FloatingText t)
    {
        if (TextCount == Texts.Length) Array.Resize(ref Texts, Texts.Length * 2);
        Texts[TextCount++] = t;
    }
}

/// <summary>
/// Avalonia custom draw operation that paints a <see cref="ParticleScene"/> straight onto
/// the render surface via the SkiaSharp API lease — no intermediate <c>WriteableBitmap</c>,
/// no per-frame bitmap lock, and no full-window pixel copy (the old hot path).
/// </summary>
internal sealed class ParticleDrawOperation : ICustomDrawOperation
{
    private readonly ParticleScene _scene;
    private readonly ParticleResources _res;

    public ParticleDrawOperation(Rect bounds, ParticleScene scene, ParticleResources res)
    {
        Bounds = bounds;
        _scene = scene;
        _res = res;
    }

    public Rect Bounds { get; }

    public bool HitTest(Point p) => false;

    // Each frame produces a fresh operation with new data, so never treat two as equal.
    public bool Equals(ICustomDrawOperation? other) => false;

    public void Dispose() { }

    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature is null)
            return;

        using var lease = leaseFeature.Lease();
        var canvas = lease.SkCanvas;
        var paint = _res.Paint;

        canvas.Save();
        canvas.ClipRect(new SKRect(0, 0, (float)Bounds.Width, (float)Bounds.Height));

        // 1. Shockwaves
        for (var i = 0; i < _scene.ShockwaveCount; i++)
        {
            ref readonly var s = ref _scene.Shockwaves[i];
            var t = s.Age / s.Lifetime;
            paint.Shader = null;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 8f * (1f - t);
            paint.Color = s.Color.WithAlpha((byte)((1f - t) * 255));
            canvas.DrawCircle(s.CenterX, s.CenterY, s.CurrentRadius, paint);
        }

        // 2. Ambient petals
        paint.Style = SKPaintStyle.Fill;
        paint.Shader = null;
        for (var i = 0; i < _scene.PetalCount; i++)
        {
            ref readonly var petal = ref _scene.Petals[i];
            paint.Color = petal.Color;
            DrawHeart(canvas, paint, petal.X, petal.Y, petal.Size);
        }

        // 3. Burst particles with motion-blur trail
        for (var i = 0; i < _scene.BurstCount; i++)
        {
            ref readonly var p = ref _scene.Bursts[i];
            if (p.Age < p.Delay)
                continue;

            var t = Math.Clamp((p.Age - p.Delay) / p.Lifetime, 0f, 1f);
            var angle = p.StartAngle + (p.TargetAngle - p.StartAngle) * t;
            var scale = CalculateScale(t) * (p.Size / 24f);
            var opacity = t > 0.55f ? 1f - (t - 0.55f) / 0.45f : 1f;
            var (path, shader) = _res.GetShape(p.Kind);

            // 3A. Fading trail clones
            for (var h = p.HistoryCount - 1; h >= 0; h--)
            {
                float hx = h == 0 ? p.HistoryX0 : p.HistoryX1;
                float hy = h == 0 ? p.HistoryY0 : p.HistoryY1;

                var indexFactor = (h + 1) / 3f;
                var trailScale = scale * (1f - indexFactor);
                var trailOpacity = opacity * (1f - indexFactor * 0.8f);
                if (trailScale <= 0f || trailOpacity <= 0f)
                    continue;

                canvas.Save();
                canvas.Translate(hx, hy);
                canvas.RotateDegrees(angle);
                canvas.Scale(trailScale, trailScale);
                paint.Shader = shader;
                paint.Color = p.Color.WithAlpha((byte)(trailOpacity * 255));
                canvas.DrawPath(path, paint);
                canvas.Restore();
            }

            // 3B. Main particle
            canvas.Save();
            canvas.Translate(p.X, p.Y);
            canvas.RotateDegrees(angle);
            canvas.Scale(scale, scale);
            paint.Shader = shader;
            paint.Color = p.Color.WithAlpha((byte)(opacity * 255));
            canvas.DrawPath(path, paint);
            canvas.Restore();
        }

        paint.Shader = null;

        // 4. Floating score texts — dark outline + bright fill
        for (var i = 0; i < _scene.TextCount; i++)
        {
            ref readonly var ft = ref _scene.Texts[i];
            var t = ft.Age / ft.Lifetime;
            var opacity = 1f - t * t;
            var alpha = (byte)(Math.Clamp(opacity, 0f, 1f) * 255);
            if (alpha == 0)
                continue;

            var scale = GetBounceScale(t) * ft.Scale;

            canvas.Save();
            canvas.Translate(ft.X, ft.Y);
            canvas.Scale(scale, scale);

            var textX = -_res.Font.MeasureText(ft.Text) / 2f;

            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 4.5f;
            paint.Color = SKColors.Black.WithAlpha(alpha);
            canvas.DrawText(ft.Text, textX, 0f, _res.Font, paint);

            paint.Style = SKPaintStyle.Fill;
            paint.Color = ft.Color.WithAlpha(alpha);
            canvas.DrawText(ft.Text, textX, 0f, _res.Font, paint);

            canvas.Restore();
        }

        paint.Style = SKPaintStyle.Fill;
        canvas.Restore();
    }

    // ── Easing curves (ported from the WPF engine) ────────────────────────

    private static float CalculateScale(float t)
    {
        if (t <= 0.2f)
        {
            var p = t / 0.2f;
            return 0.3f + 0.9f * (p * (2f - p));
        }
        if (t <= 0.4f)
        {
            var p = (t - 0.2f) / 0.2f;
            return 1.2f - 0.2f * (p * (2f - p));
        }
        var q = (t - 0.4f) / 0.6f;
        return 1.0f - q * (2f - q);
    }

    private static float GetBounceScale(float t)
    {
        if (t < 0.3f) return t / 0.3f * 1.3f;
        if (t < 0.6f) return 1.3f - 0.35f * ((t - 0.3f) / 0.3f);
        if (t < 0.8f) return 0.95f + 0.1f * ((t - 0.6f) / 0.2f);
        return 1.05f - 0.05f * ((t - 0.8f) / 0.2f);
    }

    private static void DrawHeart(SKCanvas canvas, SKPaint paint, float x, float y, float s)
    {
        using var path = new SKPath();
        path.MoveTo(x, y + s * 0.35f);
        path.CubicTo(x - s * 1.35f, y - s * 0.5f, x - s * 0.85f, y - s * 1.45f, x, y - s * 0.75f);
        path.CubicTo(x + s * 0.85f, y - s * 1.45f, x + s * 1.35f, y - s * 0.5f, x, y + s * 0.35f);
        path.Close();
        canvas.DrawPath(path, paint);
    }
}
