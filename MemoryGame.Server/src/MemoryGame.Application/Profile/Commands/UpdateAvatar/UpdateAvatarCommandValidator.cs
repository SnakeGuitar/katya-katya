using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Profile.Commands.UpdateAvatar;

/// <summary>
/// Validates <see cref="UpdateAvatarCommand"/>: ensures the user identifier is valid,
/// image data is present, does not exceed 5 MB, and has a recognized format (JPG, PNG, or BMP).
/// </summary>
public class UpdateAvatarCommandValidator : AbstractValidator<UpdateAvatarCommand>
{
    private static readonly byte[] JpgSignature = [0xFF, 0xD8];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47];
    private static readonly byte[] BmpSignature = [0x42, 0x4D];

    /// <summary>
    /// Initializes validation rules for avatar update.
    /// </summary>
    public UpdateAvatarCommandValidator()
    {
        RuleFor(x => x.UserId).ValidId();

        RuleFor(x => x.AvatarData)
            .NotNull().WithMessage("VALIDATION_AVATAR_REQUIRED")
            .NotEmpty().WithMessage("VALIDATION_AVATAR_REQUIRED")
            .Must(data => data.Length <= ValidationConstants.Avatar.MaxSizeBytes)
                .WithMessage("VALIDATION_AVATAR_TOO_LARGE")
                .When(x => x.AvatarData is { Length: > 0 })
            .Must(HasValidImageSignature)
                .WithMessage("VALIDATION_AVATAR_INVALID_FORMAT")
                .When(x => x.AvatarData is { Length: > 4 });
    }

    private static bool HasValidImageSignature(byte[] data)
    {
        if (data.Length < 4) return false;

        bool isJpg = data[0] == JpgSignature[0] && data[1] == JpgSignature[1];
        bool isPng = data[0] == PngSignature[0] && data[1] == PngSignature[1]
                  && data[2] == PngSignature[2] && data[3] == PngSignature[3];
        bool isBmp = data[0] == BmpSignature[0] && data[1] == BmpSignature[1];

        return isJpg || isPng || isBmp;
    }
}
