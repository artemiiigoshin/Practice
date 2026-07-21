using Microsoft.EntityFrameworkCore;
using Practice.Application.Repositories;
using Practice.Domain.Models;
using Practice.Infrastructure.DataContext;

namespace Practice.Infrastructure.Repositories;

public sealed class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context;

    public Task<User?> GetByLoginAsync(
        string login,
        CancellationToken cancellationToken = default)
    {
        return _context.Users.FirstOrDefaultAsync(
            user => user.Login == login,
            cancellationToken);
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}