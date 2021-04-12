using FluentAssertions;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Core.Services;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Core.UnitTests.Services
{
    public class UserServiceUnitTests
    {
        private readonly Mock<ICoreUnitOfWork> _mockUnitOfWork = new Mock<ICoreUnitOfWork>();
        private readonly Mock<IGenericRepository<User>> _mockUserRepository = new Mock<IGenericRepository<User>>();
        private readonly Mock<IPasswordHelper> _mockPasswordHelper = new Mock<IPasswordHelper>();

        private readonly UserService _target;

        public UserServiceUnitTests()
        {
            _mockUnitOfWork.Setup(e => e.Users).Returns(_mockUserRepository.Object);
            _mockPasswordHelper.Setup(e => e.EncodePassword(It.IsAny<string>())).Returns(new EncodedPassword { Salt = new byte[1], Hash = new byte[1] });

            _target = new UserService(_mockUnitOfWork.Object, _mockPasswordHelper.Object);
        }

        [Fact]
        public async Task RegisterUser_CallsSaveChanges()
        {
            // Arrange
            var registerUser = new RegisterUserDto
            {
                Email = "1@2.com",
                Password = "s3cur3",
                FirstName = "d0r3l",
                LastName = "cast@n@",
            };

            // Act
            await _target.RegisterUserAsync(registerUser);

            // Assert
            _mockUserRepository.Verify(e => e.Insert(It.IsAny<User>()), Times.Once);
            _mockUnitOfWork.Verify(e => e.SaveChangesAsync(), Times.Once);
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

            _mockPasswordHelper.Setup(e => e.EncodePassword(password)).Returns(encodedPassword);

            var registerUser = new RegisterUserDto
            {
                Email = "1@2.com",
                Password = password,
            };

            // Act
            await _target.RegisterUserAsync(registerUser);

            // Assert
            _mockUserRepository.Verify(e => e.Insert(It.Is<User>(u => u.PasswordSalt == encodedPassword.Salt && u.PasswordHash == encodedPassword.Hash)), Times.Once);
            _mockUnitOfWork.Verify(e => e.SaveChangesAsync(), Times.Once);
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
            await _target.RegisterUserAsync(registerUser);

            // Assert
            _mockUserRepository.Verify(e => e.Insert(It.Is<User>(u => u.Email == email.ToLowerInvariant() && u.DisplayEmail == email)), Times.Once);
            _mockUnitOfWork.Verify(e => e.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public void RegisterUser_WhenSaveChangesFails_Throws()
        {
            // Arrange
            var errorMessage = "Unable to save.";
            _mockUnitOfWork.Setup(e => e.SaveChangesAsync()).ThrowsAsync(new Exception(errorMessage));
            var registerUser = new RegisterUserDto { Email = "1@2.com", Password = "123" };

            // Act
            Func<Task> act = async () => await _target.RegisterUserAsync(registerUser);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage(errorMessage);
        }
    }
}
