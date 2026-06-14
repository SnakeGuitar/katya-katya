using KatyaKatya.Domain.Common;

namespace KatyaKatya.Domain.Dating;

public class UnlockedDialogue : BaseEntity
{
    public int UserId { get; private set; }
    public int CharacterId { get; private set; }
    public string DialogueKey { get; private set; } = null!;
    public DateTime UnlockedAt { get; private set; }

    private UnlockedDialogue() { }

    public static UnlockedDialogue Create(int userId, int characterId, string dialogueKey)
    {
        return new UnlockedDialogue
        {
            UserId = userId,
            CharacterId = characterId,
            DialogueKey = ValidateDialogueKey(dialogueKey),
            UnlockedAt = DateTime.UtcNow
        };
    }

    private static string ValidateDialogueKey(string dialogueKey)
    {
        if (string.IsNullOrWhiteSpace(dialogueKey))
            throw new DomainException(DomainErrors.Dating.DialogueKeyEmpty);

        if (dialogueKey.Length > 120)
            throw new DomainException(DomainErrors.Dating.DialogueKeyTooLong);

        return dialogueKey;
    }
}
