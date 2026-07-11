using Practice.Application.DTO;
using Practice.Application.Repositories;
using Practice.Application.Security;
using Practice.Domain.Models;

namespace Practice.Application.Service;

public sealed class UserService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

    public async Task<User> RegisterAsync(
        RegisterUserDto dto,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByLoginAsync(
            dto.Login,
            cancellationToken);

        if (existingUser is not null)
            throw new InvalidOperationException("User with this login already exists.");

        var passwordHash = _passwordHasher.Hash(dto.Password);

        var user = new User(
            Guid.NewGuid(),
            dto.Login,
            passwordHash,
            UserRole.User);

        _userRepository.Add(user);

        await _userRepository.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task<string> LoginAsync(
        LoginUserDto dto,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByLoginAsync(
            dto.Login,
            cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid login or password.");

        var passwordIsValid = _passwordHasher.Verify(
            dto.Password,
            user.PasswordHash);

        if (!passwordIsValid)
            throw new UnauthorizedAccessException("Invalid login or password.");

        return _jwtTokenGenerator.Generate(user);
    }
}