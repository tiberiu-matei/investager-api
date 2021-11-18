using Investager.Core.Constants;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Investager.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<UserDto>> Get()
    {
        var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);
        var response = await _userService.Get(userId);

        return response;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<RegisterUserResponse>> Register([FromBody] RegisterUserDto registerUserDto)
    {
        var response = await _userService.Register(registerUserDto);

        return response;
    }

    [AllowAnonymous]
    [HttpPut("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginDto loginDto)
    {
        var response = await _userService.Login(loginDto.Email, loginDto.Password);

        return response;
    }

    [AllowAnonymous]
    [HttpPut("refreshtoken")]
    public async Task<ActionResult<AccessTokenDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var response = await _userService.RefreshToken(refreshTokenDto.RefreshToken);
        var dto = new AccessTokenDto { AccessToken = response };

        return dto;
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserDto updateUserDto)
    {
        var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);
        await _userService.Update(userId, updateUserDto);

        return NoContent();
    }

    [HttpPut("theme")]
    public async Task<IActionResult> UpdateTheme([FromBody] UpdateThemeRequest request)
    {
        var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);

        await _userService.UpdateTheme(userId, request.Theme);

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);
        await _userService.Delete(userId);

        return NoContent();
    }
}
