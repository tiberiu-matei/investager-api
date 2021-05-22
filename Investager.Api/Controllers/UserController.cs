using Investager.Core.Constants;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
            var userId = Convert.ToInt32(HttpContext.Items[HttpContextKeys.UserId]);
            var response = await _userService.Get(userId);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            var response = await _userService.RegisterUser(registerUserDto);

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
            var userId = Convert.ToInt32(HttpContext.Items[HttpContextKeys.UserId]);
            await _userService.Update(userId, updateUserDto);

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            var userId = Convert.ToInt32(HttpContext.Items[HttpContextKeys.UserId]);
            await _userService.Delete(userId);

            return NoContent();
        }
    }
}
