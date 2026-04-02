using System.Windows;
using System.Windows.Controls;
using MemoryGame.Client.Views.Controls.Effects;

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
}
