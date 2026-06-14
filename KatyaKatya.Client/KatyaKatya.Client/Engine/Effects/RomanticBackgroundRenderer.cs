using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace KatyaKatya.Engine.Effects;

internal sealed class RomanticBackgroundRenderer
{
    private static readonly SKColor Rose = new(255, 218, 224);
    private static readonly SKColor Lavender = new(240, 230, 240);
    private static readonly SKColor Bubble = new(255, 182, 193);
    private static readonly SKColor CloudLight = new(255, 255, 255);
    private static readonly SKColor CloudShadow = new(212, 190, 202);

    private readonly Random _rng = new();
    private readonly MistLayer[] _mist = new MistLayer[6];
    private readonly CloudLayer[] _clouds = new CloudLayer[3];
    private readonly BokehLayer[] _bubbles = new BokehLayer[18];
    private readonly RomanticBackgroundScene[] _scenes = [new(), new()];
    private int _sceneIndex;

    private float _mouseX;
    private float _mouseY;

    public RomanticBackgroundRenderer()
    {
        InitializeMist();
        InitializeClouds();
        InitializeBubbles();
    }

    public int LayerCount => _mist.Length + _clouds.Length + _bubbles.Length + 2;

    public void Update(double dt, Size bounds, Point targetPointer)
    {
        var width = Math.Max(1f, (float)bounds.Width);
        var height = Math.Max(1f, (float)bounds.Height);
        var delta = (float)Math.Clamp(dt, 0.001, 0.05);

        _mouseX += ((float)targetPointer.X - _mouseX) * 4f * delta;
        _mouseY += ((float)targetPointer.Y - _mouseY) * 4f * delta;

        for (var i = 0; i < _mist.Length; i++)
        {
            ref var m = ref _mist[i];
            m.PulsePhase += m.PulseSpeed * delta;
            m.X += m.VelocityX * delta;
            m.ErraticPhaseX += 1.2f * delta;
            m.ErraticPhaseY += 0.8f * delta;
            if (m.LayerType == 2)
            {
                m.X += MathF.Sin(m.ErraticPhaseX) * 30f * delta;
                m.Y = m.BaseY + MathF.Sin(m.ErraticPhaseY) * 40f;
            }

            if (m.VelocityX >= 0 && m.X > width + m.Width) m.X = -m.Width;
            if (m.VelocityX < 0 && m.X < -m.Width) m.X = width + m.Width;
        }

        for (var i = 0; i < _clouds.Length; i++)
        {
            ref var c = ref _clouds[i];
            c.PulsePhase += c.PulseSpeed * delta;
            c.ErraticPhase += 0.8f * delta;
            c.X += (c.VelocityX + MathF.Sin(c.ErraticPhase * 1.5f) * 15f) * delta;
            c.Y = c.BaseY + MathF.Sin(c.ErraticPhase) * 20f;

            if (c.VelocityX >= 0 && c.X > width + c.Width) c.X = -c.Width;
            if (c.VelocityX < 0 && c.X < -c.Width) c.X = width + c.Width;
        }

        for (var i = 0; i < _bubbles.Length; i++)
        {
            ref var b = ref _bubbles[i];
            b.PulsePhase += b.PulseSpeed * delta;
            b.Y += b.VelocityY * b.SpeedFactor * 40f * delta;
            b.X += b.VelocityX * b.SpeedFactor * 15f * delta;

            if (b.Y < -b.Radius * 2)
            {
                b.Y = height + b.Radius;
                b.X = (float)(_rng.NextDouble() * width);
            }
            if (b.X < -b.Radius * 2) b.X = width + b.Radius;
            if (b.X > width + b.Radius * 2) b.X = -b.Radius;
        }
    }

    public ICustomDrawOperation CreateDrawOperation(Rect bounds, float density)
    {
        var scene = _scenes[_sceneIndex];
        _sceneIndex ^= 1;
        FillScene(scene, bounds, Math.Clamp(density, 0f, 1f));
        return new RomanticBackgroundDrawOperation(bounds, scene);
    }

    private void FillScene(RomanticBackgroundScene scene, Rect bounds, float density)
    {
        scene.Bounds = bounds;
        scene.MouseX = _mouseX;
        scene.MouseY = _mouseY;
        scene.MistCount = CopyCount(_mist.Length, density);
        scene.CloudCount = CopyCount(_clouds.Length, density);
        scene.BubbleCount = CopyCount(_bubbles.Length, density);
        Array.Copy(_mist, scene.Mist, scene.MistCount);
        Array.Copy(_clouds, scene.Clouds, scene.CloudCount);
        Array.Copy(_bubbles, scene.Bubbles, scene.BubbleCount);
    }

    private static int CopyCount(int max, float density) =>
        Math.Clamp((int)MathF.Ceiling(max * Math.Max(0.2f, density)), 1, max);

    private void InitializeMist()
    {
        for (var i = 0; i < _mist.Length; i++)
        {
            var layerType = i / 2;
            _mist[i] = new MistLayer
            {
                X = _rng.Next(-400, 1800),
                Y = _rng.Next(80, 720),
                BaseY = _rng.Next(80, 720),
                VelocityX = layerType == 1 ? -50f : 30f,
                Width = 800 + layerType * 300 + _rng.Next(0, 250),
                Height = 300 + layerType * 120 + _rng.Next(0, 180),
                PulseSpeed = 0.15f + (float)_rng.NextDouble() * 0.15f,
                PulsePhase = (float)(_rng.NextDouble() * Math.PI * 2),
                SpeedFactor = 0.12f + layerType * 0.3f,
                LayerType = layerType,
                BaseOpacity = 0.16f + layerType * 0.06f,
                Color = i % 2 == 0 ? Rose : Lavender
            };
        }
    }

    private void InitializeClouds()
    {
        _clouds[0] = NewCloud(1500, 650, 80, 22, 0.22f, 0.32f);
        _clouds[1] = NewCloud(1100, 480, 160, 42, 0.50f, 0.46f);
        _clouds[2] = NewCloud(1400, 580, 360, -52, 0.82f, 0.52f);
    }

    private CloudLayer NewCloud(float width, float height, float y, float vx, float speed, float opacity) => new()
    {
        X = _rng.Next(-300, 1300),
        Y = y,
        BaseY = y,
        VelocityX = vx,
        Width = width,
        Height = height,
        PulseSpeed = 0.16f + (float)_rng.NextDouble() * 0.1f,
        PulsePhase = (float)(_rng.NextDouble() * Math.PI * 2),
        SpeedFactor = speed,
        BaseOpacity = opacity,
        ErraticPhase = (float)(_rng.NextDouble() * Math.PI * 2)
    };

    private void InitializeBubbles()
    {
        for (var i = 0; i < _bubbles.Length; i++)
        {
            var tier = _rng.Next(0, 3);
            _bubbles[i] = new BokehLayer
            {
                X = _rng.Next(0, 1920),
                Y = _rng.Next(0, 1080),
                VelocityX = _rng.Next(-15, 15) / 10f,
                VelocityY = _rng.Next(-18, -5) / 10f,
                Radius = tier switch { 0 => _rng.Next(35, 75), 1 => _rng.Next(85, 135), _ => _rng.Next(150, 220) },
                PulseSpeed = 0.4f + (float)_rng.NextDouble() * 0.8f,
                PulsePhase = (float)(_rng.NextDouble() * Math.PI * 2),
                SpeedFactor = 0.2f + tier * 0.55f
            };
        }
    }

    internal struct MistLayer
    {
        public float X, Y, BaseY, VelocityX, Width, Height, PulseSpeed, PulsePhase, SpeedFactor;
        public float ErraticPhaseX, ErraticPhaseY, BaseOpacity;
        public int LayerType;
        public SKColor Color;
    }

    internal struct CloudLayer
    {
        public float X, Y, BaseY, VelocityX, Width, Height, PulseSpeed, PulsePhase, SpeedFactor, BaseOpacity, ErraticPhase;
    }

    internal struct BokehLayer
    {
        public float X, Y, VelocityX, VelocityY, Radius, PulseSpeed, PulsePhase, SpeedFactor;
    }

    private sealed class RomanticBackgroundScene
    {
        public Rect Bounds;
        public float MouseX, MouseY;
        public readonly MistLayer[] Mist = new MistLayer[6];
        public readonly CloudLayer[] Clouds = new CloudLayer[3];
        public readonly BokehLayer[] Bubbles = new BokehLayer[18];
        public int MistCount, CloudCount, BubbleCount;
    }

    private sealed class RomanticBackgroundDrawOperation : ICustomDrawOperation
    {
        private readonly RomanticBackgroundScene _scene;

        public RomanticBackgroundDrawOperation(Rect bounds, RomanticBackgroundScene scene)
        {
            Bounds = bounds;
            _scene = scene;
        }

        public Rect Bounds { get; }
        public bool HitTest(Point p) => false;
        public bool Equals(ICustomDrawOperation? other) => false;
        public void Dispose() { }

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature is null)
                return;

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;
            using var paint = new SKPaint { IsAntialias = true };

            var width = (float)Math.Max(1, Bounds.Width);
            var height = (float)Math.Max(1, Bounds.Height);
            var parallaxX = -(_scene.MouseX - width / 2f) * 0.025f;
            var parallaxY = -(_scene.MouseY - height / 2f) * 0.025f;

            canvas.Save();
            canvas.ClipRect(new SKRect(0, 0, width, height));
            DrawSpotlight(canvas, paint, width, height);
            DrawMist(canvas, paint, parallaxX, parallaxY);
            DrawClouds(canvas, paint, parallaxX, parallaxY);
            DrawBubbles(canvas, paint, parallaxX, parallaxY);
            DrawVignette(canvas, paint, width, height);
            canvas.Restore();
        }

        private void DrawSpotlight(SKCanvas canvas, SKPaint paint, float width, float height)
        {
            paint.Style = SKPaintStyle.Fill;
            paint.Shader = SKShader.CreateRadialGradient(
                new SKPoint(width * 0.5f, height * 0.35f),
                Math.Max(width, height) * 0.65f,
                [new SKColor(255, 218, 224, 130), new SKColor(240, 220, 240, 60), SKColors.Transparent],
                [0f, 0.5f, 1f],
                SKShaderTileMode.Clamp);
            canvas.DrawRect(0, 0, width, height, paint);
            paint.Shader = null;
        }

        private void DrawMist(SKCanvas canvas, SKPaint paint, float px, float py)
        {
            for (var i = 0; i < _scene.MistCount; i++)
            {
                var m = _scene.Mist[i];
                var breath = 0.65f + 0.35f * MathF.Sin(m.PulsePhase);
                var scale = 1.0f + 0.04f * MathF.Sin(m.PulsePhase);
                var x = m.X + px * m.SpeedFactor - m.Width / 2f;
                var y = m.Y + py * m.SpeedFactor - m.Height / 2f;
                DrawSoftOval(canvas, paint, x, y, m.Width * scale, m.Height * scale, m.Color.WithAlpha((byte)(255 * m.BaseOpacity * breath)));
            }
        }

        private static void DrawSoftOval(SKCanvas canvas, SKPaint paint, float x, float y, float w, float h, SKColor color)
        {
            paint.Shader = SKShader.CreateRadialGradient(
                new SKPoint(x + w / 2f, y + h / 2f),
                Math.Max(w, h) / 2f,
                [color, color.WithAlpha(0)],
                [0f, 1f],
                SKShaderTileMode.Clamp);
            canvas.DrawOval(new SKRect(x, y, x + w, y + h), paint);
            paint.Shader = null;
        }

        private void DrawClouds(SKCanvas canvas, SKPaint paint, float px, float py)
        {
            for (var i = 0; i < _scene.CloudCount; i++)
            {
                var c = _scene.Clouds[i];
                var scale = 1.0f + 0.03f * MathF.Sin(c.PulsePhase);
                var alpha = (byte)(255 * c.BaseOpacity * (0.9f + 0.1f * MathF.Sin(c.PulsePhase)));
                var x = c.X + px * c.SpeedFactor;
                var y = c.Y + py * c.SpeedFactor;

                DrawCloudPuff(canvas, paint, x, y, c.Width * scale, c.Height * scale, alpha);
            }
        }

        private static void DrawCloudPuff(SKCanvas canvas, SKPaint paint, float x, float y, float w, float h, byte alpha)
        {
            DrawSoftOval(canvas, paint, x + w * 0.12f, y + h * 0.45f, w * 0.76f, h * 0.28f, CloudShadow.WithAlpha((byte)(alpha * 0.45f)));
            DrawSoftOval(canvas, paint, x + w * 0.08f, y + h * 0.34f, w * 0.32f, h * 0.28f, CloudLight.WithAlpha(alpha));
            DrawSoftOval(canvas, paint, x + w * 0.28f, y + h * 0.22f, w * 0.36f, h * 0.32f, CloudLight.WithAlpha(alpha));
            DrawSoftOval(canvas, paint, x + w * 0.54f, y + h * 0.32f, w * 0.36f, h * 0.30f, CloudLight.WithAlpha(alpha));
            DrawSoftOval(canvas, paint, x + w * 0.22f, y + h * 0.42f, w * 0.58f, h * 0.26f, new SKColor(255, 248, 250, alpha));
        }

        private void DrawBubbles(SKCanvas canvas, SKPaint paint, float px, float py)
        {
            for (var i = 0; i < _scene.BubbleCount; i++)
            {
                var b = _scene.Bubbles[i];
                var pulse = 1.0f + 0.12f * MathF.Sin(b.PulsePhase);
                var r = b.Radius * pulse;
                DrawSoftOval(canvas, paint,
                    b.X + px * b.SpeedFactor - r,
                    b.Y + py * b.SpeedFactor - r,
                    r * 2f,
                    r * 2f,
                    Bubble.WithAlpha(34));
            }
        }

        private void DrawVignette(SKCanvas canvas, SKPaint paint, float width, float height)
        {
            paint.Shader = SKShader.CreateRadialGradient(
                new SKPoint(width / 2f, height / 2f),
                Math.Max(width, height) * 0.72f,
                [SKColors.Transparent, new SKColor(20, 10, 20, 72)],
                [0f, 1f],
                SKShaderTileMode.Clamp);
            canvas.DrawRect(0, 0, width, height, paint);
            paint.Shader = null;
        }
    }
}

