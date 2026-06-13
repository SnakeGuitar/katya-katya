using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SkiaSharp;

namespace KatyaKatya.Controls;

/// <summary>
/// Skia-backed particle overlay: ambient falling petals plus a combo-aware
/// "juice" engine for match feedback (vector hearts/stars/sparkles with
/// gradient shaders, motion-blur trails, radial shockwaves and floating
/// score texts). Port of the WPF GameAnimationService.
/// </summary>
public sealed class ParticleCanvas : Control, IDisposable
{
    private enum ShapeKind { Heart, Star, Sparkle }

    private const int MaxBurstParticles = 50;

    private readonly DispatcherTimer _timer;
    private readonly Random _rng = new();

    private readonly List<PetalParticle> _petals = [];
    private readonly List<BurstParticle> _bursts = [];
    private readonly List<Shockwave> _shockwaves = [];
    private readonly List<FloatingText> _floatingTexts = [];

    // Cached native graphics resources (zero allocations per frame)
    private readonly SKPath _heartPath;
    private readonly SKPath _starPath;
    private readonly SKPath _sparklePath;
    private readonly SKShader _heartShader;
    private readonly SKShader _starShader;
    private readonly SKShader _sparkleShader;
    private readonly SKTypeface _fontTypeface;
    private readonly SKFont _textFont;
    private readonly SKPaint _paint;

    private WriteableBitmap? _bitmap;
    private DateTime _lastFrame = DateTime.UtcNow;
    private bool _runningBackground;

    // Automatic combo tracking (matches chained within the window escalate the effect)
    private DateTime _lastSpawnTime = DateTime.MinValue;
    private int _currentCombo = 1;

    public ParticleCanvas()
    {
        IsHitTestVisible = false;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnTick;

        _heartPath = CreateHeartPath(16f);
        _starPath = CreateStarPath(16f, 6.5f);
        _sparklePath = CreateSparklePath(16f);

        _heartShader = CreateGradient(new SKColor(255, 105, 180), new SKColor(255, 182, 193)); // Rose gold
        _starShader = CreateGradient(new SKColor(255, 215, 0), new SKColor(255, 140, 0));      // Magical gold
        _sparkleShader = CreateGradient(new SKColor(0, 255, 255), new SKColor(0, 128, 255));   // Electric blue

        _fontTypeface = SKTypeface.FromFamilyName("Nunito")
                        ?? SKTypeface.FromFamilyName("Segoe UI")
                        ?? SKTypeface.Default;
        _textFont = new SKFont(_fontTypeface, 22f);
        _paint = new SKPaint { IsAntialias = true };
    }

    // ── Public API ────────────────────────────────────────────────────────

    public void Start()
    {
        _runningBackground = true;
        EnsureTimer();
    }

    public void Stop()
    {
        _runningBackground = false;
        if (!HasActiveEffects())
            _timer.Stop();
    }

    /// <summary>
    /// Spawns the match celebration at <paramref name="anchor"/>. When called with the
    /// default combo, consecutive matches within 3.5 s escalate the effect automatically.
    /// </summary>
    public void SpawnMatchBurst(Point anchor, int combo = 1)
    {
        var now = DateTime.UtcNow;
        if (combo == 1)
        {
            _currentCombo = (now - _lastSpawnTime).TotalSeconds <= 3.5
                ? Math.Min(4, _currentCombo + 1)
                : 1;
        }
        else
        {
            _currentCombo = combo;
        }
        _lastSpawnTime = now;

        var effectiveCombo = _currentCombo;
        var (kind, color) = effectiveCombo switch
        {
            1 => (ShapeKind.Heart, new SKColor(255, 105, 180)),
            2 => (ShapeKind.Star, new SKColor(255, 215, 0)),
            _ => (ShapeKind.Sparkle, new SKColor(0, 255, 255)),
        };

        var count = Math.Min(12 * effectiveCombo, MaxBurstParticles - _bursts.Count);
        for (var i = 0; i < count; i++)
        {
            var speed = _rng.Next(100, 250) + effectiveCombo * 35f;
            var angle = _rng.NextDouble() * Math.PI * 2;
            _bursts.Add(new BurstParticle
            {
                X = (float)anchor.X,
                Y = (float)anchor.Y,
                VelocityX = (float)(Math.Cos(angle) * speed),
                VelocityY = (float)(Math.Sin(angle) * speed) - 80f, // Upward bias
                Gravity = 380f,
                Drag = 0.95f,
                Size = _rng.Next(18, 32),
                StartAngle = _rng.Next(-180, 180),
                TargetAngle = _rng.Next(-360, 360),
                Age = 0f,
                Lifetime = 0.6f + _rng.Next(0, 40) / 100f,
                Delay = _rng.Next(0, 150) / 1000f,
                Kind = kind,
                Color = color,
                HistoryX0 = (float)anchor.X, HistoryY0 = (float)anchor.Y,
                HistoryX1 = (float)anchor.X, HistoryY1 = (float)anchor.Y,
            });
        }

        _shockwaves.Add(new Shockwave
        {
            CenterX = (float)anchor.X,
            CenterY = (float)anchor.Y,
            MaxRadius = 60f + 25f * effectiveCombo,
            Lifetime = 0.45f + 0.10f * effectiveCombo,
            Color = color,
        });

        var (label, fontScale) = effectiveCombo switch
        {
            1 => ("+100", 1.0f),
            2 => ("COMBO X2! +200", 1.25f),
            _ => ($"MEGA COMBO X{effectiveCombo}! +{effectiveCombo * 100}", 1.45f),
        };
        _floatingTexts.Add(new FloatingText
        {
            Text = label,
            X = (float)anchor.X,
            Y = (float)anchor.Y - 20f,
            VelocityY = -150f - 25f * effectiveCombo,
            Lifetime = 1.15f,
            Scale = fontScale,
            Color = color,
        });

        EnsureTimer();
    }

    public void PlayGameOver()
    {
        var cx = (float)(Bounds.Width / 2);
        var cy = (float)(Bounds.Height / 2);

        _shockwaves.Add(new Shockwave
        {
            CenterX = cx, CenterY = cy,
            MaxRadius = 180f, Lifetime = 0.7f,
            Color = new SKColor(255, 215, 0),
        });

        for (var ring = 0; ring < 3; ring++)
        {
            var count = 18 + ring * 6;
            var kind = ring switch { 0 => ShapeKind.Heart, 1 => ShapeKind.Star, _ => ShapeKind.Sparkle };
            var color = ring switch
            {
                0 => new SKColor(255, 105, 180),
                1 => new SKColor(255, 215, 0),
                _ => new SKColor(0, 255, 255),
            };

            for (var i = 0; i < count; i++)
            {
                var angle = Math.PI * 2 * i / count;
                var speed = 140 + ring * 70 + _rng.NextDouble() * 80;
                _bursts.Add(new BurstParticle
                {
                    X = cx, Y = cy,
                    VelocityX = (float)(Math.Cos(angle) * speed),
                    VelocityY = (float)(Math.Sin(angle) * speed),
                    Gravity = 240f,
                    Drag = 0.9f,
                    Size = _rng.Next(16, 28),
                    StartAngle = _rng.Next(-180, 180),
                    TargetAngle = _rng.Next(-360, 360),
                    Lifetime = 0.8f + _rng.Next(0, 50) / 100f,
                    Delay = ring * 0.12f,
                    Kind = kind,
                    Color = color,
                    HistoryX0 = cx, HistoryY0 = cy,
                    HistoryX1 = cx, HistoryY1 = cy,
                });
            }
        }

        EnsureTimer();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Bounds.Width <= 1 || Bounds.Height <= 1)
            return;

        EnsureBitmap();
        if (_bitmap is null)
            return;

        RenderSkiaFrame();
        var source = new Rect(0, 0, _bitmap.PixelSize.Width, _bitmap.PixelSize.Height);
        var destination = new Rect(Bounds.Size);
        context.DrawImage(_bitmap, source, destination);
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
        _bitmap?.Dispose();
        _heartPath.Dispose();
        _starPath.Dispose();
        _sparklePath.Dispose();
        _heartShader.Dispose();
        _starShader.Dispose();
        _sparkleShader.Dispose();
        _textFont.Dispose();
        _fontTypeface.Dispose();
        _paint.Dispose();
    }

    // ── Simulation ────────────────────────────────────────────────────────

    private void OnTick(object? sender, EventArgs e)
    {
        var now = DateTime.UtcNow;
        var dt = (float)Math.Clamp((now - _lastFrame).TotalSeconds, 0.001, 0.05);
        _lastFrame = now;

        if (_runningBackground && _petals.Count < 18 && _rng.NextDouble() < 0.025 && Bounds.Width > 0)
        {
            _petals.Add(PetalParticle.Spawn(_rng, Bounds.Width));
        }

        for (var i = _petals.Count - 1; i >= 0; i--)
        {
            var petal = _petals[i];
            petal.Update(dt);
            if (!petal.IsAlive || petal.Y > Bounds.Height + 80)
                _petals.RemoveAt(i);
        }

        for (var i = _bursts.Count - 1; i >= 0; i--)
        {
            var p = _bursts[i];
            p.Age += dt;
            if (p.Age >= p.Delay + p.Lifetime)
            {
                _bursts.RemoveAt(i);
                continue;
            }

            if (p.Age >= p.Delay)
            {
                // 2-frame motion-blur history
                p.HistoryX1 = p.HistoryX0; p.HistoryY1 = p.HistoryY0;
                p.HistoryX0 = p.X; p.HistoryY0 = p.Y;
                p.HistoryCount = Math.Min(2, p.HistoryCount + 1);

                p.VelocityY += p.Gravity * dt;
                p.VelocityX *= 1f - p.Drag * dt;
                p.VelocityY *= 1f - p.Drag * dt;
                p.X += p.VelocityX * dt;
                p.Y += p.VelocityY * dt;
            }

            _bursts[i] = p;
        }

        for (var i = _shockwaves.Count - 1; i >= 0; i--)
        {
            var s = _shockwaves[i];
            s.Age += dt;
            if (s.Age >= s.Lifetime)
            {
                _shockwaves.RemoveAt(i);
                continue;
            }

            var t = s.Age / s.Lifetime;
            s.CurrentRadius = t * (2f - t) * s.MaxRadius; // Quadratic ease-out
            _shockwaves[i] = s;
        }

        for (var i = _floatingTexts.Count - 1; i >= 0; i--)
        {
            var ft = _floatingTexts[i];
            ft.Age += dt;
            if (ft.Age >= ft.Lifetime)
            {
                _floatingTexts.RemoveAt(i);
                continue;
            }

            ft.VelocityY += 85f * dt; // Natural deceleration while rising
            ft.Y += ft.VelocityY * dt;
            _floatingTexts[i] = ft;
        }

        InvalidateVisual();

        if (!_runningBackground && !HasActiveEffects())
            _timer.Stop();
    }

    private bool HasActiveEffects() =>
        _petals.Count > 0 || _bursts.Count > 0 || _shockwaves.Count > 0 || _floatingTexts.Count > 0;

    private void EnsureTimer()
    {
        _lastFrame = DateTime.UtcNow;
        if (!_timer.IsEnabled)
            _timer.Start();
    }

    // ── Rendering ─────────────────────────────────────────────────────────

    private void EnsureBitmap()
    {
        var width = Math.Max(1, (int)Math.Ceiling(Bounds.Width));
        var height = Math.Max(1, (int)Math.Ceiling(Bounds.Height));
        if (_bitmap is not null && _bitmap.PixelSize.Width == width && _bitmap.PixelSize.Height == height)
            return;

        _bitmap?.Dispose();
        _bitmap = new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);
    }

    private void RenderSkiaFrame()
    {
        if (_bitmap is null)
            return;

        using var framebuffer = _bitmap.Lock();
        var info = new SKImageInfo(
            framebuffer.Size.Width,
            framebuffer.Size.Height,
            SKColorType.Bgra8888,
            SKAlphaType.Premul);
        using var surface = SKSurface.Create(info, framebuffer.Address, framebuffer.RowBytes);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        // 1. Shockwaves
        foreach (var s in _shockwaves)
        {
            var t = s.Age / s.Lifetime;
            _paint.Shader = null;
            _paint.Style = SKPaintStyle.Stroke;
            _paint.StrokeWidth = 8f * (1f - t);
            _paint.Color = s.Color.WithAlpha((byte)((1f - t) * 255));
            canvas.DrawCircle(s.CenterX, s.CenterY, s.CurrentRadius, _paint);
        }

        // 2. Ambient petals
        _paint.Style = SKPaintStyle.Fill;
        _paint.Shader = null;
        foreach (var petal in _petals)
        {
            _paint.Color = petal.SkiaColor;
            DrawHeart(canvas, _paint, petal.X, petal.Y, petal.Size);
        }

        // 3. Burst particles with motion-blur trail
        foreach (var p in _bursts)
        {
            if (p.Age < p.Delay)
                continue;

            var t = Math.Clamp((p.Age - p.Delay) / p.Lifetime, 0f, 1f);
            var angle = p.StartAngle + (p.TargetAngle - p.StartAngle) * t;
            var scale = CalculateScale(t) * (p.Size / 24f);
            var opacity = t > 0.55f ? 1f - (t - 0.55f) / 0.45f : 1f;
            var (path, shader) = GetShape(p.Kind);

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
                _paint.Shader = shader;
                _paint.Color = p.Color.WithAlpha((byte)(trailOpacity * 255));
                canvas.DrawPath(path, _paint);
                canvas.Restore();
            }

            // 3B. Main particle
            canvas.Save();
            canvas.Translate(p.X, p.Y);
            canvas.RotateDegrees(angle);
            canvas.Scale(scale, scale);
            _paint.Shader = shader;
            _paint.Color = p.Color.WithAlpha((byte)(opacity * 255));
            canvas.DrawPath(path, _paint);
            canvas.Restore();
        }

        _paint.Shader = null;

        // 4. Floating score texts — dark outline + bright fill
        foreach (var ft in _floatingTexts)
        {
            var t = ft.Age / ft.Lifetime;
            var opacity = 1f - t * t;
            var alpha = (byte)(Math.Clamp(opacity, 0f, 1f) * 255);
            if (alpha == 0)
                continue;

            var scale = GetBounceScale(t) * ft.Scale;

            canvas.Save();
            canvas.Translate(ft.X, ft.Y);
            canvas.Scale(scale, scale);

            var textX = -_textFont.MeasureText(ft.Text) / 2f;

            _paint.Style = SKPaintStyle.Stroke;
            _paint.StrokeWidth = 4.5f;
            _paint.Color = SKColors.Black.WithAlpha(alpha);
            canvas.DrawText(ft.Text, textX, 0f, _textFont, _paint);

            _paint.Style = SKPaintStyle.Fill;
            _paint.Color = ft.Color.WithAlpha(alpha);
            canvas.DrawText(ft.Text, textX, 0f, _textFont, _paint);

            canvas.Restore();
        }

        _paint.Style = SKPaintStyle.Fill;
        surface.Canvas.Flush();
    }

    private (SKPath Path, SKShader Shader) GetShape(ShapeKind kind) => kind switch
    {
        ShapeKind.Star => (_starPath, _starShader),
        ShapeKind.Sparkle => (_sparklePath, _sparkleShader),
        _ => (_heartPath, _heartShader),
    };

    // ── Easing curves (ported from the WPF engine) ────────────────────────

    private static float CalculateScale(float t)
    {
        // Elastic pop-in → settle → shrink out
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
        // "Damage number" pop curve
        if (t < 0.3f) return t / 0.3f * 1.3f;
        if (t < 0.6f) return 1.3f - 0.35f * ((t - 0.3f) / 0.3f);
        if (t < 0.8f) return 0.95f + 0.1f * ((t - 0.6f) / 0.2f);
        return 1.05f - 0.05f * ((t - 0.8f) / 0.2f);
    }

    // ── Geometry / shader factories ───────────────────────────────────────

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
        // 4-point sparkle with soft curves toward the centre
        var path = new SKPath();
        path.MoveTo(0, -radius);
        path.QuadTo(0, 0, radius, 0);
        path.QuadTo(0, 0, 0, radius);
        path.QuadTo(0, 0, -radius, 0);
        path.QuadTo(0, 0, 0, -radius);
        path.Close();
        return path;
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

    // ── Particle data ─────────────────────────────────────────────────────

    private struct BurstParticle
    {
        public float X, Y;
        public float VelocityX, VelocityY;
        public float Gravity, Drag;
        public float Size;
        public float StartAngle, TargetAngle;
        public float Age, Lifetime, Delay;
        public ShapeKind Kind;
        public SKColor Color;
        public float HistoryX0, HistoryY0, HistoryX1, HistoryY1;
        public int HistoryCount;
    }

    private struct Shockwave
    {
        public float CenterX, CenterY;
        public float CurrentRadius, MaxRadius;
        public float Age, Lifetime;
        public SKColor Color;
    }

    private struct FloatingText
    {
        public string Text;
        public float X, Y;
        public float VelocityY;
        public float Age, Lifetime;
        public float Scale;
        public SKColor Color;
    }

    private sealed class PetalParticle
    {
        private readonly double _swayAmplitude;  // px — matches WPF's 30–110 px range
        private readonly double _swayFrequency;  // rad/s — slow oscillation for lazy drift
        private readonly double _baseOpacity;
        private readonly double _initialLife;
        private double _life;
        private double _age;

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Size { get; }

        public SKColor SkiaColor
        {
            get
            {
                var ratio = _life / _initialLife;
                // Hold full opacity until the last 30 % of lifetime, then fade out
                var opacity = _baseOpacity * (ratio < 0.3 ? ratio / 0.3 : 1.0);
                return _baseColor.WithAlpha((byte)Math.Clamp(255 * opacity, 0, 255));
            }
        }

        public bool IsAlive => _life > 0;

        private readonly SKColor _baseColor;
        private readonly float _velocityX;
        private float _velocityY;

        private PetalParticle(float x, float y, float vx, float vy, SKColor color, float size,
            double life, double swayAmplitude, double swayFrequency, double baseOpacity)
        {
            X = x; Y = y;
            _velocityX = vx; _velocityY = vy;
            _baseColor = color;
            Size = size;
            _life = life;
            _initialLife = life;
            _swayAmplitude = swayAmplitude;
            _swayFrequency = swayFrequency;
            _baseOpacity = baseOpacity;
        }

        public static PetalParticle Spawn(Random rng, double canvasWidth)
        {
            var colors = Helpers.ThemeAssets.GetParticleColors(Helpers.ThemeAssets.CurrentThemeName);
            var c = colors[rng.Next(colors.Count)];
            return new PetalParticle(
                x: (float)(rng.NextDouble() * canvasWidth),
                y: -18f,
                vx: (float)((rng.NextDouble() - 0.5) * 30),
                vy: (float)(10 + rng.NextDouble() * 20),
                color: new SKColor(c.R, c.G, c.B, c.A),
                size: (float)(5 + rng.NextDouble() * 9),
                life: 6.0 + rng.NextDouble() * 3.0,
                swayAmplitude: 30 + rng.NextDouble() * 80,
                swayFrequency: 0.35 + rng.NextDouble() * 0.4,
                baseOpacity: 0.55 + rng.NextDouble() * 0.3);
        }

        public void Update(double dt)
        {
            _life -= dt;
            _age  += dt;
            // Gentle gravity — petals accelerate slowly like WPF's SineEase.EaseIn
            _velocityY += (float)(15 * dt);
            // Sinusoidal sway: derivative of A*sin(ω*age) = A*ω*cos(ω*age)
            var swayDelta = _swayAmplitude * _swayFrequency * Math.Cos(_swayFrequency * _age) * dt;
            X += (float)(_velocityX * dt + swayDelta);
            Y += (float)(_velocityY * dt);
        }
    }
}
