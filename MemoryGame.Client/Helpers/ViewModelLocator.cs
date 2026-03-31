using System.Windows;

namespace MemoryGame.Client.Helpers;

/// <summary>
/// Attached property that auto-wires a view's DataContext to its ViewModel via DI.
/// Usage: helpers:ViewModelLocator.AutoWireViewModel="True"
/// </summary>
public static class ViewModelLocator
{
    public static readonly DependencyProperty AutoWireViewModelProperty =
        DependencyProperty.RegisterAttached(
            "AutoWireViewModel",
            typeof(bool),
            typeof(ViewModelLocator),
            new PropertyMetadata(false));

    public static bool GetAutoWireViewModel(DependencyObject obj) =>
        (bool)obj.GetValue(AutoWireViewModelProperty);

    public static void SetAutoWireViewModel(DependencyObject obj, bool value) =>
        obj.SetValue(AutoWireViewModelProperty, value);
}
