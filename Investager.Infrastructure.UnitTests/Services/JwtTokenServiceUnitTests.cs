using FluentAssertions;
using Investager.Core.Interfaces;
using Investager.Infrastructure.Services;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Xunit;

namespace Investager.Infrastructure.UnitTests.Services
{
    public class JwtTokenServiceUnitTests
    {
        private static readonly DateTime UtcNow = new DateTime(2021, 05, 10, 10, 10, 10, DateTimeKind.Utc);

        private readonly Mock<ITimeHelper> _mockTimeHelper = new Mock<ITimeHelper>();

        private readonly JwtTokenService _target;

        public JwtTokenServiceUnitTests()
        {
            _mockTimeHelper.Setup(e => e.GetUtcNow()).Returns(UtcNow);

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("12314ko19k9kf399j555mm54k4kkfkfk444"));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            _target = new JwtTokenService(_mockTimeHelper.Object, signingCredentials);
        }

        [Fact]
        public void GetAccessToken_Includes_UserId()
        {
            var encodedToken = _target.GetAccessToken(385);

            var token = GetToken(encodedToken);

            token.Payload["sub"].Should().Be("385");
        }

        [Fact]
        public void GetAccessToken_Includes_Issuer()
        {
            var encodedToken = _target.GetAccessToken(385);

            var token = GetToken(encodedToken);

            token.Issuer.Should().Be("investager");
        }

        [Fact]
        public void GetAccessToken_Includes_ExpiryDate()
        {
            var encodedToken = _target.GetAccessToken(385);

            var token = GetToken(encodedToken);

            token.ValidTo.Should().Be(UtcNow.AddMinutes(60));
        }

        [Fact]
        public void GetRefreshToken_Includes_UserId()
        {
            var encodedToken = _target.GetRefreshToken(385);

            var token = GetToken(encodedToken);

            token.Payload["sub"].Should().Be("385");
        }

        [Fact]
        public void GetRefreshToken_Includes_Issuer()
        {
            var encodedToken = _target.GetRefreshToken(385);

            var token = GetToken(encodedToken);

            token.Issuer.Should().Be("investager");
        }

        [Fact]
        public void GetRefreshToken_DoesNotInclude_ExpiryDate()
        {
            var encodedToken = _target.GetRefreshToken(385);

            var token = GetToken(encodedToken);

            token.ValidTo.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public void GetRefreshToken_Includes_RefreshTokenClaim()
        {
            var encodedToken = _target.GetRefreshToken(385);

            var token = GetToken(encodedToken);

            token.Payload["rtk"].Should().Be("1");
        }

        private JwtSecurityToken GetToken(string encodedToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(encodedToken);

            return jsonToken as JwtSecurityToken;
        }
    }
}
