using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using KatyaKatya.Services.Interfaces;

namespace KatyaKatya.Services.UI;

public sealed class FilePickerService : IFilePickerService
{
    private static readonly FilePickerFileType ImageFiles = new("Image files")
    {
        Patterns = ["*.jpg", "*.jpeg", "*.png", "*.bmp"],
        AppleUniformTypeIdentifiers = ["public.image"],
        MimeTypes = ["image/jpeg", "image/png", "image/bmp"]
    };

    public async Task<PickedFile?> PickImageAsync()
    {
        var topLevel = GetTopLevel();
        if (topLevel is null)
            return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Select profile photo",
                AllowMultiple = false,
                FileTypeFilter = [ImageFiles]
            });

        var file = files.FirstOrDefault();
        if (file is null)
            return null;

        await using var stream = await file.OpenReadAsync();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);

        var previewPath = file.Path.IsFile ? file.Path.LocalPath : null;
        return new PickedFile(memory.ToArray(), previewPath);
    }

    private static TopLevel? GetTopLevel()
    {
        var lifetime = Application.Current?.ApplicationLifetime;
        return lifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            ISingleViewApplicationLifetime singleView => TopLevel.GetTopLevel(singleView.MainView),
            _ => null
        };
    }
}
