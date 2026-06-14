using KatyaKatya.Services.Core;

namespace KatyaKatya.Engine.Settings;

public sealed class GraphicsSettingsService : IGraphicsSettingsService
{
    private readonly ClientSettings _settings;

    public GraphicsSettingsService(ClientSettings settings)
    {
        _settings = settings;
    }

    public GraphicsPreset Preset
    {
        get => _settings.GraphicsPreset;
        set
        {
            _settings.GraphicsPreset = value;
            ApplyPresetDefaults(value);
        }
    }

    public bool EnableAnimatedBackground
    {
        get => _settings.EnableAnimatedBackground;
        set => _settings.EnableAnimatedBackground = value;
    }

    public bool EnableParticles
    {
        get => _settings.EnableParticles;
        set => _settings.EnableParticles = value;
    }

    public bool EnableGlassMotion
    {
        get => _settings.EnableGlassMotion;
        set => _settings.EnableGlassMotion = value;
    }

    public bool EnableUiSfx
    {
        get => _settings.EnableUiSfx;
        set => _settings.EnableUiSfx = value;
    }

    public int TargetFps => Preset == GraphicsPreset.Battery ? 30 : 60;

    public float BackgroundDensity => Preset switch
    {
        GraphicsPreset.Ultra => 1.0f,
        GraphicsPreset.Normal => 0.75f,
        GraphicsPreset.Battery => 0.35f,
        _ => 0.75f
    };

    private void ApplyPresetDefaults(GraphicsPreset preset)
    {
        switch (preset)
        {
            case GraphicsPreset.Ultra:
                EnableAnimatedBackground = true;
                EnableParticles = true;
                EnableGlassMotion = true;
                break;
            case GraphicsPreset.Normal:
                EnableAnimatedBackground = true;
                EnableParticles = true;
                EnableGlassMotion = true;
                break;
            case GraphicsPreset.Battery:
                EnableAnimatedBackground = true;
                EnableParticles = false;
                EnableGlassMotion = false;
                break;
        }
    }
}

