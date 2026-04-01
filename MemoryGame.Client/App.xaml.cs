using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MemoryGame.Client.Localization;
using MemoryGame.Client.Services;
using MemoryGame.Client.ViewModels;
using MemoryGame.Client.ViewModels.MainMenu;
using MemoryGame.Client.ViewModels.Session;
using MemoryGame.Client.ViewModels.Settings;

namespace MemoryGame.Client;

/// <summary>
/// Application entry point. Configures dependency injection and starts the shell.
/// </summary>
public partial class App : Application
{
    private const string ApiBaseUrl = "http://localhost:5059/";
    private const string HubUrl = "http://localhost:5059/hub/lobby";

    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Services (singleton — shared state across the app)
        services.AddSingleton<ClientSettings>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<MusicService>();
        services.AddSingleton(sp => new HubService(
            sp.GetRequiredService<ISessionService>(), HubUrl));

        // HTTP client
        services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri(ApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        // ViewModels (transient — new instance each navigation)
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<TitleScreenViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<GuestLoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<VerifyEmailViewModel>();
        services.AddTransient<SetupProfileViewModel>();
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Main window
        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settings = _serviceProvider.GetRequiredService<ClientSettings>();
        LocalizationManager.Instance.SetCulture(settings.LanguageCode);

        _serviceProvider.GetRequiredService<MusicService>();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>();

        var navigation = _serviceProvider.GetRequiredService<INavigationService>();
        navigation.NavigateTo<TitleScreenViewModel>();

        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _serviceProvider.GetRequiredService<MusicService>().Dispose();
        var hub = _serviceProvider.GetRequiredService<HubService>();
        await hub.DisposeAsync();
        await _serviceProvider.DisposeAsync();
        base.OnExit(e);
    }
}
