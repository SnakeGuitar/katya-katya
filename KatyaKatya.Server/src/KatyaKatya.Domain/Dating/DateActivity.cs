using KatyaKatya.Domain.Common;

namespace KatyaKatya.Domain.Dating;

public class DateActivity : BaseEntity
{
    public int UserId { get; private set; }
    public int CharacterId { get; private set; }
    public string ActivityKey { get; private set; } = null!;
    public int LovePointsDelta { get; private set; }
    public DateTime CompletedAt { get; private set; }
    public string? UnlockedDialogueKey { get; private set; }

    private DateActivity() { }

    public static DateActivity Create(
        int userId,
        int characterId,
        string activityKey,
        int lovePointsDelta,
        string? unlockedDialogueKey)
    {
        return new DateActivity
        {
            UserId = userId,
            CharacterId = characterId,
            ActivityKey = ValidateActivityKey(activityKey),
            LovePointsDelta = lovePointsDelta,
            CompletedAt = DateTime.UtcNow,
            UnlockedDialogueKey = unlockedDialogueKey
        };
    }

    private static string ValidateActivityKey(string activityKey)
    {
        if (string.IsNullOrWhiteSpace(activityKey))
            throw new DomainException(DomainErrors.Dating.DateActivityKeyEmpty);

        if (activityKey.Length > 80)
            throw new DomainException(DomainErrors.Dating.DateActivityKeyTooLong);

        return activityKey;
    }
}
