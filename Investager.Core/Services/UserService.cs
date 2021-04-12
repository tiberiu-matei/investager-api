using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public class UserService : IUserService
    {
        private readonly ICoreUnitOfWork _unitOfWork;
        private readonly IPasswordHelper _passwordHelper;

        public UserService(ICoreUnitOfWork unitOfWork, IPasswordHelper passwordHelper)
        {
            _unitOfWork = unitOfWork;
            _passwordHelper = passwordHelper;
        }

        public async Task RegisterUserAsync(RegisterUserDto registerUserDto)
        {
            var encodedPassword = _passwordHelper.EncodePassword(registerUserDto.Password);

            var user = new User
            {
                FirstName = registerUserDto.FirstName,
                LastName = registerUserDto.LastName,
                Email = registerUserDto.Email.ToLowerInvariant(),
                DisplayEmail = registerUserDto.Email,
                PasswordSalt = encodedPassword.Salt,
                PasswordHash = encodedPassword.Hash,
            };

            _unitOfWork.Users.Insert(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
