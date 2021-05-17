namespace Investager.Core.Interfaces
{
    public interface IJwtTokenService
    {
        string GetAccessToken(int userId);

        string GetRefreshToken(int userId);
    }
}
