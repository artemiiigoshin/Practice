using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Practice.Application.DTO;
using Practice.Application.Service;

namespace Practice.Controllers;

[ApiController]
[Route("auth")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterUserDto dto,
        CancellationToken cancellationToken)
    {
        await _userService.RegisterAsync(dto, cancellationToken);

        return NoContent();
    }

    [AllowAnonymous]
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