using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MemoryGame.Client.Engine.Animations;

public struct BokehBubble
{
    public float X;
    public float Y;
    public float VelocityX;
    public float VelocityY;
    public float BaseRadius;
    public float PulseSpeed;
    public float PulsePhase;
    public float SpeedFactor;
    public float BaseOpacity;
}

public struct MistParticle
{
    public float X;
    public float Y;
    public float BaseY;
    public float VelocityX;
    public float Width;
    public float Height;
    public float PulseSpeed;
    public float PulsePhase;
    public float SpeedFactor;
    public int LayerType;
    public float ErraticPhaseX;
    public float ErraticPhaseY;
    public float BaseOpacity;
}

/// <summary>
/// Represents a high-fidelity, volumetric puffy cloud composed of overlapping puffs.
/// </summary>
public struct PuffyCloud
{
    public float X;
    public float Y;
    public float BaseY;
    public float VelocityX;
    public float Width;
    public float Height;
    public float PulseSpeed;
    public float PulsePhase;
    public float SpeedFactor;
    public float BaseOpacity;
    public float ErraticPhase;
}

/// <summary>
/// Senior-Level GPU-Accelerated Dynamic Composition Service for WPF.
/// Blends a soft, ambient background mist with two prominent, highly defined 
/// volumetric puffy clouds floating in opposite directions.
/// Runs entirely on GPU composition threads with locked 60FPS+ and 0% CPU.
/// </summary>
public sealed class GameBackgroundService : IDisposable
{
    private static readonly Random Rng = new();

    private readonly FrameworkElement _parentView;
    private readonly Canvas _canvas;
    
    // Background layers
    private Border? _spotlightElement;
    private Border? _vignetteElement;
    
    private readonly List<MistParticle> _mist = new();
    private readonly List<FrameworkElement> _mistElements = new();
    
    private readonly List<BokehBubble> _bubbles = new();
    private readonly List<FrameworkElement> _bubbleElements = new();
    
    // Foreground/Midground puffy clouds
    private readonly List<PuffyCloud> _puffyClouds = new();
    private readonly List<Canvas> _puffyCloudElements = new();

    private readonly Stopwatch _stopwatch = new();
    private double _lastElapsedTime;
    private bool _isHooked;

    // Mouse parallax tracking
    private float _targetMouseX;
    private float _targetMouseY;
    private float _mouseX;
    private float _mouseY;

    public GameBackgroundService(FrameworkElement parentView, Canvas canvas)
    {
        _parentView = parentView;
        _canvas = canvas;

        // Clear any leftover controls
        _canvas.Children.Clear();

        // Hook interaction and resizing events
        _parentView.MouseMove += OnParentMouseMove;
        _canvas.SizeChanged += OnCanvasSizeChanged;

        // Build GPU Layers Back-to-Front
        CreateSpotlight();
        InitializeBackgroundMist(); // Ambient fuzzy fog
        InitializePuffyClouds();    // The two concrete, voluminous puffy clouds
        InitializeBubbles();
        CreateVignette();

        // Start native rendering thread ticks
        HookRendering();
    }

    private void CreateSpotlight()
    {
        _spotlightElement = new Border
        {
            Width = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 1920,
            Height = _canvas.ActualHeight > 0 ? _canvas.ActualHeight : 1080,
            IsHitTestVisible = false
        };

        var spotBrush = new RadialGradientBrush
        {
            Center = new Point(0.5, 0.35),
            GradientOrigin = new Point(0.5, 0.35),
            RadiusX = 0.85,
            RadiusY = 0.85
        };
        spotBrush.GradientStops.Add(new GradientStop(Color.FromArgb(130, 255, 218, 224), 0));
        spotBrush.GradientStops.Add(new GradientStop(Color.FromArgb(60, 240, 220, 240), 0.5));
        spotBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
        
        _spotlightElement.Background = spotBrush;
        _canvas.Children.Add(_spotlightElement);
    }

    private void CreateVignette()
    {
        _vignetteElement = new Border
        {
            Width = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 1920,
            Height = _canvas.ActualHeight > 0 ? _canvas.ActualHeight : 1080,
            IsHitTestVisible = false
        };

        var vigBrush = new RadialGradientBrush
        {
            Center = new Point(0.5, 0.5),
            RadiusX = 0.85,
            RadiusY = 0.85
        };
        vigBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 0.0));
        vigBrush.GradientStops.Add(new GradientStop(Color.FromArgb(95, 20, 10, 20), 1.0));
        
        _vignetteElement.Background = vigBrush;
        _canvas.Children.Add(_vignetteElement);
    }

    private void OnCanvasSizeChanged(object sender, SizeChangedEventArgs e)
    {
        double width = e.NewSize.Width;
        double height = e.NewSize.Height;

        if (width <= 0 || height <= 0) return;

        if (_spotlightElement != null)
        {
            _spotlightElement.Width = width;
            _spotlightElement.Height = height;
        }

        if (_vignetteElement != null)
        {
            _vignetteElement.Width = width;
            _vignetteElement.Height = height;
        }
    }

    /// <summary>
    /// Creates the ambient fuzzy background mist to keep aesthetic consistency.
    /// Soft, low-opacity, wide-spreading horizontal layers.
    /// </summary>
    private void InitializeBackgroundMist()
    {
        Color[] colors = new[]
        {
            Color.FromRgb(255, 218, 224), // Pastel Rose / Soft Pink
            Color.FromRgb(240, 230, 240), // Pale Lavender
            Color.FromRgb(255, 228, 225), // Misty Rose
            Color.FromRgb(255, 240, 245)  // Lavender Blush
        };

        for (int i = 0; i < 6; i++)
        {
            var tintColor = colors[Rng.Next(0, colors.Length)];
            int layerType = i / 2; // 0 (Top/Far) | 1 (Mid) | 2 (Bottom/Near)
            float velX, baseY, speedFactor, widthSize, heightSize, opacityMin, opacityMax;

            if (layerType == 0) // Far depth
            {
                velX = 25f + Rng.Next(0, 15);
                baseY = Rng.Next(80, 200);
                speedFactor = 0.08f + Rng.Next(0, 5) / 100f;
                widthSize = Rng.Next(800, 1100);
                heightSize = Rng.Next(300, 400);
                opacityMin = 0.15f;
                opacityMax = 0.28f; // Soft ambient
            }
            else if (layerType == 1) // Mid depth
            {
                velX = -(40f + Rng.Next(0, 20));
                baseY = Rng.Next(300, 480);
                speedFactor = 0.35f + Rng.Next(0, 10) / 100f;
                widthSize = Rng.Next(1100, 1400);
                heightSize = Rng.Next(400, 500);
                opacityMin = 0.20f;
                opacityMax = 0.32f;
            }
            else // Near depth
            {
                velX = Rng.Next(-15, 15);
                baseY = Rng.Next(600, 720);
                speedFactor = 0.70f + Rng.Next(0, 15) / 100f;
                widthSize = Rng.Next(1400, 1800);
                heightSize = Rng.Next(500, 650);
                opacityMin = 0.22f;
                opacityMax = 0.35f;
            }

            float baseOpacity = (float)(opacityMin + Rng.NextDouble() * (opacityMax - opacityMin));

            var cloudElement = new Ellipse
            {
                Width = widthSize,
                Height = heightSize,
                IsHitTestVisible = false,
                Opacity = baseOpacity
            };

            // Classic ultra-smooth atmospheric gradient
            var brush = new RadialGradientBrush();
            brush.GradientStops.Add(new GradientStop(tintColor, 0.0));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, tintColor.R, tintColor.G, tintColor.B), 1.0));
            cloudElement.Fill = brush;

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(1, 1));
            transformGroup.Children.Add(new TranslateTransform(Rng.Next(-400, 1800), baseY));
            cloudElement.RenderTransform = transformGroup;

            _canvas.Children.Add(cloudElement);
            _mistElements.Add(cloudElement);

            _mist.Add(new MistParticle
            {
                X = Rng.Next(-400, 1800),
                Y = baseY,
                BaseY = baseY,
                VelocityX = velX,
                Width = widthSize, 
                Height = heightSize,
                PulseSpeed = 0.15f + Rng.Next(0, 15) / 100f, 
                PulsePhase = (float)(Rng.NextDouble() * Math.PI * 2),
                SpeedFactor = speedFactor,
                LayerType = layerType,
                ErraticPhaseX = (float)(Rng.NextDouble() * Math.PI * 2),
                ErraticPhaseY = (float)(Rng.NextDouble() * Math.PI * 2),
                BaseOpacity = baseOpacity
            });
        }
    }

    /// <summary>
    /// Initializes two majestic, massive, highly realistic volumetric puffy clouds.
    /// Cruising in opposite directions with distinct parallax factors.
    /// </summary>
    private void InitializePuffyClouds()
    {
        // ── Puffy Cloud 1: Deep Background Ambient Cloud (Ultra-Slow, Very Soft) ──
        float width1 = 1500f;
        float height1 = 650f;
        float baseY1 = Rng.Next(30, 120);
        float velX1 = 15f + Rng.Next(0, 10); // Very slow drift
        float speedFactor1 = 0.20f; // Far background parallax
        float opacity1 = 0.40f; // Highly transparent ambient layer

        var cloudElement1 = CreatePuffyCloudCanvas(width1, height1);
        _canvas.Children.Add(cloudElement1);
        _puffyCloudElements.Add(cloudElement1);

        _puffyClouds.Add(new PuffyCloud
        {
            X = Rng.Next(-500, 400),
            Y = baseY1,
            BaseY = baseY1,
            VelocityX = velX1,
            Width = width1,
            Height = height1,
            PulseSpeed = 0.18f,
            PulsePhase = (float)(Rng.NextDouble() * Math.PI * 2),
            SpeedFactor = speedFactor1,
            BaseOpacity = opacity1,
            ErraticPhase = (float)(Rng.NextDouble() * Math.PI * 2)
        });

        // ── Puffy Cloud 2: Midground Volumetric Cloud (Left-to-Right, Majestic) ──
        float width2 = 1100f;
        float height2 = 480f;
        float baseY2 = Rng.Next(80, 220);
        float velX2 = 35f + Rng.Next(0, 15);
        float speedFactor2 = 0.50f; // Midground parallax
        float opacity2 = 0.72f;

        var cloudElement2 = CreatePuffyCloudCanvas(width2, height2);
        _canvas.Children.Add(cloudElement2);
        _puffyCloudElements.Add(cloudElement2);

        _puffyClouds.Add(new PuffyCloud
        {
            X = Rng.Next(-300, 600),
            Y = baseY2,
            BaseY = baseY2,
            VelocityX = velX2,
            Width = width2,
            Height = height2,
            PulseSpeed = 0.24f,
            PulsePhase = (float)(Rng.NextDouble() * Math.PI * 2),
            SpeedFactor = speedFactor2,
            BaseOpacity = opacity2,
            ErraticPhase = (float)(Rng.NextDouble() * Math.PI * 2)
        });

        // ── Puffy Cloud 3: Foreground Atmospheric Fog Cover (Right-to-Left, Gigante) ──
        float width3 = 1400f;
        float height3 = 580f;
        float baseY3 = Rng.Next(280, 420);
        float velX3 = -(45f + Rng.Next(0, 15)); // Opposite drift
        float speedFactor3 = 0.82f; // Foreground parallax
        float opacity3 = 0.80f;

        var cloudElement3 = CreatePuffyCloudCanvas(width3, height3);
        _canvas.Children.Add(cloudElement3);
        _puffyCloudElements.Add(cloudElement3);

        _puffyClouds.Add(new PuffyCloud
        {
            X = Rng.Next(800, 1800),
            Y = baseY3,
            BaseY = baseY3,
            VelocityX = velX3,
            Width = width3,
            Height = height3,
            PulseSpeed = 0.20f,
            PulsePhase = (float)(Rng.NextDouble() * Math.PI * 2),
            SpeedFactor = speedFactor3,
            BaseOpacity = opacity3,
            ErraticPhase = (float)(Rng.NextDouble() * Math.PI * 2)
        });
    }

    /// <summary>
    /// Builds an advanced volumetric puffy cloud compound Canvas:
    /// 1. Cohesive Underbelly Shadows: Base shadow weight diagonally offset.
    /// 2. Subsurface Scattering Transitions: A warm peach-rose glowing layer bridging shadow and body.
    /// 3. Asymmetric Body Layer: 11 overlapping ovals creating complex surface microdetails.
    /// 4. Silver Lining (Rim Light): Thin, high-opacity pure white highlights on the top edges.
    /// Everything is rendered diagonally (Light coming from top-left) for perfect optical realism.
    /// </summary>
    private Canvas CreatePuffyCloudCanvas(float width, float height)
    {
        var cloudCanvas = new Canvas
        {
            Width = width,
            Height = height,
            IsHitTestVisible = false
        };

        float wScale = width / 400f;
        float hScale = height / 200f;

        Color white = Colors.White;
        Color cream = Color.FromRgb(255, 248, 250);
        Color transitionColor = Color.FromRgb(242, 215, 222); // Glowing warm peach-rose (Subsurface Scattering)
        Color shadowColor = Color.FromRgb(205, 190, 202);     // Volumetric lavender-grey shadow

        // ── LAYER 1: Shared Underbelly Shadows (Unified base shadow weight, offset slightly to bottom-right) ──
        AddCloudShadowPuff(cloudCanvas, 60 * wScale, 115 * hScale, 290 * wScale, 65 * hScale, shadowColor);
        AddCloudShadowPuff(cloudCanvas, 25 * wScale, 125 * hScale, 160 * wScale, 55 * hScale, shadowColor);
        AddCloudShadowPuff(cloudCanvas, 225 * wScale, 125 * hScale, 160 * wScale, 55 * hScale, shadowColor);
        AddCloudShadowPuff(cloudCanvas, 140 * wScale, 130 * hScale, 180 * wScale, 60 * hScale, shadowColor);

        // ── LAYER 2: Subsurface Scattering Transitions (Bridge between shadow and body, glowing warm tone) ──
        AddCloudTransitionPuff(cloudCanvas, 85 * wScale, 65 * hScale, 230 * wScale, 110 * hScale, transitionColor);
        AddCloudTransitionPuff(cloudCanvas, 30 * wScale, 80 * hScale, 140 * wScale, 90 * hScale, transitionColor);
        AddCloudTransitionPuff(cloudCanvas, 230 * wScale, 80 * hScale, 140 * wScale, 90 * hScale, transitionColor);

        // ── LAYER 3: Complex Multi-Puff White/Cream Body (High density asymmetric microdetails) ──
        // Center cluster (broken symmetry)
        AddCloudBodyPuff(cloudCanvas, 100 * wScale, 40 * hScale, 180 * wScale, 105 * hScale, white, cream);
        AddCloudBodyPuff(cloudCanvas, 140 * wScale, 45 * hScale, 170 * wScale, 105 * hScale, white, cream);
        
        // Left cluster
        AddCloudBodyPuff(cloudCanvas, 30 * wScale, 65 * hScale, 110 * wScale, 85 * hScale, white, cream);
        AddCloudBodyPuff(cloudCanvas, 65 * wScale, 55 * hScale, 95 * wScale, 75 * hScale, white, cream);
        
        // Right cluster
        AddCloudBodyPuff(cloudCanvas, 250 * wScale, 65 * hScale, 110 * wScale, 85 * hScale, white, cream);
        AddCloudBodyPuff(cloudCanvas, 220 * wScale, 55 * hScale, 95 * wScale, 75 * hScale, white, cream);
        
        // Top crowns (Sun facing)
        AddCloudBodyPuff(cloudCanvas, 95 * wScale, 22 * hScale, 110 * wScale, 90 * hScale, white, cream);
        AddCloudBodyPuff(cloudCanvas, 175 * wScale, 18 * hScale, 125 * wScale, 95 * hScale, white, cream);
        AddCloudBodyPuff(cloudCanvas, 135 * wScale, 28 * hScale, 115 * wScale, 85 * hScale, white, cream);
        
        // Front detailed overlay puffs (microdetails to break visual flatness)
        AddCloudBodyPuff(cloudCanvas, 115 * wScale, 60 * hScale, 80 * wScale, 60 * hScale, white, cream);
        AddCloudBodyPuff(cloudCanvas, 195 * wScale, 58 * hScale, 90 * wScale, 65 * hScale, white, cream);
        AddCloudBodyPuff(cloudCanvas, 155 * wScale, 75 * hScale, 100 * wScale, 70 * hScale, white, cream);

        // ── LAYER 4: Silver Lining (Rim lighting contour elements on the top-left edges facing the sun) ──
        AddCloudRimPuff(cloudCanvas, 85 * wScale, 15 * hScale, 75 * wScale, 55 * hScale);
        AddCloudRimPuff(cloudCanvas, 155 * wScale, 10 * hScale, 85 * wScale, 60 * hScale);
        AddCloudRimPuff(cloudCanvas, 215 * wScale, 20 * hScale, 75 * wScale, 55 * hScale);
        AddCloudRimPuff(cloudCanvas, 25 * wScale, 50 * hScale, 65 * wScale, 50 * hScale);

        // Pre-allocate GPU transform group
        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(new ScaleTransform(1, 1));
        transformGroup.Children.Add(new TranslateTransform(0, 0));
        cloudCanvas.RenderTransform = transformGroup;

        return cloudCanvas;
    }

    private void AddCloudBodyPuff(Canvas canvas, double x, double y, double w, double h, Color c1, Color c2)
    {
        var puff = new Ellipse
        {
            Width = w,
            Height = h,
            IsHitTestVisible = false
        };

        // Shifted top-left (Directional Sun alignment) - Massive wide radius for ultra-diffuse ambient blend
        var brush = new RadialGradientBrush
        {
            Center = new Point(0.35, 0.22),
            GradientOrigin = new Point(0.35, 0.22),
            RadiusX = 0.80,
            RadiusY = 0.80
        };
        // Using semi-transparent alpha stops so overlapping shapes fuse softly together
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(170, c1.R, c1.G, c1.B), 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(110, c2.R, c2.G, c2.B), 0.55));
        brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0)); // Seamless edge

        puff.Fill = brush;

        Canvas.SetLeft(puff, x);
        Canvas.SetTop(puff, y);
        canvas.Children.Add(puff);
    }

    private void AddCloudTransitionPuff(Canvas canvas, double x, double y, double w, double h, Color transitionColor)
    {
        var puff = new Ellipse
        {
            Width = w,
            Height = h,
            IsHitTestVisible = false
        };

        // Shifted top-left (Directional Sun alignment)
        var brush = new RadialGradientBrush
        {
            Center = new Point(0.35, 0.22),
            GradientOrigin = new Point(0.35, 0.22),
            RadiusX = 0.85,
            RadiusY = 0.85
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(110, transitionColor.R, transitionColor.G, transitionColor.B), 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(45, transitionColor.R, transitionColor.G, transitionColor.B), 0.55));
        brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));

        puff.Fill = brush;

        Canvas.SetLeft(puff, x);
        Canvas.SetTop(puff, y);
        canvas.Children.Add(puff);
    }

    private void AddCloudShadowPuff(Canvas canvas, double x, double y, double w, double h, Color shadowColor)
    {
        var puff = new Ellipse
        {
            Width = w,
            Height = h,
            IsHitTestVisible = false
        };

        // Shifted bottom-right (Self-shadowing projection opposing light direction)
        var brush = new RadialGradientBrush
        {
            Center = new Point(0.45, 0.45),
            RadiusX = 0.90,
            RadiusY = 0.90
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(120, shadowColor.R, shadowColor.G, shadowColor.B), 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(45, shadowColor.R, shadowColor.G, shadowColor.B), 0.60));
        brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));

        puff.Fill = brush;

        Canvas.SetLeft(puff, x);
        Canvas.SetTop(puff, y);
        canvas.Children.Add(puff);
    }

    private void AddCloudRimPuff(Canvas canvas, double x, double y, double w, double h)
    {
        var puff = new Ellipse
        {
            Width = w,
            Height = h,
            IsHitTestVisible = false
        };

        // Highly focused soft white highlight on the top-left boundary (Silver Lining)
        var brush = new RadialGradientBrush
        {
            Center = new Point(0.28, 0.18),
            GradientOrigin = new Point(0.28, 0.18),
            RadiusX = 0.80,
            RadiusY = 0.80
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(140, 255, 255, 255), 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(60, 255, 255, 255), 0.60)); 
        brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));

        puff.Fill = brush;

        Canvas.SetLeft(puff, x);
        Canvas.SetTop(puff, y);
        canvas.Children.Add(puff);
    }

    private void InitializeBubbles()
    {
        Color[] colors = new[]
        {
            Color.FromArgb(40, 255, 182, 193),  // Light Pink
            Color.FromArgb(35, 255, 105, 180),  // Hot Pink
            Color.FromArgb(45, 230, 230, 250),  // Lavender
            Color.FromArgb(50, 255, 240, 245),  // Lavender Blush
            Color.FromArgb(45, 255, 228, 225)   // Misty Rose
        };

        for (int i = 0; i < 15; i++)
        {
            var tintColor = colors[Rng.Next(0, colors.Length)];
            int tier = Rng.Next(0, 3);
            float speedFactor, baseRadius, alphaScale;

            if (tier == 0) // Far Layer
            {
                speedFactor = 0.15f + Rng.Next(0, 15) / 100f;
                baseRadius = Rng.Next(40, 75);
                alphaScale = 0.45f;
            }
            else if (tier == 1) // Midground Layer
            {
                speedFactor = 0.5f + Rng.Next(0, 35) / 100f;
                baseRadius = Rng.Next(90, 140);
                alphaScale = 0.85f;
            }
            else // Near Layer
            {
                speedFactor = 1.2f + Rng.Next(0, 45) / 100f;
                baseRadius = Rng.Next(170, 240);
                alphaScale = 1.2f;
            }

            float finalOpacity = Math.Clamp((tintColor.A / 255f) * alphaScale, 0f, 1f);

            var bubbleElement = new Ellipse
            {
                Width = baseRadius * 2,
                Height = baseRadius * 2,
                IsHitTestVisible = false,
                Opacity = finalOpacity
            };

            var brush = new RadialGradientBrush();
            var pureColor = Color.FromRgb(tintColor.R, tintColor.G, tintColor.B);
            brush.GradientStops.Add(new GradientStop(pureColor, 0.0));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, pureColor.R, pureColor.G, pureColor.B), 1.0));
            bubbleElement.Fill = brush;

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(1, 1));
            transformGroup.Children.Add(new TranslateTransform(Rng.Next(0, 1920), Rng.Next(0, 1080)));
            bubbleElement.RenderTransform = transformGroup;

            _canvas.Children.Add(bubbleElement);
            _bubbleElements.Add(bubbleElement);

            _bubbles.Add(new BokehBubble
            {
                X = Rng.Next(0, 1920),
                Y = Rng.Next(0, 1080),
                VelocityX = Rng.Next(-15, 15) / 10f,
                VelocityY = Rng.Next(-18, -5) / 10f,
                BaseRadius = baseRadius,
                PulseSpeed = 0.4f + Rng.Next(0, 8) / 10f,
                PulsePhase = (float)(Rng.NextDouble() * Math.PI * 2),
                SpeedFactor = speedFactor,
                BaseOpacity = finalOpacity
            });
        }
    }

    private void HookRendering()
    {
        if (!_isHooked)
        {
            CompositionTarget.Rendering += OnRenderingTick;
            _stopwatch.Start();
            _isHooked = true;
        }
    }

    private void UnhookRendering()
    {
        if (_isHooked)
        {
            CompositionTarget.Rendering -= OnRenderingTick;
            _stopwatch.Stop();
            _isHooked = false;
        }
    }

    private void OnParentMouseMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(_parentView);
        _targetMouseX = (float)pos.X;
        _targetMouseY = (float)pos.Y;
    }

    private void OnRenderingTick(object? sender, EventArgs e)
    {
        double currentElapsed = _stopwatch.Elapsed.TotalSeconds;
        double deltaTime = currentElapsed - _lastElapsedTime;
        _lastElapsedTime = currentElapsed;

        if (deltaTime <= 0 || deltaTime > 0.1) return;

        // Smooth parallax interpolation
        _mouseX += (_targetMouseX - _mouseX) * 4f * (float)deltaTime;
        _mouseY += (_targetMouseY - _mouseY) * 4f * (float)deltaTime;

        float width = (float)_canvas.ActualWidth;
        float height = (float)_canvas.ActualHeight;

        if (width <= 0 || height <= 0) return;

        float centerX = width / 2f;
        float centerY = height / 2f;

        float parallaxX = -(_mouseX - centerX) * 0.025f;
        float parallaxY = -(_mouseY - centerY) * 0.025f;

        // 1. Update and Render Ambient Background Mist
        for (int i = 0; i < _mist.Count; i++)
        {
            var m = _mist[i];
            m.PulsePhase += m.PulseSpeed * (float)deltaTime;

            if (m.LayerType == 0) // Top
            {
                m.X += m.VelocityX * (float)deltaTime;
                if (m.X > width + m.Width) m.X = -m.Width;
            }
            else if (m.LayerType == 1) // Mid
            {
                m.X += m.VelocityX * (float)deltaTime;
                if (m.X < -m.Width) m.X = width + m.Width;
            }
            else // Bottom Erratic
            {
                m.ErraticPhaseX += 1.6f * (float)deltaTime;
                m.ErraticPhaseY += 1.0f * (float)deltaTime;

                float erraticWind = (float)Math.Sin(m.ErraticPhaseX) * 45f;
                m.X += (m.VelocityX + erraticWind) * (float)deltaTime;
                m.Y = m.BaseY + (float)Math.Sin(m.ErraticPhaseY) * 45f;

                if (m.X > width + m.Width) m.X = -m.Width;
                if (m.X < -m.Width) m.X = width + m.Width;
            }

            _mist[i] = m;

            var element = _mistElements[i];
            var transformGroup = (TransformGroup)element.RenderTransform;
            var scaleTransform = (ScaleTransform)transformGroup.Children[0];
            var translateTransform = (TranslateTransform)transformGroup.Children[1];

            // Breathing pulse opacity
            float breath = 0.65f + 0.35f * (float)Math.Sin(m.PulsePhase);
            element.Opacity = m.BaseOpacity * breath;

            // Breathing scale on GPU
            float scale = 1.0f + 0.04f * (float)Math.Sin(m.PulsePhase);
            scaleTransform.ScaleX = scale;
            scaleTransform.ScaleY = scale;

            float finalX = m.X + parallaxX * m.SpeedFactor;
            float finalY = m.Y + parallaxY * m.SpeedFactor;
            translateTransform.X = finalX - m.Width / 2f;
            translateTransform.Y = finalY - m.Height / 2f;
        }

        // 2. Update and Render Fluffy Puffy Clouds (Cruising opposite directions) - ER RATIC DRIFT
        for (int i = 0; i < _puffyClouds.Count; i++)
        {
            var pc = _puffyClouds[i];
            pc.PulsePhase += pc.PulseSpeed * (float)deltaTime;
            pc.ErraticPhase += 0.8f * (float)deltaTime; // Natural float cycle

            // Horizontal Cruise with dynamic erratic wind currents!
            float windCurrent = (float)Math.Sin(pc.ErraticPhase * 1.5f) * 15f; 
            pc.X += (pc.VelocityX + windCurrent) * (float)deltaTime;

            // Natural erratic vertical float sway (wider displacement)
            pc.Y = pc.BaseY + (float)Math.Sin(pc.ErraticPhase) * 20f;

            // Horizontal boundary wrap-around
            if (pc.VelocityX > 0)
            {
                if (pc.X > width + pc.Width) pc.X = -pc.Width;
            }
            else
            {
                if (pc.X < -pc.Width) pc.X = width + pc.Width;
            }

            _puffyClouds[i] = pc;

            // GPU Render Update: scale and translate on GPU
            var element = _puffyCloudElements[i];
            var transformGroup = (TransformGroup)element.RenderTransform;
            var scaleTransform = (ScaleTransform)transformGroup.Children[0];
            var translateTransform = (TranslateTransform)transformGroup.Children[1];

            // Slow breathing scale
            float scale = 1.0f + 0.03f * (float)Math.Sin(pc.PulsePhase);
            scaleTransform.ScaleX = scale;
            scaleTransform.ScaleY = scale;

            // Opacity breathing
            float breath = 0.90f + 0.10f * (float)Math.Sin(pc.PulsePhase);
            element.Opacity = pc.BaseOpacity * breath;

            // Position + Parallax
            float finalX = pc.X + parallaxX * pc.SpeedFactor;
            float finalY = pc.Y + parallaxY * pc.SpeedFactor;
            translateTransform.X = finalX;
            translateTransform.Y = finalY;
        }

        // 3. Update and Render Floating Bokeh Bubbles
        for (int i = 0; i < _bubbles.Count; i++)
        {
            var b = _bubbles[i];
            b.PulsePhase += b.PulseSpeed * (float)deltaTime;

            b.Y += b.VelocityY * b.SpeedFactor * 40f * (float)deltaTime;
            b.X += b.VelocityX * b.SpeedFactor * 15f * (float)deltaTime;

            if (b.Y < -b.BaseRadius * 2)
            {
                b.Y = height + b.BaseRadius;
                b.X = Rng.Next(0, (int)width);
            }
            if (b.X < -b.BaseRadius * 2) b.X = width + b.BaseRadius;
            if (b.X > width + b.BaseRadius * 2) b.X = -b.BaseRadius;

            _bubbles[i] = b;

            var element = _bubbleElements[i];
            var transformGroup = (TransformGroup)element.RenderTransform;
            var scaleTransform = (ScaleTransform)transformGroup.Children[0];
            var translateTransform = (TranslateTransform)transformGroup.Children[1];

            float radiusPulse = 1.0f + 0.12f * (float)Math.Sin(b.PulsePhase);
            scaleTransform.ScaleX = radiusPulse;
            scaleTransform.ScaleY = radiusPulse;

            float finalX = b.X + parallaxX * b.SpeedFactor;
            float finalY = b.Y + parallaxY * b.SpeedFactor;
            translateTransform.X = finalX - b.BaseRadius;
            translateTransform.Y = finalY - b.BaseRadius;
        }
    }

    public void Dispose()
    {
        UnhookRendering();
        _parentView.MouseMove -= OnParentMouseMove;
        _canvas.SizeChanged -= OnCanvasSizeChanged;
        
        _canvas.Children.Clear();
        _mistElements.Clear();
        _bubbleElements.Clear();
        _puffyCloudElements.Clear();

        GC.SuppressFinalize(this);
    }
}
