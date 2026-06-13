using Microsoft.EntityFrameworkCore;
using KatyaKatya.Domain.Users;
using KatyaKatya.Domain.Users.ValueObjects;
using KatyaKatya.Infrastructure.Persistence;

namespace KatyaKatya.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MemoryGameDbContext _context;

    public UserRepository(MemoryGameDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(Email email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> ExistsByEmailAsync(Email email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public void Remove(User user)
    {
        _context.Users.Remove(user);
    }
}
