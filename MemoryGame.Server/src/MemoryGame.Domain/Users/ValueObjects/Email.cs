using MemoryGame.Domain.Common;

namespace MemoryGame.Domain.Users.ValueObjects;

public sealed class Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty.");

        if (value.Length > 50)
            throw new DomainException("Email cannot exceed 50 characters.");

        if (!value.Contains('@'))
            throw new DomainException("Email format is invalid.");

        return new Email(value.Trim().ToLowerInvariant());
    }

    public override bool Equals(object? obj) =>
        obj is Email other && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;
}
