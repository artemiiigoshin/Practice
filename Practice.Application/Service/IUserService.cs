using Practice.Application.DTO;
using Practice.Domain.Models;

namespace Practice.Application.Service;

public interface IUserService
{
    Task<User> RegisterAsync(
        RegisterUserDto dto,
        CancellationToken cancellationToken = default);

    Task<string> LoginAsync(
        LoginUserDto dto,
        CancellationToken cancellationToken = default);
}