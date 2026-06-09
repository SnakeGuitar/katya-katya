using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using KatyaKatya.Services.Core;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.Services.Media;
using KatyaKatya.Services.Network;
using KatyaKatya.Services.UI;
using KatyaKatya.ViewModels;
using KatyaKatya.ViewModels.Session;
using KatyaKatya.ViewModels.Common;
using KatyaKatya.ViewModels.MainMenu;
using KatyaKatya.ViewModels.Settings;
using KatyaKatya.ViewModels.Profile;
using KatyaKatya.ViewModels.Social;
using KatyaKatya.ViewModels.Gallery;
using KatyaKatya.ViewModels.Lobby;
using KatyaKatya.ViewModels.SinglePlayer;
using KatyaKatya.Views;
using KatyaKatya.Helpers;
using KatyaKatya.Localization;

namespace KatyaKatya;

public partial class App : Application
{
    private const string ApiBaseUrl = "http://127.0.0.1:5000/";
    private const string HubUrl = "http://127.0.0.1:5000/hub/lobby";

    private ServiceProvider _serviceProvider = null!;

    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        Services = _serviceProvider;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
            };

            var navigation = _serviceProvider.GetRequiredService<INavigationService>();
            var settings = _serviceProvider.GetRequiredService<ClientSettings>();
            var theme = _serviceProvider.GetRequiredService<IThemeService>();
            LocalizationManager.Instance.SetCulture(settings.LanguageCode);
            theme.ApplyTheme(settings.ThemeName);
            _serviceProvider.GetRequiredService<IMusicService>();
            navigation.NavigateTo<SplashScreenViewModel>();

            desktop.ShutdownRequested += OnShutdown;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new MainView
            {
                DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
            };

            var navigation = _serviceProvider.GetRequiredService<INavigationService>();
            var settings = _serviceProvider.GetRequiredService<ClientSettings>();
            var theme = _serviceProvider.GetRequiredService<IThemeService>();
            LocalizationManager.Instance.SetCulture(settings.LanguageCode);
            theme.ApplyTheme(settings.ThemeName);
            _serviceProvider.GetRequiredService<IMusicService>();
            navigation.NavigateTo<SplashScreenViewModel>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Services (singleton)
        services.AddSingleton<IMusicService, MusicService>();
        services.AddSingleton<ClientSettings>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<IFilePickerService, FilePickerService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IThemeAssetService, ThemeAssetService>();
        services.AddSingleton<IProfileService, ProfileService>();
        services.AddSingleton<ILobbyService, LobbyService>();
        services.AddSingleton<IChatService, ChatService>();
        services.AddSingleton<IGameService, GameService>();
        services.AddSingleton(sp => new HubService(
            sp.GetRequiredService<ISessionService>(), HubUrl));

        // HTTP client
        services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri(ApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        // Helpers
        services.AddTransient<ProfileLoader>();

        // ViewModels (transient)
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<SplashScreenViewModel>();
        services.AddTransient<TitleScreenViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<GuestLoginViewModel>();
        services.AddTransient<VerifyEmailViewModel>();
        services.AddTransient<SetupProfileViewModel>();
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<MoreMenuViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<EditProfileViewModel>();
        services.AddTransient<FriendsViewModel>();
        services.AddTransient<GalleryViewModel>();
        services.AddTransient<LobbyMenuViewModel>();
        services.AddTransient<HostLobbyViewModel>();
        services.AddTransient<LobbyViewModel>();
        services.AddTransient<GameBoardViewModel>();
        services.AddTransient<SinglePlayerMenuViewModel>();
        services.AddTransient<SinglePlayerGameViewModel>();
    }

    private async void OnShutdown(object? sender, ShutdownRequestedEventArgs e)
    {
        var hub = _serviceProvider.GetRequiredService<HubService>();
        await hub.DisposeAsync();
        await _serviceProvider.DisposeAsync();
    }
}
