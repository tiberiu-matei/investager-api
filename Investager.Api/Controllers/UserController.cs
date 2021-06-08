using Investager.Core.Constants;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Investager.Api.Controllers
{
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
        public async Task<IActionResult> Get()
        {
            var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);
            var response = await _userService.Get(userId);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            var response = await _userService.Register(registerUserDto);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPut("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var response = await _userService.Login(loginDto.Email, loginDto.Password);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPut("refreshtoken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var response = await _userService.RefreshToken(refreshTokenDto.RefreshToken);

            return Ok(new AccessTokenDto { AccessToken = response });
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
}
