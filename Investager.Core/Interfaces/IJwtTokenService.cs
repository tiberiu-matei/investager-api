using System.IdentityModel.Tokens.Jwt;

namespace Investager.Core.Interfaces
{
    public interface IJwtTokenService
    {
        string GetAccessToken(int userId);

        string GetRefreshToken(int userId);

        JwtSecurityToken Validate(string token);
    }
}
