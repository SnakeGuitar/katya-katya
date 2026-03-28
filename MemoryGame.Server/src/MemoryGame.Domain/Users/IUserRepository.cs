using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(Email email);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<bool> ExistsByEmailAsync(Email email);
    Task AddAsync(User user);
    void Update(User user);
    void Remove(User user);
}
