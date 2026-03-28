using MemoryGame.Application.Common.Interfaces;

namespace MemoryGame.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    public string Hash(string plainPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);
    }

    public bool Verify(string plainPassword, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
    }
}
