using Investager.Core.Dtos;
using Investager.Core.Exceptions;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public class UserService : IUserService
    {
        private readonly ICoreUnitOfWork _unitOfWork;
        private readonly IPasswordHelper _passwordHelper;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ITimeHelper _timeHelper;

        public UserService(
            ICoreUnitOfWork unitOfWork,
            IPasswordHelper passwordHelper,
            IJwtTokenService jwtTokenService,
            ITimeHelper timeHelper)
        {
            _unitOfWork = unitOfWork;
            _passwordHelper = passwordHelper;
            _jwtTokenService = jwtTokenService;
            _timeHelper = timeHelper;
        }

        public async Task<UserDto> Get(int userId)
        {
            var users = await _unitOfWork.Users.Find(e => e.Id == userId);
            var user = users.Single();

            return new UserDto
            {
                Email = user.Email,
                DisplayName = user.DisplayName,
                Theme = user.Theme,
            };
        }

        public async Task<RegisterUserResponse> Register(RegisterUserDto registerUserDto)
        {
            var encodedPassword = _passwordHelper.EncodePassword(registerUserDto.Password);

            var user = new User
            {
                Email = registerUserDto.Email.ToLowerInvariant(),
                DisplayEmail = registerUserDto.Email,
                DisplayName = registerUserDto.DisplayName,
                Theme = Theme.None,
                PasswordSalt = encodedPassword.Salt,
                PasswordHash = encodedPassword.Hash,
            };

            _unitOfWork.Users.Add(user);
            await _unitOfWork.SaveChanges();

            var accessToken = _jwtTokenService.GetAccessToken(user.Id);
            var refreshToken = _jwtTokenService.GetRefreshToken(user.Id);

            await AddRefreshToken(user, refreshToken);

            var response = new RegisterUserResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };

            return response;
        }

        public async Task<LoginResponse> Login(string email, string password)
        {
            var userResponse = await _unitOfWork.Users.Find(e => e.Email == email);
            var user = userResponse.Single();

            var passwordCorrect = _passwordHelper.IsPasswordCorrect(password, user.PasswordHash, user.PasswordSalt);
            if (!passwordCorrect)
            {
                throw new InvalidPasswordException("Password invalid.");
            }

            var accessToken = _jwtTokenService.GetAccessToken(user.Id);
            var refreshToken = _jwtTokenService.GetRefreshToken(user.Id);

            await AddRefreshToken(user, refreshToken);

            var response = new LoginResponse
            {
                DisplayName = user.DisplayName,
                Theme = user.Theme,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };

            return response;
        }

        public async Task<string> RefreshToken(string refreshToken)
        {
            var token = DecodeToken(refreshToken);
            var userId = Convert.ToInt32(token.Subject);
            var userTokens = await _unitOfWork.RefreshTokens.Find(e => e.UserId == userId && e.EncodedValue == refreshToken);
            userTokens.Single();

            var accessToken = _jwtTokenService.GetAccessToken(userId);

            return accessToken;
        }

        public async Task Update(int userId, UpdateUserDto updateUserDto)
        {
            var user = await _unitOfWork.Users.GetByIdWithTracking(userId);
            user.DisplayName = updateUserDto.DisplayName;

            await _unitOfWork.SaveChanges();
        }

        public async Task UpdateTheme(int userId, Theme theme)
        {
            var user = await _unitOfWork.Users.GetByIdWithTracking(userId);
            user.Theme = theme;

            await _unitOfWork.SaveChanges();
        }

        public async Task Delete(int userId)
        {
            _unitOfWork.Users.Delete(userId);

            await _unitOfWork.SaveChanges();
        }

        private async Task AddRefreshToken(User user, string refreshToken)
        {
            var refreshTokenEntity = new RefreshToken
            {
                EncodedValue = refreshToken,
                CreatedAt = _timeHelper.GetUtcNow(),
                LastUsedAt = _timeHelper.GetUtcNow(),
                User = user,
            };

            _unitOfWork.RefreshTokens.Add(refreshTokenEntity);
            await _unitOfWork.SaveChanges();
        }

        private JwtSecurityToken DecodeToken(string encodedToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(encodedToken);

            return jsonToken as JwtSecurityToken;
        }
    }
}
