using FluentAssertions;
using Investager.Api.Policies;
using Investager.Core.Constants;
using Investager.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Api.UnitTests.Policies
{
    public class AuthenticatedUserHandlerUnitTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        private readonly Mock<IJwtTokenService> _mockJwtTokenService = new Mock<IJwtTokenService>();
        private readonly HttpContext _httpContext = new DefaultHttpContext();
        private readonly AuthorizationHandlerContext _context;

        private readonly AuthenticatedUserHandler _target;

        public AuthenticatedUserHandlerUnitTests()
        {
            _context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { new AuthenticatedUserRequirement() }, new ClaimsPrincipal(), null);
            _mockHttpContextAccessor.Setup(e => e.HttpContext).Returns(_httpContext);

            _target = new AuthenticatedUserHandler(_mockHttpContextAccessor.Object, _mockJwtTokenService.Object);
        }

        [Fact]
        public void HandleAsync_WhenNoAuthorizationHeader_Throws()
        {
            // Act
            Func<Task> act = async () => await _target.HandleAsync(_context);

            // Assert
            act.Should().Throw<Exception>();
            _context.HasSucceeded.Should().BeFalse();
        }

        [Fact]
        public void HandleAsync_WhenAuthorizationHeaderInvalid_Throws()
        {
            // Arrange
            _httpContext.Request.Headers.Add("Authorization", "1337");

            // Act
            Func<Task> act = async () => await _target.HandleAsync(_context);

            // Assert
            act.Should().Throw<Exception>();
            _context.HasSucceeded.Should().BeFalse();
        }

        [Fact]
        public void HandleAsync_WhenTokenInvalid_Throws()
        {
            // Arrange
            var errorMessage = "Token not valid.";
            _httpContext.Request.Headers.Add("Authorization", "Bearer 1337");
            _mockJwtTokenService.Setup(e => e.Validate(It.IsAny<string>())).Throws(new Exception(errorMessage));

            // Act
            Func<Task> act = async () => await _target.HandleAsync(_context);

            // Assert
            act.Should().Throw<Exception>().WithMessage(errorMessage);
            _context.HasSucceeded.Should().BeFalse();
        }

        [Fact]
        public async Task HandleAsync_WhenTokenValid_PopulatesItemsDictionary()
        {
            // Arrange
            var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImp0aSI6IjZhZDZlODBjLWMxMmItNGQ1NS05MTNjLTkwZjgzMjMxZDQyYSIsImV4cCI6MTYyMTcxNjQ5MiwiaXNzIjoiaW52ZXN0YWdlciJ9.TYO6Ea4it_jnKB3RioSyVdxt4MfcPE4faiV9e6qHbx4";
            _httpContext.Request.Headers.Add("Authorization", $"Bearer {token}");
            _mockJwtTokenService.Setup(e => e.Validate(It.IsAny<string>())).Returns(DecodeToken(token));

            // Act
            await _target.HandleAsync(_context);

            // Assert
            _httpContext.Items[HttpContextKeys.UserId].Should().Be("15");
            _context.HasSucceeded.Should().BeTrue();
        }

        public JwtSecurityToken DecodeToken(string encodedToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(encodedToken);

            return jsonToken as JwtSecurityToken;
        }
    }
}
