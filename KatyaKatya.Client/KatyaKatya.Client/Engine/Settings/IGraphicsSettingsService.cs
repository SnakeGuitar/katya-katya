namespace KatyaKatya.Engine.Settings;

public interface IGraphicsSettingsService
{
    GraphicsPreset Preset { get; set; }
    bool EnableAnimatedBackground { get; set; }
    bool EnableParticles { get; set; }
    bool EnableGlassMotion { get; set; }
    bool EnableUiSfx { get; set; }
    int TargetFps { get; }
    float BackgroundDensity { get; }
}

