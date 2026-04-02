using MemoryGame.Client.Views.Controls.Effects;
using System.Windows.Controls;

namespace MemoryGame.Client.Views.Session;

public partial class TitleScreenView : UserControl
{
    private readonly PetalParticleSystem _petals = new() { MaxParticles = 22 };
    public TitleScreenView()
    {
        InitializeComponent();
        Loaded += (_, _) => _petals.Attach(PetalCanvas);
        Unloaded += (_, _) => _petals.Detach();
    }
}
