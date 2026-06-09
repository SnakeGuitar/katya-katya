namespace KatyaKatya.Services.Interfaces;

public interface IFilePickerService
{
    Task<PickedFile?> PickImageAsync();
}

public sealed record PickedFile(byte[] Bytes, string? PreviewPath);
