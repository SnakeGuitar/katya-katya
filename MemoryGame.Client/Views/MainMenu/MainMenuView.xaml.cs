using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using MemoryGame.Client.Engine.Particles;

namespace MemoryGame.Client.Views.MainMenu;

public partial class MainMenuView : UserControl
{
    private readonly PetalParticleSystem _petals = new() { MaxParticles = 22 };

    public MainMenuView()
    {
        InitializeComponent();
        Loaded   += (_, _) => _petals.Attach(PetalCanvas);
        Unloaded += (_, _) => _petals.Detach();
    }

    private void ButtonPanelBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is Border border)
        {
            Point pos = e.GetPosition(border);
            GlowBrush.Center = pos;
            GlowBrush.GradientOrigin = pos;
        }
    }

    private void ButtonPanelBorder_MouseEnter(object sender, MouseEventArgs e)
    {
        var animation = new DoubleAnimation
        {
            To = 1.0,
            Duration = TimeSpan.FromSeconds(0.3),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        GlowOverlay.BeginAnimation(UIElement.OpacityProperty, animation);
    }

    private void ButtonPanelBorder_MouseLeave(object sender, MouseEventArgs e)
    {
        var animation = new DoubleAnimation
        {
            To = 0.0,
            Duration = TimeSpan.FromSeconds(0.4),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        GlowOverlay.BeginAnimation(UIElement.OpacityProperty, animation);
    }
}
