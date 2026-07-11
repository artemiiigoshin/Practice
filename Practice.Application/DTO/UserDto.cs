namespace Practice.Application.DTO;

public sealed record RegisterUserDto(
    string Login,
    string Password);

public sealed record LoginUserDto(
    string Login,
    string Password);

public sealed record TokenDto(string Token);