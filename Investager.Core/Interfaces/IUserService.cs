using Investager.Core.Dtos;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface IUserService
    {
        Task<RegisterUserResponse> RegisterUser(RegisterUserDto registerUserDto);

        Task<LoginResponse> Login(string email, string password);

        Task<string> RefreshToken(string refreshToken);
    }
}
