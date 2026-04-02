using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MemoryGame.Client.Views.Controls.Effects;

/// <summary>
/// Falling cherry-blossom petal effect.
/// </summary>
/// <remarks>
/// Each petal is a semi-transparent ellipse that falls with ease-in gravity,
/// sways sinusoidally, spins, and fades out over the last 30 % of its fall.
/// All parameters (size, speed, sway, opacity) are randomized per petal.
/// </remarks>
public sealed class PetalParticleSystem : ParticleSystem
{
    /// <summary>Rose-palette colors sampled at random for each petal.</summary>
    private static readonly Color[] Colors =
    [
        Color.FromArgb(210, 255, 200, 215),
        Color.FromArgb(190, 255, 220, 235),
        Color.FromArgb(220, 240, 175, 200),
        Color.FromArgb(180, 255, 245, 248),
        Color.FromArgb(200, 255, 170, 190),
    ];

    /// <summary>
    /// Spawns one petal at a random position along the top edge of <paramref name="canvas"/>.
    /// </summary>
    /// <param name="canvas">Target canvas.</param>
    /// <param name="canvasW">Canvas width in device-independent pixels.</param>
    /// <param name="canvasH">Canvas height in device-independent pixels.</param>
    /// <remarks>
    /// Randomized ranges:
    /// width 14–30 px · height 1.2×–1.8× width · fall 5–9 s ·
    /// sway 30–110 px · opacity 0.55–0.85 · spin ±360°
    /// </remarks>
    protected override void SpawnParticle(Canvas canvas, double canvasW, double canvasH)
    {
        double w         = Rng.NextDouble() * 16 + 14;
        double h         = w * (Rng.NextDouble() * 0.6 + 1.2);
        double startX    = Rng.NextDouble() * canvasW;
        double fallSecs  = Rng.NextDouble() * 4 + 5;
        double swayAmp   = Rng.NextDouble() * 80 + 30;
        double initAngle = Rng.NextDouble() * 360;
        double spin      = Rng.NextDouble() > 0.5 ? 360 : -360;
        double opacity   = Rng.NextDouble() * 0.3 + 0.55;

        var petal = new Ellipse
        {
            Width  = w,
            Height = h,
            Fill   = new SolidColorBrush(Colors[Rng.Next(Colors.Length)]),
            Opacity = opacity,
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new RotateTransform(initAngle),
        };

        Canvas.SetLeft(petal, startX);
        Canvas.SetTop(petal, -h);
        canvas.Children.Add(petal);

        var duration = TimeSpan.FromSeconds(fallSecs);

        // Fall — ease-in simulates gravity
        petal.BeginAnimation(Canvas.TopProperty,
            new DoubleAnimation(-h, canvasH + h, duration)
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn }
            });

        // Sinusoidal horizontal sway
        double dir  = Rng.NextDouble() > 0.5 ? 1 : -1;
        double midX = startX + dir * swayAmp;
        double endX = startX - dir * swayAmp * 0.5;
        var sway = new DoubleAnimationUsingKeyFrames { Duration = duration };
        sway.KeyFrames.Add(new LinearDoubleKeyFrame(startX, KeyTime.FromPercent(0)));
        sway.KeyFrames.Add(new EasingDoubleKeyFrame(midX, KeyTime.FromPercent(0.45))
            { EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } });
        sway.KeyFrames.Add(new EasingDoubleKeyFrame(endX, KeyTime.FromPercent(1))
            { EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } });
        petal.BeginAnimation(Canvas.LeftProperty, sway);

        // Spin
        ((RotateTransform)petal.RenderTransform).BeginAnimation(
            RotateTransform.AngleProperty,
            new DoubleAnimation(initAngle, initAngle + spin, duration));

        // Fade out during the last 30 % of the fall
        petal.BeginAnimation(UIElement.OpacityProperty,
            new DoubleAnimation(opacity, 0, TimeSpan.FromSeconds(fallSecs * 0.3))
            {
                BeginTime = TimeSpan.FromSeconds(fallSecs * 0.7)
            });

        ScheduleRemoval(canvas, petal, duration + TimeSpan.FromMilliseconds(300));
    }
}
