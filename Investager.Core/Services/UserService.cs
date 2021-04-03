using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task RegisterUser(RegisterUserDto registerUserDto)
        {
            var user = new User
            {
                FirstName = registerUserDto.FirstName,
                LastName = registerUserDto.LastName,
                Email = registerUserDto.Email,
            };

            _unitOfWork.Users.Insert(user);
            await _unitOfWork.SaveChanges();
        }
    }
}
