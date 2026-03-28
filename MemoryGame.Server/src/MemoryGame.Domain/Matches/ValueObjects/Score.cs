using MemoryGame.Domain.Common;

namespace MemoryGame.Domain.Matches.ValueObjects;

public sealed class Score
{
    public int Value { get; }

    private Score(int value) => Value = value;

    public static Score Zero => new(0);

    public static Score Create(int value)
    {
        if (value < 0)
            throw new DomainException("Score cannot be negative.");

        return new Score(value);
    }

    public Score Add(int points) => Create(Value + points);

    public override bool Equals(object? obj) =>
        obj is Score other && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();
}
