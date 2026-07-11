using Practice.Domain.Models;

namespace Practice.Application.Repositories;

public interface IUserRepository
{
    Task<User?> GetByLoginAsync(
        string login,
        CancellationToken cancellationToken = default);

    void Add(User user);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}