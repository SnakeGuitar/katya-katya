using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using SkiaSharp.Views.Desktop;

namespace MemoryGame.Client.Engine.Animations;

/// <summary>
/// Tipos de partículas visuales soportadas por el motor.
/// </summary>
public enum ParticleType
{
    Heart,
    Star,
    Sparkle,
    TrailPoint
}

/// <summary>
/// Representación extremadamente ligera de una partícula en memoria.
/// Estructura plana para evitar presión sobre el Garbage Collector.
/// </summary>
public struct Particle
{
    public float X;
    public float Y;
    public float VelocityX;
    public float VelocityY;
    public float Gravity;
    public float Drag;
    public float Size;
    
    public float StartAngle;
    public float TargetAngle;
    
    public float Age;        // Tiempo transcurrido (incluyendo retraso inicial)
    public float Lifetime;   // Duración de la animación activa
    public float Delay;      // Retraso inicial en segundos

    public ParticleType Type;
    public SKColor Color;
    public SKShader Shader;
    public SKPath Path;

    // Buffer circular en línea para el rastro (Motion Blur) sin asignación en el Heap — optimizado a 2 frames
    public float HistoryX0, HistoryX1;
    public float HistoryY0, HistoryY1;
    public int HistoryCount;
}

/// <summary>
/// Estructura que representa una onda de choque radial expansiva tras un match.
/// </summary>
public struct Shockwave
{
    public float CenterX;
    public float CenterY;
    public float CurrentRadius;
    public float MaxRadius;
    public float Lifetime;
    public float Age;
    public SKColor Color;
}

/// <summary>
/// Estructura que representa un número flotante animado para combos y puntuaciones.
/// </summary>
public struct FloatingText
{
    public string Text;
    public float X;
    public float Y;
    public float VelocityY;
    public float Age;
    public float Lifetime;
    public float Scale;
    public SKColor Color;
}

/// <summary>
/// Reusable service for gameplay animations: love-point particles and Game Over overlay.
/// Upgraded to a complete high-performance, physics-based 2D juice engine using SkiaSharp.
/// </summary>
public sealed class GameAnimationService : IDisposable
{
    private static readonly Random Rng = new();

    private readonly SKElement _canvas;
    private readonly Func<Point> _anchorProvider;
    
    // Entidades del motor en memoria (GC-friendly)
    private readonly List<Particle> _particles = new();
    private readonly List<Shockwave> _shockwaves = new();
    private readonly List<FloatingText> _floatingTexts = new();
    
    // Recursos gráficos nativos cacheados
    private readonly SKBitmap? _bitmap;
    private readonly SKPaint _paint;
    private readonly SKTypeface _fontTypeface;
    private readonly SKFont _textFont;

    // Vector Paths pre-calculados (Zero Allocations en render tick)
    private readonly SKPath _heartPath;
    private readonly SKPath _starPath;
    private readonly SKPath _sparklePath;

    // Gradientes HSL pre-calculados (Zero Allocations en render tick)
    private readonly SKShader _heartShader;
    private readonly SKShader _starShader;
    private readonly SKShader _sparkleShader;

    // Control del Render Loop
    private readonly Stopwatch _stopwatch = new();
    private double _lastElapsedTime;
    private bool _isRenderingHooked;

    // Control inteligente del Combo automático si no es provisto por el ViewModel
    private DateTime _lastSpawnTime = DateTime.MinValue;
    private int _currentCombo = 1;

    /// <param name="canvas">The overlay SKElement for hardware-compatible transparent particles (IsHitTestVisible=False).</param>
    /// <param name="anchorProvider">Returns the anchor point (in canvas coordinates) each time particles spawn.</param>
    public GameAnimationService(SKElement canvas, Func<Point> anchorProvider)
    {
        _canvas = canvas;
        _anchorProvider = anchorProvider;

        // Cargar recurso de imagen original love-points con fallback seguro
        try
        {
            _bitmap = LoadBitmapFromResource("pack://application:,,,/Resources/Images/Icons/love-points.png");
        }
        catch
        {
            _bitmap = null;
        }

        _paint = new SKPaint
        {
            IsAntialias = true
        };

        // Cargar Tipografía Premium para Textos Flotantes
        _fontTypeface = SKTypeface.FromFamilyName("Outfit") 
                        ?? SKTypeface.FromFamilyName("Inter")
                        ?? SKTypeface.FromFamilyName("Segoe UI")
                        ?? SKTypeface.FromFamilyName("Arial")
                        ?? SKTypeface.Default;

        _textFont = new SKFont(_fontTypeface, 22f);

        // Inicializar Caminos Vectoriales Matemáticos (SKPath)
        _heartPath = CreateHeartPath(16f);
        _starPath = CreateStarPath(16f, 6.5f);
        _sparklePath = CreateSparklePath(16f, 3.5f);

        // Inicializar Shaders con paletas de colores armónicas (Rose Gold, Magical Gold, Electric Blue)
        _heartShader = SKShader.CreateLinearGradient(
            new SKPoint(-16, -16),
            new SKPoint(16, 16),
            new[] { new SKColor(255, 105, 180), new SKColor(255, 182, 193) }, // Hot Pink -> Light Pink (Rose Gold Feel)
            null,
            SKShaderTileMode.Clamp);

        _starShader = SKShader.CreateLinearGradient(
            new SKPoint(-16, -16),
            new SKPoint(16, 16),
            new[] { new SKColor(255, 215, 0), new SKColor(255, 140, 0) }, // Gold -> Orange (Magical Gold)
            null,
            SKShaderTileMode.Clamp);

        _sparkleShader = SKShader.CreateLinearGradient(
            new SKPoint(-16, -16),
            new SKPoint(16, 16),
            new[] { new SKColor(0, 255, 255), new SKColor(0, 128, 255) }, // Cyan -> Electric Blue
            null,
            SKShaderTileMode.Clamp);

        // Suscribirse al evento de renderizado de SkiaSharp
        _canvas.PaintSurface += OnPaintSurface;
    }

    private static SKBitmap LoadBitmapFromResource(string uriString)
    {
        var uri = new Uri(uriString);
        var resourceStream = Application.GetResourceStream(uri);
        if (resourceStream == null)
        {
            throw new InvalidOperationException($"Could not load application resource: {uriString}");
        }

        using var stream = resourceStream.Stream;
        return SKBitmap.Decode(stream);
    }

    // ── Caminos Vectoriales ───────────────────────────────────────────────

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
        int points = 5;
        double angleStep = Math.PI / points;
        double angle = -Math.PI / 2; // Iniciar desde arriba

        path.MoveTo((float)(Math.Cos(angle) * radius), (float)(Math.Sin(angle) * radius));
        for (int i = 0; i < points * 2; i++)
        {
            angle += angleStep;
            float r = (i % 2 == 0) ? innerRadius : radius;
            path.LineTo((float)(Math.Cos(angle) * r), (float)(Math.Sin(angle) * r));
        }
        path.Close();
        return path;
    }

    private static SKPath CreateSparklePath(float radius, float innerWidth)
    {
        var path = new SKPath();
        // Sparkle estilizado de 4 puntas (curvas suaves hacia el centro)
        path.MoveTo(0, -radius);
        path.QuadTo(0, 0, radius, 0);
        path.QuadTo(0, 0, 0, radius);
        path.QuadTo(0, 0, -radius, 0);
        path.QuadTo(0, 0, 0, -radius);
        path.Close();
        return path;
    }

    // ── Game Over ─────────────────────────────────────────────────────────

    public static void PlayGameOver(Border overlay, Border card)
    {
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.35));
        overlay.BeginAnimation(UIElement.OpacityProperty, fadeIn);

        var bounce = new BounceEase { Bounces = 2, Bounciness = 3, EasingMode = EasingMode.EaseOut };
        var slideUp = new DoubleAnimation(40, 0, TimeSpan.FromSeconds(0.4)) { EasingFunction = bounce };

        if (card.RenderTransform is TranslateTransform tt)
            tt.BeginAnimation(TranslateTransform.YProperty, slideUp);
    }

    // ── Particles ─────────────────────────────────────────────────────────

    private const int MAX_TOTAL_PARTICLES = 50;  // Limit total particles for performance (reduced for high res)

    /// <summary>
    /// Spawns particle explosion, radial shockwave, and floating combo texts.
    /// Compatible with original signature, and introduces combo multiplier styling dynamically.
    /// </summary>
    public void SpawnParticles(int count = 12)
    {
        SpawnParticles(count, 1);
    }

    /// <summary>
    /// Spawns particles reacting directly to a combo multiplier.
    /// </summary>
    public void SpawnParticles(int count, int comboMultiplier)
    {
        var anchor = _anchorProvider();

        // 1. Detección automática del combo si se llama con parámetros por defecto
        var now = DateTime.UtcNow;
        if (comboMultiplier == 1)
        {
            if ((now - _lastSpawnTime).TotalSeconds <= 3.5)
            {
                _currentCombo = Math.Min(4, _currentCombo + 1);
            }
            else
            {
                _currentCombo = 1;
            }
        }
        else
        {
            _currentCombo = comboMultiplier;
        }
        _lastSpawnTime = now;

        int effectiveCombo = _currentCombo;

        // Escalar cantidad de partículas basándonos en el combo, pero limitar el total
        int particleCount = (count == 12) ? (12 * effectiveCombo) : count;

        // Clamp total particles to prevent performance issues
        lock (_particles)
        {
            int availableSlots = MAX_TOTAL_PARTICLES - _particles.Count;
            particleCount = Math.Min(particleCount, availableSlots);
        }

        // Configuración de la paleta armónica y el tipo de partícula según combo
        ParticleType pType;
        SKColor pColor;
        SKShader pShader;
        SKPath pPath;

        if (effectiveCombo == 1)
        {
            pType = ParticleType.Heart;
            pColor = new SKColor(255, 105, 180); // Warm Pink
            pShader = _heartShader;
            pPath = _heartPath;
        }
        else if (effectiveCombo == 2)
        {
            pType = ParticleType.Star;
            pColor = new SKColor(255, 215, 0); // Magical Gold
            pShader = _starShader;
            pPath = _starPath;
        }
        else
        {
            pType = ParticleType.Sparkle;
            pColor = new SKColor(0, 255, 255); // Electric Blue/Cyan
            pShader = _sparkleShader;
            pPath = _sparklePath;
        }

        lock (_particles)
        {
            for (int i = 0; i < particleCount; i++)
            {
                float size = Rng.Next(18, 32);
                float startX = (float)anchor.X;
                float startY = (float)anchor.Y;

                // Dinámica explosiva en 360 grados
                float speed = Rng.Next(100, 250) + (effectiveCombo * 35f);
                double angleRad = Rng.NextDouble() * 2.0 * Math.PI;

                float velX = (float)(Math.Cos(angleRad) * speed);
                float velY = (float)(Math.Sin(angleRad) * speed) - 80f; // Sesgo hacia arriba

                float delay = Rng.Next(0, 150) / 1000.0f;
                float dur = 0.6f + Rng.Next(0, 40) / 100.0f;

                float startAngle = Rng.Next(-180, 180);
                float targetAngle = startAngle + Rng.Next(-180, 180);

                var particle = new Particle
                {
                    X = startX,
                    Y = startY,
                    VelocityX = velX,
                    VelocityY = velY,
                    Gravity = 380f, // Gravedad que atrae las partículas hacia abajo
                    Drag = 0.95f,    // Resistencia al aire
                    StartAngle = startAngle,
                    TargetAngle = targetAngle,
                    Size = size,
                    Age = 0f,
                    Lifetime = dur,
                    Delay = delay,
                    Type = pType,
                    Color = pColor,
                    Shader = pShader,
                    Path = pPath,
                    // Inicializar historial de rastro — 2 frames
                    HistoryX0 = startX, HistoryY0 = startY,
                    HistoryX1 = startX, HistoryY1 = startY,
                    HistoryCount = 0
                };

                _particles.Add(particle);
            }
        }

        // 2. Disparar Onda de Choque Radial (Shockwave)
        lock (_shockwaves)
        {
            _shockwaves.Add(new Shockwave
            {
                CenterX = (float)anchor.X,
                CenterY = (float)anchor.Y,
                CurrentRadius = 0f,
                MaxRadius = 60f + 25f * effectiveCombo, // Más expansivo con mayor combo
                Lifetime = 0.45f + 0.10f * effectiveCombo,
                Age = 0f,
                Color = pColor
            });
        }

        // 3. Disparar Popups de Texto Flotante (Floating Score Text)
        lock (_floatingTexts)
        {
            string label;
            float fontScale;
            if (effectiveCombo == 1)
            {
                label = "+100";
                fontScale = 1.0f;
            }
            else if (effectiveCombo == 2)
            {
                label = "COMBO X2! +200";
                fontScale = 1.25f;
            }
            else
            {
                label = $"MEGA COMBO X{effectiveCombo}! +{effectiveCombo * 100}";
                fontScale = 1.45f;
            }

            _floatingTexts.Add(new FloatingText
            {
                Text = label,
                X = (float)anchor.X,
                Y = (float)(anchor.Y - 20f),
                VelocityY = -150f - 25f * effectiveCombo, // Asciende más rápido con mayor combo
                Age = 0f,
                Lifetime = 1.15f,
                Scale = fontScale,
                Color = pColor
            });
        }

        // Iniciar Loop de Renderizado si no está activo
        if (!_isRenderingHooked)
        {
            _stopwatch.Restart();
            _lastElapsedTime = 0;
            HookRendering();
        }
    }

    private void HookRendering()
    {
        if (!_isRenderingHooked)
        {
            CompositionTarget.Rendering += OnRenderingTick;
            _isRenderingHooked = true;
        }
    }

    private void UnhookRendering()
    {
        if (_isRenderingHooked)
        {
            CompositionTarget.Rendering -= OnRenderingTick;
            _isRenderingHooked = false;
        }
    }

    private void OnRenderingTick(object? sender, EventArgs e)
    {
        double currentElapsed = _stopwatch.Elapsed.TotalSeconds;
        double deltaTime = currentElapsed - _lastElapsedTime;
        _lastElapsedTime = currentElapsed;

        UpdateParticles((float)deltaTime);
    }

    private void UpdateParticles(float deltaTime)
    {
        bool anyActive = false;

        // Actualizar Partículas
        lock (_particles)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Age += deltaTime;

                if (p.Age >= p.Delay + p.Lifetime)
                {
                    _particles.RemoveAt(i);
                }
                else
                {
                    if (p.Age >= p.Delay)
                    {
                        // Registrar historial de rastro (Motion Blur) — optimizado a 2 frames
                        p.HistoryX1 = p.HistoryX0;
                        p.HistoryY1 = p.HistoryY0;
                        p.HistoryX0 = p.X;
                        p.HistoryY0 = p.Y;
                        p.HistoryCount = Math.Min(2, p.HistoryCount + 1);

                        // Ecuaciones de Física Reales
                        p.VelocityY += p.Gravity * deltaTime;
                        p.VelocityX *= (1f - p.Drag * deltaTime);
                        p.VelocityY *= (1f - p.Drag * deltaTime);
                        p.X += p.VelocityX * deltaTime;
                        p.Y += p.VelocityY * deltaTime;
                    }

                    _particles[i] = p;
                    anyActive = true;
                }
            }
        }

        // Actualizar Ondas de Choque
        lock (_shockwaves)
        {
            for (int i = _shockwaves.Count - 1; i >= 0; i--)
            {
                var s = _shockwaves[i];
                s.Age += deltaTime;

                if (s.Age >= s.Lifetime)
                {
                    _shockwaves.RemoveAt(i);
                }
                else
                {
                    // Expansión orgánica con Ease Out cuadrático
                    float t = s.Age / s.Lifetime;
                    float easedT = t * (2f - t);
                    s.CurrentRadius = easedT * s.MaxRadius;

                    _shockwaves[i] = s;
                    anyActive = true;
                }
            }
        }

        // Actualizar Textos Flotantes
        lock (_floatingTexts)
        {
            for (int i = _floatingTexts.Count - 1; i >= 0; i--)
            {
                var ft = _floatingTexts[i];
                ft.Age += deltaTime;

                if (ft.Age >= ft.Lifetime)
                {
                    _floatingTexts.RemoveAt(i);
                }
                else
                {
                    // Desaceleración ascendente natural
                    ft.VelocityY += 85f * deltaTime;
                    ft.Y += ft.VelocityY * deltaTime;

                    _floatingTexts[i] = ft;
                    anyActive = true;
                }
            }
        }

        if (anyActive)
        {
            _canvas.InvalidateVisual();
        }
        else
        {
            UnhookRendering();
            _stopwatch.Stop();
        }
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        // 1. Dibujar Ondas de Choque (Shockwaves)
        lock (_shockwaves)
        {
            foreach (var s in _shockwaves)
            {
                float t = s.Age / s.Lifetime;
                float opacity = 1.0f - t;
                float strokeWidth = 8f * (1.0f - t);

                _paint.Shader = null;
                _paint.Style = SKPaintStyle.Stroke;
                _paint.StrokeWidth = strokeWidth;
                _paint.Color = s.Color.WithAlpha((byte)(opacity * 255));

                canvas.DrawCircle(s.CenterX, s.CenterY, s.CurrentRadius, _paint);
            }
        }

        // 2. Dibujar Partículas con Efecto de Rastro (Motion Blur)
        lock (_particles)
        {
            foreach (var p in _particles)
            {
                if (p.Age < p.Delay)
                    continue;

                float t = Math.Clamp((p.Age - p.Delay) / p.Lifetime, 0f, 1f);

                // Ángulo de rotación lineal
                float angle = p.StartAngle + (p.TargetAngle - p.StartAngle) * t;

                // Escala elástica: Pop de entrada -> shrink a 0
                float baseScale = CalculateScale(t);
                float scale = baseScale * (p.Size / 24f);

                // Desvanecimiento al final del ciclo de vida
                float opacity = 1.0f;
                if (t > 0.55f)
                {
                    opacity = 1.0f - (t - 0.55f) / 0.45f;
                }

                // 2A. Dibujar Rastro (Fading Clones de Historial) — optimizado a 2 frames
                for (int h = p.HistoryCount - 1; h >= 0; h--)
                {
                    float hX, hY;
                    if (h == 0) { hX = p.HistoryX0; hY = p.HistoryY0; }
                    else { hX = p.HistoryX1; hY = p.HistoryY1; }

                    float indexFactor = (float)(h + 1) / 3f;
                    float trailScale = scale * (1.0f - indexFactor);
                    if (trailScale <= 0f) continue;

                    float trailOpacity = opacity * (1.0f - indexFactor * 0.8f);
                    if (trailOpacity <= 0f) continue;

                    canvas.Save();
                    canvas.Translate(hX, hY);
                    canvas.RotateDegrees(angle);
                    canvas.Scale(trailScale, trailScale);

                    _paint.Shader = p.Shader;
                    _paint.Color = p.Color.WithAlpha((byte)(trailOpacity * 255));
                    _paint.Style = SKPaintStyle.Fill;

                    canvas.DrawPath(p.Path, _paint);
                    canvas.Restore();
                }

                // 2B. Dibujar Partícula Principal
                canvas.Save();
                canvas.Translate(p.X, p.Y);
                canvas.RotateDegrees(angle);
                canvas.Scale(scale, scale);

                _paint.Shader = p.Shader;
                _paint.Color = p.Color.WithAlpha((byte)(opacity * 255));
                _paint.Style = SKPaintStyle.Fill;

                canvas.DrawPath(p.Path, _paint);
                canvas.Restore();
            }
        }

        _paint.Shader = null; // Limpiar shader para dibujos posteriores

        // 3. Dibujar Textos Flotantes (Floating Score UI)
        lock (_floatingTexts)
        {
            foreach (var ft in _floatingTexts)
            {
                float t = ft.Age / ft.Lifetime;
                float opacity = 1.0f - (float)Math.Pow(t, 2); // Fading más rápido al final
                float scale = GetBounceScale(t) * ft.Scale;

                byte alpha = (byte)(opacity * 255);
                if (alpha == 0) continue;

                canvas.Save();
                canvas.Translate(ft.X, ft.Y);
                canvas.Scale(scale, scale);

                // Configurar Pintura de Texto
                _paint.Shader = null;

                // Medir para centrar horizontalmente el texto usando el SKFont moderno
                float textWidth = _textFont.MeasureText(ft.Text);
                float textX = -textWidth / 2f;
                float textY = 0f;

                // Doble pase para un delineado limpio (profesional UI)
                // Pase 1: Outline Oscuro
                _paint.Style = SKPaintStyle.Stroke;
                _paint.StrokeWidth = 4.5f;
                _paint.Color = SKColors.Black.WithAlpha(alpha);
                canvas.DrawText(ft.Text, textX, textY, _textFont, _paint);

                // Pase 2: Relleno de Color Brillante
                _paint.Style = SKPaintStyle.Fill;
                _paint.Color = ft.Color.WithAlpha(alpha);
                canvas.DrawText(ft.Text, textX, textY, _textFont, _paint);

                canvas.Restore();
            }
        }
    }

    private static float CalculateScale(float t)
    {
        // Easing elástico cuadrático
        if (t <= 0.2f)
        {
            float p = t / 0.2f;
            float easedP = p * (2f - p);
            return 0.3f + (1.2f - 0.3f) * easedP;
        }
        else if (t <= 0.4f)
        {
            float p = (t - 0.2f) / 0.2f;
            float easedP = p * (2f - p);
            return 1.2f + (1.0f - 1.2f) * easedP;
        }
        else
        {
            float p = (t - 0.4f) / 0.6f;
            float easedP = p * (2f - p);
            return 1.0f + (0.0f - 1.0f) * easedP;
        }
    }

    private static float GetBounceScale(float t)
    {
        // Curva elástica/rebote para popups de texto "Damage Numbers"
        if (t < 0.3f)
        {
            float p = t / 0.3f;
            return p * 1.3f; // Pop inicial por encima de 1.0
        }
        else if (t < 0.6f)
        {
            float p = (t - 0.3f) / 0.3f;
            return 1.3f - 0.35f * p; // Asentarse levemente hacia abajo
        }
        else if (t < 0.8f)
        {
            float p = (t - 0.6f) / 0.2f;
            return 0.95f + 0.1f * p; // Pequeña corrección hacia arriba
        }
        else
        {
            return 1.05f - 0.05f * ((t - 0.8f) / 0.2f); // Asentarse en 1.0 al final
        }
    }

    public void Dispose()
    {
        UnhookRendering();
        _canvas.PaintSurface -= OnPaintSurface;

        // Disponer Caminos Vectoriales creados nativamente
        _heartPath.Dispose();
        _starPath.Dispose();
        _sparklePath.Dispose();

        // Disponer Shaders creados nativamente
        _heartShader.Dispose();
        _starShader.Dispose();
        _sparkleShader.Dispose();

        // Disponer Recursos Adicionales
        _textFont.Dispose();
        _fontTypeface.Dispose();
        _paint.Dispose();
        _bitmap?.Dispose();

        GC.SuppressFinalize(this);
    }
}

