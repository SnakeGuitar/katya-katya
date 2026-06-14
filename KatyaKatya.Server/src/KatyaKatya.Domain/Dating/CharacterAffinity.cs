namespace KatyaKatya.Domain.Dating;

public class CharacterAffinity
{
    public int UserId { get; private set; }
    public int CharacterId { get; private set; }
    public int LovePoints { get; private set; }
    public AffinityLevel Level { get; private set; }
    public RelationshipMood Mood { get; private set; }
    public DateTime? LastInteractionAt { get; private set; }
    public int TotalGiftsReceived { get; private set; }
    public int TotalDates { get; private set; }

    private CharacterAffinity() { }

    public static CharacterAffinity Create(int userId, int characterId)
    {
        return new CharacterAffinity
        {
            UserId = userId,
            CharacterId = characterId,
            LovePoints = 0,
            Level = AffinityLevel.Stranger,
            Mood = RelationshipMood.Neutral
        };
    }

    public void ApplyGift(int lovePointsDelta, RelationshipMood mood)
    {
        AddLovePoints(lovePointsDelta);
        Mood = mood;
        TotalGiftsReceived++;
        LastInteractionAt = DateTime.UtcNow;
    }

    public void CompleteDate(int lovePointsDelta, RelationshipMood mood)
    {
        AddLovePoints(lovePointsDelta);
        Mood = mood;
        TotalDates++;
        LastInteractionAt = DateTime.UtcNow;
    }

    public void SetLevel(AffinityLevel level) => Level = level;

    private void AddLovePoints(int lovePointsDelta)
    {
        LovePoints = Math.Max(0, LovePoints + lovePointsDelta);
    }
}
