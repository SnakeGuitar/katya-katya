using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KatyaKatya;

/// <summary>
/// Resolves Views from ViewModels by naming convention.
/// e.g. KatyaKatya.ViewModels.Session.LoginViewModel → KatyaKatya.Views.Session.LoginView
/// </summary>
[RequiresUnreferencedCode("ViewLocator uses reflection to find view types.")]
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var vmName = param.GetType().FullName!;

        // Replace "ViewModels" with "Views" and remove "Model" suffix
        var viewName = vmName
            .Replace(".ViewModels.", ".Views.")
            .Replace("ViewModel", "View", StringComparison.Ordinal);

        var viewType = Type.GetType(viewName);

        if (viewType is null)
            return new TextBlock
            {
                Text = $"[ViewLocator] View not found: {viewName}",
                Foreground = Avalonia.Media.Brushes.Red,
                FontSize = 16,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };

        try
        {
            return (Control)Activator.CreateInstance(viewType)!;
        }
        catch (Exception ex)
        {
            return new TextBlock
            {
                Text = $"[ViewLocator] Error creating {viewName}:\n{ex.InnerException?.Message ?? ex.Message}",
                Foreground = Avalonia.Media.Brushes.Red,
                FontSize = 14,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
        }
    }

    public bool Match(object? data) => data is ObservableObject;
}
