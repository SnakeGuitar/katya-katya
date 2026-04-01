using CommunityToolkit.Mvvm.ComponentModel;
using MemoryGame.Client.ViewModels.Session;

namespace MemoryGame.Client.Services;

/// <summary>
/// Resolves view models from DI and manages a history stack for back-navigation.
/// </summary>
public partial class NavigationService : ObservableObject, INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<ObservableObject> _history = new();

    [ObservableProperty]
    private ObservableObject? _currentViewModel;

    public bool CanGoBack => _history.Count > 0;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>() where TViewModel : ObservableObject
    {
        var viewModel = Resolve<TViewModel>();
        Push(viewModel);
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(Action<TViewModel> setup) where TViewModel : ObservableObject
    {
        var viewModel = Resolve<TViewModel>();
        setup(viewModel);
        Push(viewModel);
    }

    /// <inheritdoc/>
    public void NavigateToRoot<TViewModel>() where TViewModel : ObservableObject
    {
        _history.Clear();
        OnPropertyChanged(nameof(CanGoBack));
        CurrentViewModel = Resolve<TViewModel>();
    }

    /// <inheritdoc/>
    public void GoBack()
    {
        if (_history.TryPop(out var previous))
        {
            OnPropertyChanged(nameof(CanGoBack));
            CurrentViewModel = previous;
        }
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private TViewModel Resolve<TViewModel>() where TViewModel : ObservableObject
        => (TViewModel)_serviceProvider.GetService(typeof(TViewModel))!;

    /// <summary>
    /// Pushes the current view model onto the history stack (if there is one),
    /// then sets the new view model as current.
    /// </summary>
    private void Push(ObservableObject next)
    {
        if (CurrentViewModel is not null)
        {
            _history.Push(CurrentViewModel);
            OnPropertyChanged(nameof(CanGoBack));
        }

        CurrentViewModel = next;
    }
}
