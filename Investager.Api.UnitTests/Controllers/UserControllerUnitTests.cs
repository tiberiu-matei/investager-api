using FluentAssertions;
using Investager.Api.Controllers;
using Investager.Core.Constants;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Api.UnitTests.Controllers
{
    public class UserControllerUnitTests
    {
        private readonly Mock<IUserService> _mockUserService = new Mock<IUserService>();
        private readonly HttpContext _httpContext = new DefaultHttpContext();

        private readonly UserController _target;

        public UserControllerUnitTests()
        {
            _target = new UserController(_mockUserService.Object);
            _target.ControllerContext.HttpContext = _httpContext;
        }

        [Fact]
        public void Get_WhenUserIdNotPresent_Throws()
        {
            // Act
            Func<Task> act = async () => await _target.Get();

            // Assert
            act.Should().Throw<Exception>();
            _mockUserService.Verify(e => e.Get(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Get_ReturnsExpectedData()
        {
            // Arrange
            var userId = 3;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();

            var dto = new UserDto
            {
                Email = "dhura@dora.sq",
                DisplayName = "Dhurata",
                Theme = Theme.Dark,
            };

            _mockUserService.Setup(e => e.Get(userId)).ReturnsAsync(dto);

            // Act
            var response = await _target.Get();

            // Assert
            response.Value.Should().Be(dto);
            _mockUserService.Verify(e => e.Get(userId), Times.Once);
        }

        [Fact]
        public async Task Register_ReturnsExpectedData()
        {
            // Arrange
            var request = new RegisterUserDto
            {
                Email = "dhura@dora.sq",
                Password = "keshkesh",
                DisplayName = "Dhurata",
            };

            var dto = new RegisterUserResponse
            {
                AccessToken = "abc",
                RefreshToken = "def",
            };

            _mockUserService.Setup(e => e.Register(request)).ReturnsAsync(dto);

            // Act
            var response = await _target.Register(request);

            // Assert
            response.Value.Should().Be(dto);
            _mockUserService.Verify(e => e.Register(request), Times.Once);
        }

        [Fact]
        public async Task Login_ReturnsExpectedData()
        {
            // Arrange
            var request = new LoginDto
            {
                Email = "dhura@dora.sq",
                Password = "keshkesh",
            };

            var dto = new LoginResponse
            {
                DisplayName = "Dhurata",
                Theme = Theme.Dark,
                AccessToken = "abc",
                RefreshToken = "def",
            };

            _mockUserService.Setup(e => e.Login(request.Email, request.Password)).ReturnsAsync(dto);

            // Act
            var response = await _target.Login(request);

            // Assert
            response.Value.Should().Be(dto);
            _mockUserService.Verify(e => e.Login(request.Email, request.Password), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_ReturnsExpectedData()
        {
            // Arrange
            var request = new RefreshTokenDto
            {
                RefreshToken = "abc",
            };

            var accessToken = "accc";

            _mockUserService.Setup(e => e.RefreshToken(request.RefreshToken)).ReturnsAsync(accessToken);

            // Act
            var response = await _target.RefreshToken(request);

            // Assert
            response.Value.AccessToken.Should().Be(accessToken);
            _mockUserService.Verify(e => e.RefreshToken(request.RefreshToken), Times.Once);
        }

        [Fact]
        public void Update_WhenUserIdNotPresent_Throws()
        {
            // Act
            Func<Task> act = async () => await _target.Update(new UpdateUserDto());

            // Assert
            act.Should().Throw<Exception>();
            _mockUserService.Verify(e => e.Update(It.IsAny<int>(), It.IsAny<UpdateUserDto>()), Times.Never);
        }

        [Fact]
        public async Task Update_CallsService()
        {
            // Arrange
            var userId = 3;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();

            var request = new UpdateUserDto
            {
                DisplayName = "Dhurata",
            };

            // Act
            var response = await _target.Update(request);
            var result = response as NoContentResult;

            // Assert
            result.StatusCode.Should().Be(204);
            _mockUserService.Verify(e => e.Update(userId, request), Times.Once);
        }

        [Fact]
        public void UpdateTheme_WhenUserIdNotPresent_Throws()
        {
            // Act
            Func<Task> act = async () => await _target.UpdateTheme(new UpdateThemeRequest { Theme = Theme.Light });

            // Assert
            act.Should().Throw<Exception>();
            _mockUserService.Verify(e => e.UpdateTheme(It.IsAny<int>(), It.IsAny<Theme>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTheme_CallsService()
        {
            // Arrange
            var userId = 3;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();

            // Act
            var response = await _target.UpdateTheme(new UpdateThemeRequest { Theme = Theme.Dark });
            var result = response as NoContentResult;

            // Assert
            result.StatusCode.Should().Be(204);
            _mockUserService.Verify(e => e.UpdateTheme(userId, Theme.Dark), Times.Once);
        }

        [Fact]
        public void Delete_WhenUserIdNotPresent_Throws()
        {
            // Act
            Func<Task> act = async () => await _target.Delete();

            // Assert
            act.Should().Throw<Exception>();
            _mockUserService.Verify(e => e.Delete(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Delete_CallsService()
        {
            // Arrange
            var userId = 3;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();

            // Act
            var response = await _target.Delete();
            var result = response as NoContentResult;

            // Assert
            result.StatusCode.Should().Be(204);
            _mockUserService.Verify(e => e.Delete(userId), Times.Once);
        }
    }
}
