using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace KatyaKatya.Controls;

/// <summary>
/// GPU-composited dynamic background: ambient mist layers, volumetric puffy
/// clouds drifting in opposite directions, floating bokeh bubbles, a soft
/// spotlight and a vignette — all with mouse parallax. Port of the WPF
/// GameBackgroundService.
/// </summary>
public sealed class AnimatedBackground : Canvas
{
    private static readonly Random Rng = new();

    private struct MistState
    {
        public float X, Y, BaseY;
        public float VelocityX;
        public float Width, Height;
        public float PulseSpeed, PulsePhase;
        public float SpeedFactor;
        public int LayerType;
        public float ErraticPhaseX, ErraticPhaseY;
        public float BaseOpacity;
    }

    private struct CloudState
    {
        public float X, Y, BaseY;
        public float VelocityX;
        public float Width, Height;
        public float PulseSpeed, PulsePhase;
        public float SpeedFactor;
        public float BaseOpacity;
        public float ErraticPhase;
    }

    private struct BubbleState
    {
        public float X, Y;
        public float VelocityX, VelocityY;
        public float BaseRadius;
        public float PulseSpeed, PulsePhase;
        public float SpeedFactor;
    }

    private readonly DispatcherTimer _timer;
    private readonly List<MistState> _mist = [];
    private readonly List<Control> _mistElements = [];
    private readonly List<CloudState> _clouds = [];
    private readonly List<Control> _cloudElements = [];
    private readonly List<BubbleState> _bubbles = [];
    private readonly List<Control> _bubbleElements = [];

    private Border? _spotlight;
    private Border? _vignette;
    private TopLevel? _topLevel;
    private DateTime _lastFrame = DateTime.UtcNow;

    // Smoothed mouse parallax
    private float _targetMouseX, _targetMouseY;
    private float _mouseX, _mouseY;

    public AnimatedBackground()
    {
        IsHitTestVisible = false;
        ClipToBounds = true;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnTick;

        // Back-to-front layers
        CreateSpotlight();
        InitializeMist();
        InitializeClouds();
        InitializeBubbles();
        CreateVignette();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _topLevel = TopLevel.GetTopLevel(this);
        if (_topLevel is not null)
            _topLevel.PointerMoved += OnPointerMoved;

        _lastFrame = DateTime.UtcNow;
        _timer.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_topLevel is not null)
        {
            _topLevel.PointerMoved -= OnPointerMoved;
            _topLevel = null;
        }

        _timer.Stop();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (e.NewSize.Width <= 0 || e.NewSize.Height <= 0)
            return;

        if (_spotlight is not null)
        {
            _spotlight.Width = e.NewSize.Width;
            _spotlight.Height = e.NewSize.Height;
        }

        if (_vignette is not null)
        {
            _vignette.Width = e.NewSize.Width;
            _vignette.Height = e.NewSize.Height;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var pos = e.GetPosition(this);
        _targetMouseX = (float)pos.X;
        _targetMouseY = (float)pos.Y;
    }

    // ── Layer construction ────────────────────────────────────────────────

    private void CreateSpotlight()
    {
        var brush = new RadialGradientBrush
        {
            Center = new RelativePoint(0.5, 0.35, RelativeUnit.Relative),
            GradientOrigin = new RelativePoint(0.5, 0.35, RelativeUnit.Relative),
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(130, 255, 218, 224), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(60, 240, 220, 240), 0.5));
        brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));

        _spotlight = new Border
        {
            Width = 1920,
            Height = 1080,
            IsHitTestVisible = false,
            Background = brush,
        };
        Children.Add(_spotlight);
    }

    private void CreateVignette()
    {
        var brush = new RadialGradientBrush
        {
            Center = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
            GradientOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
        };
        brush.GradientStops.Add(new GradientStop(Colors.Transparent, 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(95, 20, 10, 20), 1.0));

        _vignette = new Border
        {
            Width = 1920,
            Height = 1080,
            IsHitTestVisible = false,
            Background = brush,
        };
        Children.Add(_vignette);
    }

    /// <summary>Soft, low-opacity wide horizontal fog layers at three depths.</summary>
    private void InitializeMist()
    {
        Color[] colors =
        [
            Color.FromRgb(255, 218, 224), // Pastel rose
            Color.FromRgb(240, 230, 240), // Pale lavender
            Color.FromRgb(255, 228, 225), // Misty rose
            Color.FromRgb(255, 240, 245), // Lavender blush
        ];

        for (var i = 0; i < 6; i++)
        {
            var tint = colors[Rng.Next(colors.Length)];
            var layerType = i / 2; // 0 far · 1 mid · 2 near
            float velX, baseY, speedFactor, width, height, opacityMin, opacityMax;

            if (layerType == 0)
            {
                velX = 25f + Rng.Next(0, 15);
                baseY = Rng.Next(80, 200);
                speedFactor = 0.08f + Rng.Next(0, 5) / 100f;
                width = Rng.Next(800, 1100);
                height = Rng.Next(300, 400);
                opacityMin = 0.15f; opacityMax = 0.28f;
            }
            else if (layerType == 1)
            {
                velX = -(40f + Rng.Next(0, 20));
                baseY = Rng.Next(300, 480);
                speedFactor = 0.35f + Rng.Next(0, 10) / 100f;
                width = Rng.Next(1100, 1400);
                height = Rng.Next(400, 500);
                opacityMin = 0.20f; opacityMax = 0.32f;
            }
            else
            {
                velX = Rng.Next(-15, 15);
                baseY = Rng.Next(600, 720);
                speedFactor = 0.70f + Rng.Next(0, 15) / 100f;
                width = Rng.Next(1400, 1800);
                height = Rng.Next(500, 650);
                opacityMin = 0.22f; opacityMax = 0.35f;
            }

            var baseOpacity = (float)(opacityMin + Rng.NextDouble() * (opacityMax - opacityMin));

            var element = new Ellipse
            {
                Width = width,
                Height = height,
                IsHitTestVisible = false,
                Opacity = baseOpacity,
                Fill = CreateSoftRadial(tint),
                RenderTransform = NewTransformGroup(Rng.Next(-400, 1800), baseY),
            };
            Children.Add(element);
            _mistElements.Add(element);

            _mist.Add(new MistState
            {
                X = Rng.Next(-400, 1800),
                Y = baseY,
                BaseY = baseY,
                VelocityX = velX,
                Width = width,
                Height = height,
                PulseSpeed = 0.15f + Rng.Next(0, 15) / 100f,
                PulsePhase = (float)(Rng.NextDouble() * Math.PI * 2),
                SpeedFactor = speedFactor,
                LayerType = layerType,
                ErraticPhaseX = (float)(Rng.NextDouble() * Math.PI * 2),
                ErraticPhaseY = (float)(Rng.NextDouble() * Math.PI * 2),
                BaseOpacity = baseOpacity,
            });
        }
    }

    /// <summary>Three volumetric multi-puff clouds at distinct parallax depths.</summary>
    private void InitializeClouds()
    {
        AddCloud(width: 1500f, height: 650f, baseY: Rng.Next(30, 120),
                 velX: 15f + Rng.Next(0, 10), speedFactor: 0.20f, opacity: 0.40f,
                 startX: Rng.Next(-500, 400), pulseSpeed: 0.18f);

        AddCloud(width: 1100f, height: 480f, baseY: Rng.Next(80, 220),
                 velX: 35f + Rng.Next(0, 15), speedFactor: 0.50f, opacity: 0.72f,
                 startX: Rng.Next(-300, 600), pulseSpeed: 0.24f);

        AddCloud(width: 1400f, height: 580f, baseY: Rng.Next(280, 420),
                 velX: -(45f + Rng.Next(0, 15)), speedFactor: 0.82f, opacity: 0.80f,
                 startX: Rng.Next(800, 1800), pulseSpeed: 0.20f);
    }

    private void AddCloud(float width, float height, float baseY, float velX,
        float speedFactor, float opacity, float startX, float pulseSpeed)
    {
        var element = CreatePuffyCloud(width, height);
        element.Opacity = opacity;
        Children.Add(element);
        _cloudElements.Add(element);

        _clouds.Add(new CloudState
        {
            X = startX,
            Y = baseY,
            BaseY = baseY,
            VelocityX = velX,
            Width = width,
            Height = height,
            PulseSpeed = pulseSpeed,
            PulsePhase = (float)(Rng.NextDouble() * Math.PI * 2),
            SpeedFactor = speedFactor,
            BaseOpacity = opacity,
            ErraticPhase = (float)(Rng.NextDouble() * Math.PI * 2),
        });
    }

    /// <summary>
    /// Volumetric puffy cloud: underbelly shadows, warm subsurface-scattering
    /// transitions, an asymmetric multi-puff body, and silver-lining rim lights.
    /// Light comes from the top-left.
    /// </summary>
    private static Canvas CreatePuffyCloud(float width, float height)
    {
        var cloud = new Canvas
        {
            Width = width,
            Height = height,
            IsHitTestVisible = false,
            RenderTransform = NewTransformGroup(0, 0),
        };

        var w = width / 400f;
        var h = height / 200f;

        var white = Colors.White;
        var cream = Color.FromRgb(255, 248, 250);
        var transition = Color.FromRgb(242, 215, 222); // Warm peach-rose glow
        var shadow = Color.FromRgb(205, 190, 202);     // Lavender-grey shadow

        // 1. Underbelly shadows (offset bottom-right)
        AddPuff(cloud, 60 * w, 115 * h, 290 * w, 65 * h, ShadowBrush(shadow));
        AddPuff(cloud, 25 * w, 125 * h, 160 * w, 55 * h, ShadowBrush(shadow));
        AddPuff(cloud, 225 * w, 125 * h, 160 * w, 55 * h, ShadowBrush(shadow));
        AddPuff(cloud, 140 * w, 130 * h, 180 * w, 60 * h, ShadowBrush(shadow));

        // 2. Subsurface-scattering transitions
        AddPuff(cloud, 85 * w, 65 * h, 230 * w, 110 * h, TransitionBrush(transition));
        AddPuff(cloud, 30 * w, 80 * h, 140 * w, 90 * h, TransitionBrush(transition));
        AddPuff(cloud, 230 * w, 80 * h, 140 * w, 90 * h, TransitionBrush(transition));

        // 3. Asymmetric white/cream body
        AddPuff(cloud, 100 * w, 40 * h, 180 * w, 105 * h, BodyBrush(white, cream));
        AddPuff(cloud, 140 * w, 45 * h, 170 * w, 105 * h, BodyBrush(white, cream));
        AddPuff(cloud, 30 * w, 65 * h, 110 * w, 85 * h, BodyBrush(white, cream));
        AddPuff(cloud, 65 * w, 55 * h, 95 * w, 75 * h, BodyBrush(white, cream));
        AddPuff(cloud, 250 * w, 65 * h, 110 * w, 85 * h, BodyBrush(white, cream));
        AddPuff(cloud, 220 * w, 55 * h, 95 * w, 75 * h, BodyBrush(white, cream));
        AddPuff(cloud, 95 * w, 22 * h, 110 * w, 90 * h, BodyBrush(white, cream));
        AddPuff(cloud, 175 * w, 18 * h, 125 * w, 95 * h, BodyBrush(white, cream));
        AddPuff(cloud, 135 * w, 28 * h, 115 * w, 85 * h, BodyBrush(white, cream));
        AddPuff(cloud, 115 * w, 60 * h, 80 * w, 60 * h, BodyBrush(white, cream));
        AddPuff(cloud, 195 * w, 58 * h, 90 * w, 65 * h, BodyBrush(white, cream));
        AddPuff(cloud, 155 * w, 75 * h, 100 * w, 70 * h, BodyBrush(white, cream));

        // 4. Silver lining on the sun-facing edges
        AddPuff(cloud, 85 * w, 15 * h, 75 * w, 55 * h, RimBrush());
        AddPuff(cloud, 155 * w, 10 * h, 85 * w, 60 * h, RimBrush());
        AddPuff(cloud, 215 * w, 20 * h, 75 * w, 55 * h, RimBrush());
        AddPuff(cloud, 25 * w, 50 * h, 65 * w, 50 * h, RimBrush());

        return cloud;
    }

    private static void AddPuff(Canvas canvas, double x, double y, double w, double h, IBrush brush)
    {
        var puff = new Ellipse
        {
            Width = w,
            Height = h,
            IsHitTestVisible = false,
            Fill = brush,
        };
        SetLeft(puff, x);
        SetTop(puff, y);
        canvas.Children.Add(puff);
    }

    private static RadialGradientBrush BodyBrush(Color c1, Color c2)
    {
        var brush = new RadialGradientBrush
        {
            Center = new RelativePoint(0.35, 0.22, RelativeUnit.Relative),
            GradientOrigin = new RelativePoint(0.35, 0.22, RelativeUnit.Relative),
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(170, c1.R, c1.G, c1.B), 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(110, c2.R, c2.G, c2.B), 0.55));
        brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
        return brush;
    }

    private static RadialGradientBrush TransitionBrush(Color c)
    {
        var brush = new RadialGradientBrush
        {
            Center = new RelativePoint(0.35, 0.22, RelativeUnit.Relative),
            GradientOrigin = new RelativePoint(0.35, 0.22, RelativeUnit.Relative),
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(110, c.R, c.G, c.B), 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(45, c.R, c.G, c.B), 0.55));
        brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
        return brush;
    }

    private static RadialGradientBrush ShadowBrush(Color c)
    {
        var brush = new RadialGradientBrush
        {
            Center = new RelativePoint(0.45, 0.45, RelativeUnit.Relative),
            GradientOrigin = new RelativePoint(0.45, 0.45, RelativeUnit.Relative),
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(120, c.R, c.G, c.B), 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(45, c.R, c.G, c.B), 0.60));
        brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
        return brush;
    }

    private static RadialGradientBrush RimBrush()
    {
        var brush = new RadialGradientBrush
        {
            Center = new RelativePoint(0.28, 0.18, RelativeUnit.Relative),
            GradientOrigin = new RelativePoint(0.28, 0.18, RelativeUnit.Relative),
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(140, 255, 255, 255), 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(60, 255, 255, 255), 0.60));
        brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
        return brush;
    }

    private static RadialGradientBrush CreateSoftRadial(Color tint)
    {
        var brush = new RadialGradientBrush();
        brush.GradientStops.Add(new GradientStop(tint, 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, tint.R, tint.G, tint.B), 1.0));
        return brush;
    }

    private void InitializeBubbles()
    {
        Color[] colors =
        [
            Color.FromArgb(40, 255, 182, 193),
            Color.FromArgb(35, 255, 105, 180),
            Color.FromArgb(45, 230, 230, 250),
            Color.FromArgb(50, 255, 240, 245),
            Color.FromArgb(45, 255, 228, 225),
        ];

        for (var i = 0; i < 15; i++)
        {
            var tint = colors[Rng.Next(colors.Length)];
            var tier = Rng.Next(0, 3);
            float speedFactor, baseRadius, alphaScale;

            if (tier == 0)
            {
                speedFactor = 0.15f + Rng.Next(0, 15) / 100f;
                baseRadius = Rng.Next(40, 75);
                alphaScale = 0.45f;
            }
            else if (tier == 1)
            {
                speedFactor = 0.5f + Rng.Next(0, 35) / 100f;
                baseRadius = Rng.Next(90, 140);
                alphaScale = 0.85f;
            }
            else
            {
                speedFactor = 1.2f + Rng.Next(0, 45) / 100f;
                baseRadius = Rng.Next(170, 240);
                alphaScale = 1.2f;
            }

            var opacity = Math.Clamp(tint.A / 255f * alphaScale, 0f, 1f);
            var pure = Color.FromRgb(tint.R, tint.G, tint.B);

            var element = new Ellipse
            {
                Width = baseRadius * 2,
                Height = baseRadius * 2,
                IsHitTestVisible = false,
                Opacity = opacity,
                Fill = CreateSoftRadial(pure),
                RenderTransform = NewTransformGroup(Rng.Next(0, 1920), Rng.Next(0, 1080)),
            };
            Children.Add(element);
            _bubbleElements.Add(element);

            _bubbles.Add(new BubbleState
            {
                X = Rng.Next(0, 1920),
                Y = Rng.Next(0, 1080),
                VelocityX = Rng.Next(-15, 15) / 10f,
                VelocityY = Rng.Next(-18, -5) / 10f,
                BaseRadius = baseRadius,
                PulseSpeed = 0.4f + Rng.Next(0, 8) / 10f,
                PulsePhase = (float)(Rng.NextDouble() * Math.PI * 2),
                SpeedFactor = speedFactor,
            });
        }
    }

    private static TransformGroup NewTransformGroup(double x, double y) => new()
    {
        Children =
        {
            new ScaleTransform(1, 1),
            new TranslateTransform(x, y),
        },
    };

    // ── Per-frame update ──────────────────────────────────────────────────

    private void OnTick(object? sender, EventArgs e)
    {
        var now = DateTime.UtcNow;
        var dt = (float)(now - _lastFrame).TotalSeconds;
        _lastFrame = now;
        if (dt <= 0 || dt > 0.1f)
            return;

        var width = (float)Bounds.Width;
        var height = (float)Bounds.Height;
        if (width <= 0 || height <= 0)
            return;

        // Smooth parallax interpolation toward the cursor
        _mouseX += (_targetMouseX - _mouseX) * 4f * dt;
        _mouseY += (_targetMouseY - _mouseY) * 4f * dt;
        var parallaxX = -(_mouseX - width / 2f) * 0.025f;
        var parallaxY = -(_mouseY - height / 2f) * 0.025f;

        // 1. Ambient mist
        for (var i = 0; i < _mist.Count; i++)
        {
            var m = _mist[i];
            m.PulsePhase += m.PulseSpeed * dt;

            if (m.LayerType == 0)
            {
                m.X += m.VelocityX * dt;
                if (m.X > width + m.Width) m.X = -m.Width;
            }
            else if (m.LayerType == 1)
            {
                m.X += m.VelocityX * dt;
                if (m.X < -m.Width) m.X = width + m.Width;
            }
            else
            {
                m.ErraticPhaseX += 1.6f * dt;
                m.ErraticPhaseY += 1.0f * dt;
                m.X += (m.VelocityX + (float)Math.Sin(m.ErraticPhaseX) * 45f) * dt;
                m.Y = m.BaseY + (float)Math.Sin(m.ErraticPhaseY) * 45f;
                if (m.X > width + m.Width) m.X = -m.Width;
                if (m.X < -m.Width) m.X = width + m.Width;
            }

            _mist[i] = m;

            var breath = 0.65f + 0.35f * (float)Math.Sin(m.PulsePhase);
            var scale = 1.0f + 0.04f * (float)Math.Sin(m.PulsePhase);
            ApplyTransform(_mistElements[i], scale,
                m.X + parallaxX * m.SpeedFactor - m.Width / 2f,
                m.Y + parallaxY * m.SpeedFactor - m.Height / 2f,
                m.BaseOpacity * breath);
        }

        // 2. Puffy clouds with erratic wind currents
        for (var i = 0; i < _clouds.Count; i++)
        {
            var c = _clouds[i];
            c.PulsePhase += c.PulseSpeed * dt;
            c.ErraticPhase += 0.8f * dt;

            var wind = (float)Math.Sin(c.ErraticPhase * 1.5f) * 15f;
            c.X += (c.VelocityX + wind) * dt;
            c.Y = c.BaseY + (float)Math.Sin(c.ErraticPhase) * 20f;

            if (c.VelocityX > 0)
            {
                if (c.X > width + c.Width) c.X = -c.Width;
            }
            else
            {
                if (c.X < -c.Width) c.X = width + c.Width;
            }

            _clouds[i] = c;

            var scale = 1.0f + 0.03f * (float)Math.Sin(c.PulsePhase);
            var breath = 0.90f + 0.10f * (float)Math.Sin(c.PulsePhase);
            ApplyTransform(_cloudElements[i], scale,
                c.X + parallaxX * c.SpeedFactor,
                c.Y + parallaxY * c.SpeedFactor,
                c.BaseOpacity * breath);
        }

        // 3. Bokeh bubbles drifting upward
        for (var i = 0; i < _bubbles.Count; i++)
        {
            var b = _bubbles[i];
            b.PulsePhase += b.PulseSpeed * dt;
            b.Y += b.VelocityY * b.SpeedFactor * 40f * dt;
            b.X += b.VelocityX * b.SpeedFactor * 15f * dt;

            if (b.Y < -b.BaseRadius * 2)
            {
                b.Y = height + b.BaseRadius;
                b.X = Rng.Next(0, Math.Max(1, (int)width));
            }
            if (b.X < -b.BaseRadius * 2) b.X = width + b.BaseRadius;
            if (b.X > width + b.BaseRadius * 2) b.X = -b.BaseRadius;

            _bubbles[i] = b;

            var pulse = 1.0f + 0.12f * (float)Math.Sin(b.PulsePhase);
            ApplyTransform(_bubbleElements[i], pulse,
                b.X + parallaxX * b.SpeedFactor - b.BaseRadius,
                b.Y + parallaxY * b.SpeedFactor - b.BaseRadius,
                opacity: null);
        }
    }

    private static void ApplyTransform(Control element, float scale, float x, float y, float? opacity)
    {
        var group = (TransformGroup)element.RenderTransform!;
        var scaleTransform = (ScaleTransform)group.Children[0];
        var translateTransform = (TranslateTransform)group.Children[1];

        scaleTransform.ScaleX = scale;
        scaleTransform.ScaleY = scale;
        translateTransform.X = x;
        translateTransform.Y = y;

        if (opacity.HasValue)
            element.Opacity = opacity.Value;
    }
}
