using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using KatyaKatya.Engine.Core;
using KatyaKatya.Engine.Settings;
using KatyaKatya.Engine.Skia;
using SkiaSharp;

namespace KatyaKatya.Controls;

/// <summary>
/// Skia-backed particle overlay: ambient falling petals plus a combo-aware
/// "juice" engine for match feedback (vector hearts/stars/sparkles with
/// gradient shaders, motion-blur trails, radial shockwaves and floating
/// score texts). Port of the WPF GameAnimationService.
/// Driven by the shared <see cref="IGameLoop"/>; painted by a Skia
/// <see cref="ParticleDrawOperation"/> straight onto the render surface
/// (no per-frame WriteableBitmap).
/// </summary>
public sealed class ParticleCanvas : Control, IFrameUpdatable, IFrameDebugMetrics, IDisposable
{
    internal enum ShapeKind { Heart, Star, Sparkle }

    private const int MaxBurstParticles = 50;

    private IGameLoop? _loop;
    private IGraphicsSettingsService? _graphicsSettings;
    private readonly Random _rng = new();

    private readonly List<PetalParticle> _petals = [];
    private readonly List<BurstParticle> _bursts = [];
    private readonly List<Shockwave> _shockwaves = [];
    private readonly List<FloatingText> _floatingTexts = [];

    // Long-lived Skia resources, plus a double-buffered scene snapshot. The render thread
    // reads one buffer while the UI thread fills the other next frame.
    private readonly ParticleResources _res = new();
    private readonly ParticleScene[] _scenes = [new ParticleScene(), new ParticleScene()];
    private int _sceneIndex;

    private bool _runningBackground;

    // Automatic combo tracking (matches chained within the window escalate the effect)
    private DateTime _lastSpawnTime = DateTime.MinValue;
    private int _currentCombo = 1;

    public ParticleCanvas()
    {
        IsHitTestVisible = false;
    }

    // ── Loop integration ──────────────────────────────────────────────────

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _loop ??= App.Services?.GetService<IGameLoop>();
        _graphicsSettings ??= App.Services?.GetService<IGraphicsSettingsService>();
        _loop?.Register(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _loop?.Unregister(this);
    }

    /// <summary>
    /// Debug kill-switch: when true, every particle canvas stops ticking and rendering.
    /// Used by the perf toggles (F10) to isolate the canvas's per-frame cost.
    /// </summary>
    public static bool DiagnosticsDisabled;

    /// <summary>True while ambient petals run or any burst effect is still alive.</summary>
    public bool IsActive =>
        !DiagnosticsDisabled
        && (_graphicsSettings?.EnableParticles ?? true)
        && (_runningBackground || HasActiveEffects());

    string? IFrameDebugMetrics.DebugMetrics =>
        $"particles petals:{_petals.Count} bursts:{_bursts.Count} sw:{_shockwaves.Count}";

    // ── Public API ────────────────────────────────────────────────────────

    public void Start()
    {
        _runningBackground = true;
        _loop?.Wake();
    }

    public void Stop()
    {
        _runningBackground = false;
        // The loop parks itself once no system is animating.
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

        _loop?.Wake();
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

        _loop?.Wake();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (DiagnosticsDisabled || _graphicsSettings?.EnableParticles == false)
            return;

        if (Bounds.Width <= 1 || Bounds.Height <= 1 || !HasActiveEffects())
            return;

        // Snapshot the live simulation (UI thread) into the back buffer, then hand it to a
        // Skia draw operation that runs on the render thread.
        var scene = _scenes[_sceneIndex];
        _sceneIndex ^= 1;
        scene.Clear();

        foreach (var s in _shockwaves)
            scene.AddShockwave(s);
        foreach (var petal in _petals)
            scene.AddPetal(petal.X, petal.Y, petal.Size, petal.SkiaColor);
        foreach (var b in _bursts)
            scene.AddBurst(b);
        foreach (var ft in _floatingTexts)
            scene.AddText(ft);

        context.Custom(new ParticleDrawOperation(new Rect(Bounds.Size), scene, _res));
    }

    public void Dispose()
    {
        _loop?.Unregister(this);
        _res.Dispose();
    }

    // ── Simulation ────────────────────────────────────────────────────────

    public void Tick(in FrameTime time)
    {
        var dt = (float)Math.Clamp(time.DeltaSeconds, 0.001, 0.05);

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
    }

    private bool HasActiveEffects() =>
        _petals.Count > 0 || _bursts.Count > 0 || _shockwaves.Count > 0 || _floatingTexts.Count > 0;

    // ── Particle data ─────────────────────────────────────────────────────

    internal struct BurstParticle
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

    internal struct Shockwave
    {
        public float CenterX, CenterY;
        public float CurrentRadius, MaxRadius;
        public float Age, Lifetime;
        public SKColor Color;
    }

    internal struct FloatingText
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
                size: (float)(14 + rng.NextDouble() * 16),
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
