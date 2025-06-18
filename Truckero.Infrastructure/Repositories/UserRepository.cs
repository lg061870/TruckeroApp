using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        var result = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        return result;
    }

    public async Task<User?> GetUserByAccessTokenAsync(string accessToken)
    {
        return await _context.Users
            .Include(u => u.AuthTokens)
            .Where(u => u.AuthTokens.Any(t => t.AccessToken == accessToken && t.RevokedAt == null))
            .FirstOrDefaultAsync();
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveUserChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
