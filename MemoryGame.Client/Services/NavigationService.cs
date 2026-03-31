using CommunityToolkit.Mvvm.ComponentModel;

namespace MemoryGame.Client.Services;

/// <summary>
/// Resolves view models from DI and sets the current one for the shell to display.
/// </summary>
public partial class NavigationService : ObservableObject, INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableObject? _currentViewModel;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ObservableObject
    {
        var viewModel = (TViewModel)_serviceProvider.GetService(typeof(TViewModel))!;
        CurrentViewModel = viewModel;
    }

    public void NavigateTo<TViewModel>(Action<TViewModel> setup) where TViewModel : ObservableObject
    {
        var viewModel = (TViewModel)_serviceProvider.GetService(typeof(TViewModel))!;
        setup(viewModel);
        CurrentViewModel = viewModel;
    }
}
