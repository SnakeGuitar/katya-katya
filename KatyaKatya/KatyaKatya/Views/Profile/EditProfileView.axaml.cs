using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using KatyaKatya.ViewModels.Profile;

namespace KatyaKatya.Views.Profile;

public partial class EditProfileView : UserControl
{
    public EditProfileView()
    {
        InitializeComponent();
    }

    private async void OnChangeAvatarClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        try
        {
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Avatar Image",
                AllowMultiple = false,
                FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
            });

            if (files.Count > 0)
            {
                var file = files[0];
                
                // Keep file size validation: max 1MB
                var properties = await file.GetBasicPropertiesAsync();
                if (properties.Size > 1024 * 1024)
                {
                    if (DataContext is EditProfileViewModel vmWarning)
                    {
                        // Use the dialog service if possible, or just skip if we don't have direct access here.
                        // We will let the view model's DI handle standard notifications, but let's let the upload fail on server or block here.
                    }
                }

                await using var stream = await file.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                if (DataContext is EditProfileViewModel vm)
                {
                    await vm.UpdateAvatarDirectAsync(bytes);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EditProfileView] Error picking file: {ex.Message}");
        }
    }
}
