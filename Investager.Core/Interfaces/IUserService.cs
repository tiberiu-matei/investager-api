using Investager.Core.Dtos;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> Get(int userId);

        Task<RegisterUserResponse> Register(RegisterUserDto registerUserDto);

        Task<LoginResponse> Login(string email, string password);

        Task<string> RefreshToken(string refreshToken);

        Task Update(int userId, UpdateUserDto updateUserDto);

        Task Delete(int userId);
    }
}
