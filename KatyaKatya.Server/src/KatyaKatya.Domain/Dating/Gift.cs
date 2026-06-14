using KatyaKatya.Domain.Common;

namespace KatyaKatya.Domain.Dating;

public class Gift : BaseEntity
{
    public int UserId { get; private set; }
    public int CharacterId { get; private set; }
    public string GiftType { get; private set; } = null!;
    public int LovePointsDelta { get; private set; }
    public DateTime SentAt { get; private set; }
    public string? ResponseKey { get; private set; }

    private Gift() { }

    public static Gift Create(int userId, int characterId, string giftType, int lovePointsDelta, string? responseKey)
    {
        return new Gift
        {
            UserId = userId,
            CharacterId = characterId,
            GiftType = ValidateGiftType(giftType),
            LovePointsDelta = lovePointsDelta,
            SentAt = DateTime.UtcNow,
            ResponseKey = responseKey
        };
    }

    private static string ValidateGiftType(string giftType)
    {
        if (string.IsNullOrWhiteSpace(giftType))
            throw new DomainException(DomainErrors.Dating.GiftTypeEmpty);

        if (giftType.Length > 80)
            throw new DomainException(DomainErrors.Dating.GiftTypeTooLong);

        return giftType;
    }
}
