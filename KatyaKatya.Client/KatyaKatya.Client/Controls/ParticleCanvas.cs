using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SkiaSharp;

namespace KatyaKatya.Controls;

public sealed class ParticleCanvas : Control, IDisposable
{
    private readonly DispatcherTimer _timer;
    private readonly Random _rng = new();
    private readonly List<Particle> _particles = [];
    private WriteableBitmap? _bitmap;
    private DateTime _lastFrame = DateTime.UtcNow;
    private bool _runningBackground;

    public ParticleCanvas()
    {
        IsHitTestVisible = false;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnTick;
    }

    public void Start()
    {
        _runningBackground = true;
        EnsureTimer();
    }

    public void Stop()
    {
        _runningBackground = false;
        if (_particles.Count == 0)
            _timer.Stop();
    }

    public void SpawnMatchBurst(Point anchor, int combo = 1)
    {
        var count = Math.Clamp(22 + combo * 6, 22, 46);
        for (var i = 0; i < count; i++)
        {
            var angle = _rng.NextDouble() * Math.PI * 2;
            var speed = 90 + _rng.NextDouble() * 210;
            var velocity = new Vector(Math.Cos(angle) * speed, Math.Sin(angle) * speed - 40);
            _particles.Add(Particle.Burst(anchor, velocity, PickColor(), _rng.NextDouble() < 0.45));
        }

        EnsureTimer();
    }

    public void PlayGameOver()
    {
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        for (var ring = 0; ring < 3; ring++)
        {
            var count = 34 + ring * 8;
            for (var i = 0; i < count; i++)
            {
                var angle = Math.PI * 2 * i / count;
                var speed = 140 + ring * 70 + _rng.NextDouble() * 80;
                var velocity = new Vector(Math.Cos(angle) * speed, Math.Sin(angle) * speed);
                _particles.Add(Particle.Burst(center, velocity, PickColor(), ring % 2 == 0));
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
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var now = DateTime.UtcNow;
        var deltaSeconds = Math.Clamp((now - _lastFrame).TotalSeconds, 0.001, 0.05);
        _lastFrame = now;

        if (_runningBackground && _rng.NextDouble() < 0.18 && Bounds.Width > 0)
        {
            var x = _rng.NextDouble() * Bounds.Width;
            var velocity = new Vector((_rng.NextDouble() - 0.5) * 20, 22 + _rng.NextDouble() * 35);
            _particles.Add(Particle.Petal(new Point(x, -18), velocity, PickColor()));
        }

        for (var i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            particle.Update(deltaSeconds);

            if (!particle.IsAlive || particle.Position.Y > Bounds.Height + 80)
                _particles.RemoveAt(i);
        }

        InvalidateVisual();

        if (!_runningBackground && _particles.Count == 0)
            _timer.Stop();
    }

    private void EnsureTimer()
    {
        _lastFrame = DateTime.UtcNow;
        if (!_timer.IsEnabled)
            _timer.Start();
    }

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

        using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        foreach (var particle in _particles)
        {
            paint.Color = particle.SkiaColor;
            if (particle.IsHeart)
                DrawHeart(canvas, paint, particle);
            else
                canvas.DrawCircle((float)particle.Position.X, (float)particle.Position.Y, (float)particle.Size, paint);
        }

        surface.Canvas.Flush();
    }

    private SKColor PickColor()
    {
        var colors = Helpers.ThemeAssets.GetParticleColors(Helpers.ThemeAssets.CurrentThemeName);
        var color = colors[_rng.Next(colors.Count)];
        return new SKColor(color.R, color.G, color.B, color.A);
    }

    private static void DrawHeart(SKCanvas canvas, SKPaint paint, Particle particle)
    {
        var x = (float)particle.Position.X;
        var y = (float)particle.Position.Y;
        var s = (float)particle.Size;

        using var path = new SKPath();
        path.MoveTo(x, y + s * 0.35f);
        path.CubicTo(x - s * 1.35f, y - s * 0.5f, x - s * 0.85f, y - s * 1.45f, x, y - s * 0.75f);
        path.CubicTo(x + s * 0.85f, y - s * 1.45f, x + s * 1.35f, y - s * 0.5f, x, y + s * 0.35f);
        path.Close();
        canvas.DrawPath(path, paint);
    }

    private sealed class Particle
    {
        private readonly double _spin;
        private readonly double _initialLife;

        public Point Position { get; private set; }
        public Vector Velocity { get; private set; }
        public double Life { get; private set; }
        public double Size { get; }
        public bool IsHeart { get; }
        public SKColor BaseColor { get; }
        public SKColor SkiaColor => BaseColor.WithAlpha((byte)Math.Clamp(255 * Life / _initialLife, 0, BaseColor.Alpha));
        public bool IsAlive => Life > 0;

        private Particle(Point position, Vector velocity, SKColor color, double size, double life, bool isHeart, double spin)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            Size = size;
            Life = life;
            IsHeart = isHeart;
            _spin = spin;
            _initialLife = life;
        }

        public static Particle Burst(Point position, Vector velocity, SKColor color, bool isHeart) =>
            new(position, velocity, color, 4 + Random.Shared.NextDouble() * 8, 0.75 + Random.Shared.NextDouble() * 0.55, isHeart, 0);

        public static Particle Petal(Point position, Vector velocity, SKColor color) =>
            new(position, velocity, color, 3 + Random.Shared.NextDouble() * 5, 4.5 + Random.Shared.NextDouble() * 1.5, true, (Random.Shared.NextDouble() - 0.5) * 40);

        public void Update(double deltaSeconds)
        {
            Life -= deltaSeconds;
            Velocity += new Vector(0, 150 * deltaSeconds);
            Position += Velocity * deltaSeconds + new Vector(Math.Sin(Life * _spin) * deltaSeconds, 0);
        }
    }
}
