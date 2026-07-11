using Microsoft.AspNetCore.Mvc;
using Practice.Application.DTO;
using Practice.Application.Service;

namespace Practice.Controllers;

[ApiController]
[Route("users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterUserDto dto,
        CancellationToken cancellationToken)
    {
        var user = await _userService.RegisterAsync(
            dto,
            cancellationToken);

        return Created(
            $"/users/{user.Id}",
            new
            {
                user.Id,
                user.Login,
                Role = user.Role.ToString()
            });
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenDto>> Login(
        LoginUserDto dto,
        CancellationToken cancellationToken)
    {
        var token = await _userService.LoginAsync(
            dto,
            cancellationToken);

        return Ok(new TokenDto(token));
    }
}