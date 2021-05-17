using Investager.Core.Constants;
using Investager.Core.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Investager.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private const string Issuer = "investager";
        private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(60);

        private readonly ITimeHelper _timeHelper;
        private readonly SigningCredentials _signingCredentials;

        public JwtTokenService(ITimeHelper timeHelper, SigningCredentials signingCredentials)
        {
            _timeHelper = timeHelper;
            _signingCredentials = signingCredentials;
        }

        public string GetAccessToken(int userId)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var jwt = new JwtSecurityToken(
                issuer: Issuer,
                expires: _timeHelper.GetUtcNow() + AccessTokenLifetime,
                claims: claims,
                signingCredentials: _signingCredentials);

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return token;
        }

        public string GetRefreshToken(int userId)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(InvestagerClaimNames.RefreshToken, "1")
            };

            var jwt = new JwtSecurityToken(
                issuer: Issuer,
                claims: claims,
                signingCredentials: _signingCredentials);

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return token;
        }
    }
}
