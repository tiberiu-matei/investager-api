using FluentAssertions;
using Investager.Core.Dtos;
using Investager.Core.Exceptions;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Core.UnitTests.Services;

public class UserServiceUnitTests
{
    private readonly Mock<ICoreUnitOfWork> _mockUnitOfWork = new Mock<ICoreUnitOfWork>();
    private readonly Mock<IPasswordHelper> _mockPasswordHelper = new Mock<IPasswordHelper>();
    private readonly Mock<IJwtTokenService> _mockJwtTokenService = new Mock<IJwtTokenService>();
    private readonly Mock<ITimeHelper> _mockTimeHelper = new Mock<ITimeHelper>();

    private readonly Mock<IGenericRepository<User>> _mockUserRepository = new Mock<IGenericRepository<User>>();
    private readonly Mock<IGenericRepository<RefreshToken>> _mockRefreshTokenRepository = new Mock<IGenericRepository<RefreshToken>>();

    private readonly UserService _target;

    public UserServiceUnitTests()
    {
        _mockUnitOfWork.Setup(e => e.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(e => e.RefreshTokens).Returns(_mockRefreshTokenRepository.Object);
        _mockPasswordHelper
            .Setup(e => e.EncodePassword(It.IsAny<string>()))
            .Returns(new EncodedPassword { Salt = new byte[1], Hash = new byte[1] });

        _target = new UserService(
            _mockUnitOfWork.Object,
            _mockPasswordHelper.Object,
            _mockJwtTokenService.Object,
            _mockTimeHelper.Object);
    }

    [Fact]
    public async Task Get_WhenUserNotFound_Throws()
    {
        // Arrange
        _mockUserRepository
            .Setup(e => e.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User>());

        // Act
        Func<Task> act = async () => await _target.Get(1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Sequence contains no elements");
    }

    [Fact]
    public async Task Get_ReturnsExpectedProperties()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "abc@a.com",
            DisplayName = "dorel",
            Theme = Theme.Dark,
        };

        _mockUserRepository
            .Setup(e => e.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        // Act
        var userDto = await _target.Get(1);

        // Assert
        userDto.Email.Should().Be(user.Email);
        userDto.DisplayName.Should().Be(user.DisplayName);
        userDto.Theme.Should().Be(user.Theme);
    }

    [Fact]
    public async Task RegisterUser_CallsSaveChanges()
    {
        // Arrange
        var registerUser = new RegisterUserDto
        {
            Email = "1@2.com",
            Password = "s3cur3",
            DisplayName = "d0r3l",
        };

        // Act
        await _target.Register(registerUser);

        // Assert
        _mockUserRepository.Verify(e => e.Add(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(e => e.SaveChanges(), Times.Exactly(2));
    }

    [Fact]
    public async Task RegisterUser_SetsCorrectPasswordProperties()
    {
        // Arrange
        var password = "very_s3cur3";
        var encodedPassword = new EncodedPassword
        {
            Salt = new byte[2],
            Hash = new byte[4],
        };

        _mockPasswordHelper
            .Setup(e => e.EncodePassword(password))
            .Returns(encodedPassword);

        var registerUser = new RegisterUserDto
        {
            Email = "1@2.com",
            Password = password,
        };

        // Act
        await _target.Register(registerUser);

        // Assert
        _mockUserRepository.Verify(
            e => e.Add(It.Is<User>(u => u.PasswordSalt == encodedPassword.Salt
                && u.PasswordHash == encodedPassword.Hash)),
            Times.Once);
        _mockUnitOfWork.Verify(e => e.SaveChanges(), Times.Exactly(2));
    }

    [Fact]
    public async Task RegisterUser_SetsCorrectEmailProperties()
    {
        // Arrange
        var email = "Sp3CiAlAddrEss@domAIN.cOm";
        var registerUser = new RegisterUserDto
        {
            Email = email,
            Password = "123",
        };

        // Act
        await _target.Register(registerUser);

        // Assert
        _mockUserRepository.Verify(
            e => e.Add(It.Is<User>(u => u.Email == email.ToLowerInvariant() && u.DisplayEmail == email)),
            Times.Once);
        _mockUnitOfWork.Verify(e => e.SaveChanges(), Times.Exactly(2));
    }

    [Fact]
    public async Task RegisterUser_SetsCorrectTheme()
    {
        // Arrange
        var email = "Sp3CiAlAddrEss@domAIN.cOm";
        var registerUser = new RegisterUserDto
        {
            Email = email,
            Password = "123",
        };

        // Act
        await _target.Register(registerUser);

        // Assert
        _mockUserRepository.Verify(e => e.Add(It.Is<User>(u => u.Theme == Theme.None)), Times.Once);
        _mockUnitOfWork.Verify(e => e.SaveChanges(), Times.Exactly(2));
    }

    [Fact]
    public async Task RegisterUser_GeneratesTokens()
    {
        // Arrange
        var refreshToken = "myjwt";
        var accessToken = "access";
        _mockJwtTokenService.Setup(e => e.GetRefreshToken(It.IsAny<int>())).Returns(refreshToken);
        _mockJwtTokenService.Setup(e => e.GetAccessToken(It.IsAny<int>())).Returns(accessToken);

        var email = "Sp3CiAlAddrEss@domAIN.cOm";
        var registerUser = new RegisterUserDto
        {
            Email = email,
            Password = "123",
        };

        // Act
        var response = await _target.Register(registerUser);

        // Assert
        response.AccessToken.Should().Be(accessToken);
        response.RefreshToken.Should().Be(refreshToken);
        _mockRefreshTokenRepository.Verify(
            e => e.Add(It.Is<RefreshToken>(u => u.EncodedValue == refreshToken)),
            Times.Once);
        _mockUnitOfWork.Verify(e => e.SaveChanges(), Times.Exactly(2));
    }

    [Fact]
    public async Task RegisterUser_WhenSaveChangesFails_Throws()
    {
        // Arrange
        var errorMessage = "Unable to save.";
        _mockUnitOfWork.Setup(e => e.SaveChanges()).ThrowsAsync(new Exception(errorMessage));
        var registerUser = new RegisterUserDto { Email = "1@2.com", Password = "123" };

        // Act
        Func<Task> act = async () => await _target.Register(registerUser);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage(errorMessage);
    }

    [Fact]
    public async Task Login_GeneratesTokens()
    {
        // Arrange
        var refreshToken = "myjwt";
        var accessToken = "access";
        _mockJwtTokenService.Setup(e => e.GetRefreshToken(It.IsAny<int>())).Returns(refreshToken);
        _mockJwtTokenService.Setup(e => e.GetAccessToken(It.IsAny<int>())).Returns(accessToken);

        var user = new User
        {
            DisplayName = "gigino",
            Theme = Theme.Dark,
            Email = "stuff@investager.com",
            PasswordHash = new byte[] { 31, 155 },
            PasswordSalt = new byte[] { 101, 2 },
        };
        _mockUserRepository
            .Setup(e => e.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        var password = "s3cr3t";
        _mockPasswordHelper
            .Setup(e => e.IsPasswordCorrect(password, user.PasswordHash, user.PasswordSalt))
            .Returns(true);

        // Act
        var response = await _target.Login(user.Email, password);

        // Assert
        response.DisplayName.Should().Be(user.DisplayName);
        response.Theme.Should().Be(user.Theme);
        response.AccessToken.Should().Be(accessToken);
        response.RefreshToken.Should().Be(refreshToken);
        _mockRefreshTokenRepository.Verify(
            e => e.Add(It.Is<RefreshToken>(u => u.EncodedValue == refreshToken)),
            Times.Once);
        _mockUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Login_WhenSaveChangesFails_Throws()
    {
        // Arrange
        var errorMessage = "Unable to save.";
        _mockUnitOfWork
            .Setup(e => e.SaveChanges())
            .ThrowsAsync(new Exception(errorMessage));

        var refreshToken = "myjwt";
        var accessToken = "access";
        _mockJwtTokenService
            .Setup(e => e.GetRefreshToken(It.IsAny<int>()))
            .Returns(refreshToken);
        _mockJwtTokenService
            .Setup(e => e.GetAccessToken(It.IsAny<int>()))
            .Returns(accessToken);

        var user = new User
        {
            DisplayName = "gigino",
            Email = "stuff@investager.com",
            PasswordHash = new byte[] { 31, 155 },
            PasswordSalt = new byte[] { 101, 2 },
        };

        _mockUserRepository
            .Setup(e => e.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        var password = "s3cr3t";
        _mockPasswordHelper
            .Setup(e => e.IsPasswordCorrect(password, user.PasswordHash, user.PasswordSalt))
            .Returns(true);

        // Act
        Func<Task> act = async () => await _target.Login(user.Email, password);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage(errorMessage);
    }

    [Fact]
    public async Task Login_WhenPasswordIncorrect_Throws()
    {
        // Arrange
        var user = new User
        {
            DisplayName = "gigino",
            Email = "stuff@investager.com",
            PasswordHash = new byte[] { 31, 155 },
            PasswordSalt = new byte[] { 101, 2 },
        };

        _mockUserRepository
            .Setup(e => e.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        var password = "s3cr3t";
        _mockPasswordHelper
            .Setup(e => e.IsPasswordCorrect(password, user.PasswordHash, user.PasswordSalt))
            .Returns(false);

        // Act
        Func<Task> act = async () => await _target.Login(user.Email, password);

        // Assert
        await act.Should().ThrowAsync<InvalidPasswordException>()
            .WithMessage("Password invalid.");
    }

    [Fact]
    public async Task RefreshToken_GeneratesToken()
    {
        // Arrange
        var refreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwianRp" +
            "IjoiZjkxZmU3MDQtYzk3NC00ZGNkLThhNDUtNmVkZGNkODYwNDkwIiwicnRrIjoiMSIsImlz" +
            "cyI6ImludmVzdGFnZXIifQ.XzgfaEWn_LGSIFs1v6MgW3H0dWZpNhnXs-1TMLhAuow";
        var accessToken = "access";
        _mockJwtTokenService
            .Setup(e => e.GetAccessToken(It.IsAny<int>()))
            .Returns(accessToken);

        var userId = 7;

        var refreshTokenEntity = new RefreshToken
        {
            Id = 1,
            UserId = userId,
            EncodedValue = refreshToken,
        };

        _mockRefreshTokenRepository
            .Setup(e => e.Find(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { refreshTokenEntity });

        // Act
        var response = await _target.RefreshToken(refreshToken);

        // Assert
        response.Should().Be(accessToken);
        _mockJwtTokenService.Verify(e => e.GetAccessToken(userId), Times.Once);
        _mockRefreshTokenRepository.Verify(e => e.Add(It.IsAny<RefreshToken>()), Times.Never);
        _mockUnitOfWork.Verify(e => e.SaveChanges(), Times.Never);
    }

    [Fact]
    public async Task RefreshToken_WhenTokenNotFound_Throws()
    {
        // Arrange
        var refreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwianRpIjoiZjkxZmU3MDQ" +
            "tYzk3NC00ZGNkLThhNDUtNmVkZGNkODYwNDkwIiwicnRrIjoiMSIsImlzcyI6ImludmVzdGFnZXIifQ.XzgfaEW" +
            "n_LGSIFs1v6MgW3H0dWZpNhnXs-1TMLhAuow";

        var accessToken = "access";
        _mockJwtTokenService
            .Setup(e => e.GetAccessToken(It.IsAny<int>()))
            .Returns(accessToken);

        _mockRefreshTokenRepository
            .Setup(e => e.Find(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken>());

        // Act
        Func<Task> act = async () => await _target.RefreshToken(refreshToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Sequence contains no elements");
    }

    [Fact]
    public async Task Update_WhenSaveChangesFails_Throws()
    {
        // Arrange
        var errorMessage = "Unable to save.";
        _mockUnitOfWork
            .Setup(e => e.SaveChanges())
            .ThrowsAsync(new Exception(errorMessage));

        var user = new User
        {
            DisplayName = "gigino",
            Email = "stuff@investager.com",
        };
        _mockUserRepository
            .Setup(e => e.GetByIdWithTracking(It.IsAny<int>()))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _target.Update(1, new UpdateUserDto());

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage(errorMessage);
    }

    [Fact]
    public async Task Update_ChangesValues()
    {
        // Arrange
        var user = new User
        {
            DisplayName = "gigino",
            Email = "stuff@investager.com",
        };
        _mockUserRepository
            .Setup(e => e.GetByIdWithTracking(It.IsAny<int>()))
            .ReturnsAsync(user);

        var dto = new UpdateUserDto
        {
            DisplayName = "johnny",
        };

        // Act
        await _target.Update(1, dto);

        // Assert
        user.DisplayName.Should().Be(dto.DisplayName);
        _mockUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task UpdateTheme_WhenUserNotFound_Throws()
    {
        // Arrange
        var errorMessage = "User not found.";
        _mockUserRepository
            .Setup(e => e.GetByIdWithTracking(It.IsAny<int>()))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        Func<Task> act = async () => await _target.UpdateTheme(1, Theme.Dark);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage(errorMessage);
    }

    [Fact]
    public async Task UpdateTheme_ChangesValues()
    {
        // Arrange
        var user = new User
        {
            DisplayName = "gigino",
            Email = "stuff@investager.com",
            Theme = Theme.Light,
        };
        _mockUserRepository
            .Setup(e => e.GetByIdWithTracking(It.IsAny<int>()))
            .ReturnsAsync(user);

        // Act
        await _target.UpdateTheme(1, Theme.Dark);

        // Assert
        user.Theme.Should().Be(Theme.Dark);
        _mockUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenSaveChangesFails_Throws()
    {
        // Arrange
        var errorMessage = "Unable to save.";
        _mockUnitOfWork
            .Setup(e => e.SaveChanges())
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        Func<Task> act = async () => await _target.Delete(1);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage(errorMessage);
    }

    [Fact]
    public async Task Delete_DoesNotThrow()
    {
        // Act
        await _target.Delete(1);

        // Assert
        _mockUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);
    }
}
