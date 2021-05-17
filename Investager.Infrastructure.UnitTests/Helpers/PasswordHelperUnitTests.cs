using FluentAssertions;
using Investager.Infrastructure.Helpers;
using System;
using Xunit;

namespace Investager.Infrastructure.UnitTests.Helpers
{
    public class PasswordHelperUnitTests
    {
        private readonly PasswordHelper _target;

        public PasswordHelperUnitTests()
        {
            _target = new PasswordHelper();
        }

        [Fact]
        public void EncodePassword_ForSamePassword_GeneratesUniqueHash()
        {
            // Arrange
            var password = "secure";

            // Act
            var firstEncoding = _target.EncodePassword(password);
            var secondEncoding = _target.EncodePassword(password);

            // Assert
            firstEncoding.Hash.Should().NotBeEquivalentTo(secondEncoding.Hash);
        }

        [Fact]
        public void IsPasswordValid_WhenPasswordCorrect_ReturnsTrue()
        {
            // Arrange
            var password = "secure";
            var encodedPassword = _target.EncodePassword(password);

            // Act
            var passwordValid = _target.IsPasswordCorrect(password, encodedPassword.Hash, encodedPassword.Salt);

            // Assert
            passwordValid.Should().BeTrue();
        }

        [Fact]
        public void IsPasswordValid_WhenPasswordIncorrect_ReturnsFalse()
        {
            // Arrange
            var password = "secure";
            var encodedPassword = _target.EncodePassword(password);

            // Act
            var passwordValid = _target.IsPasswordCorrect($"{password}1", encodedPassword.Hash, encodedPassword.Salt);

            // Assert
            passwordValid.Should().BeFalse();
        }

        [Fact]
        public void IsPasswordValid_WhenSaltNot128Bytes_Throws()
        {
            // Arrange
            var password = "secure";
            var encodedPassword = _target.EncodePassword(password);
            encodedPassword.Salt = new byte[10];

            // Act
            Action act = () => _target.IsPasswordCorrect(password, encodedPassword.Hash, encodedPassword.Salt);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Invalid length of password salt or hash.");
        }

        [Fact]
        public void IsPasswordValid_WhenHashNot64Bytes_Throws()
        {
            // Arrange
            var password = "secure";
            var encodedPassword = _target.EncodePassword(password);
            encodedPassword.Hash = new byte[10];

            // Act
            Action act = () => _target.IsPasswordCorrect(password, encodedPassword.Hash, encodedPassword.Salt);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Invalid length of password salt or hash.");
        }
    }
}
