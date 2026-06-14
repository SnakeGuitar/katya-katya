using KatyaKatya.Domain.Common;

namespace KatyaKatya.Domain.Dating;

public class Character : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string AssetKey { get; private set; } = null!;
    public bool IsActive { get; private set; }

    private Character() { }

    public static Character Create(string name, string? description, string assetKey)
    {
        return new Character
        {
            Name = ValidateName(name),
            Description = description,
            AssetKey = ValidateAssetKey(assetKey),
            IsActive = true
        };
    }

    public void UpdateDetails(string name, string? description, string assetKey)
    {
        Name = ValidateName(name);
        Description = description;
        AssetKey = ValidateAssetKey(assetKey);
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(DomainErrors.Dating.CharacterNameEmpty);

        if (name.Length > 80)
            throw new DomainException(DomainErrors.Dating.CharacterNameTooLong);

        return name;
    }

    private static string ValidateAssetKey(string assetKey)
    {
        if (string.IsNullOrWhiteSpace(assetKey))
            throw new DomainException(DomainErrors.Dating.CharacterAssetKeyEmpty);

        if (assetKey.Length > 80)
            throw new DomainException(DomainErrors.Dating.CharacterAssetKeyTooLong);

        return assetKey;
    }
}
