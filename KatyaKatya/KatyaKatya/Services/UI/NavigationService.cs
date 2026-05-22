using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using KatyaKatya.Services.Interfaces;

namespace KatyaKatya.Services.UI;

/// <summary>
/// ViewModel-driven navigation with history stack.
/// </summary>
public class NavigationService : ObservableObject, INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<ObservableObject> _history = new();
    private ObservableObject? _currentViewModel;
    private bool _isAnimatedTransition;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ObservableObject? CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public bool IsAnimatedTransition
    {
        get => _isAnimatedTransition;
        private set => SetProperty(ref _isAnimatedTransition, value);
    }

    public bool CanGoBack => _history.Count > 0;

    public void NavigateTo<TViewModel>() where TViewModel : ObservableObject
    {
        if (CurrentViewModel is not null)
            _history.Push(CurrentViewModel);

        IsAnimatedTransition = false;
        CurrentViewModel = _serviceProvider.GetRequiredService<TViewModel>();
    }

    public void NavigateTo<TViewModel>(Action<TViewModel> setup) where TViewModel : ObservableObject
    {
        if (CurrentViewModel is not null)
            _history.Push(CurrentViewModel);

        IsAnimatedTransition = false;
        var vm = _serviceProvider.GetRequiredService<TViewModel>();
        setup(vm);
        CurrentViewModel = vm;
    }

    public void NavigateToRoot<TViewModel>() where TViewModel : ObservableObject
    {
        _history.Clear();
        IsAnimatedTransition = false;
        CurrentViewModel = _serviceProvider.GetRequiredService<TViewModel>();
    }

    public void NavigateToRoot<TViewModel>(Action<TViewModel> setup) where TViewModel : ObservableObject
    {
        _history.Clear();
        IsAnimatedTransition = false;
        var vm = _serviceProvider.GetRequiredService<TViewModel>();
        setup(vm);
        CurrentViewModel = vm;
    }

    public void NavigateToRootWithFade<TViewModel>() where TViewModel : ObservableObject
    {
        _history.Clear();
        IsAnimatedTransition = true;
        CurrentViewModel = _serviceProvider.GetRequiredService<TViewModel>();
    }

    public void GoBack()
    {
        if (_history.Count > 0)
        {
            IsAnimatedTransition = false;
            CurrentViewModel = _history.Pop();
        }
    }
}
