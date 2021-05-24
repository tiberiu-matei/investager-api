using FluentAssertions;
using Investager.Api.Controllers;
using Investager.Core.Constants;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Api.UnitTests.Controllers
{
    public class PortfolioControllerUnitTests
    {
        private readonly Mock<IPortfolioService> _mockPortfolioService = new Mock<IPortfolioService>();
        private readonly HttpContext _httpContext = new DefaultHttpContext();

        private readonly PortfolioController _target;

        public PortfolioControllerUnitTests()
        {
            _target = new PortfolioController(_mockPortfolioService.Object);
            _target.ControllerContext.HttpContext = _httpContext;
        }

        [Fact]
        public void GetById_WhenUserIdNotPresent_Throws()
        {
            // Act
            Func<Task> act = async () => await _target.GetById(1);

            // Assert
            act.Should().Throw<Exception>();
            _mockPortfolioService.Verify(e => e.GetById(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetById_ReturnsExpectedData()
        {
            // Arrange
            var portfolioId = 2;
            var userId = 3;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();

            var dto = new PortfolioDto
            {
                Id = portfolioId,
                Name = "jake_jake",
                AssetIds = new List<int> { 9, 77 },
            };

            _mockPortfolioService.Setup(e => e.GetById(userId, portfolioId)).ReturnsAsync(dto);

            // Act
            var response = await _target.GetById(portfolioId);
            var result = response as OkObjectResult;
            var value = result.Value as PortfolioDto;

            // Assert
            result.StatusCode.Should().Be(200);
            value.Should().Be(dto);
            _mockPortfolioService.Verify(e => e.GetById(userId, portfolioId), Times.Once);
        }

        [Fact]
        public void GetAll_WhenUserIdNotPresent_Throws()
        {
            // Act
            Func<Task> act = async () => await _target.GetAll();

            // Assert
            act.Should().Throw<Exception>();
            _mockPortfolioService.Verify(e => e.GetAll(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetAll_ReturnsExpectedData()
        {
            // Arrange
            var userId = 3;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();

            var dto1 = new PortfolioDto
            {
                Id = 2,
                Name = "jake_jake",
                AssetIds = new List<int> { 9, 77 },
            };

            var dto2 = new PortfolioDto
            {
                Id = 385,
                Name = "jake_jake",
                AssetIds = new List<int> { 311, 104 },
            };

            _mockPortfolioService.Setup(e => e.GetAll(userId)).ReturnsAsync(new List<PortfolioDto> { dto1, dto2 });

            // Act
            var response = await _target.GetAll();
            var result = response as OkObjectResult;
            var value = result.Value as IEnumerable<PortfolioDto>;

            // Assert
            result.StatusCode.Should().Be(200);
            value.First().Should().Be(dto1);
            value.Last().Should().Be(dto2);
            _mockPortfolioService.Verify(e => e.GetAll(userId), Times.Once);
        }

        [Fact]
        public void Create_WhenUserIdNotPresent_Throws()
        {
            // Act
            Func<Task> act = async () => await _target.Create(new UpdatePortfolioDto());

            // Assert
            act.Should().Throw<Exception>();
            _mockPortfolioService.Verify(e => e.Create(It.IsAny<int>(), It.IsAny<UpdatePortfolioDto>()), Times.Never);
        }

        [Fact]
        public async Task Create_ReturnsExpectedData()
        {
            // Arrange
            var portfolioId = 2;
            var userId = 3;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();

            var request = new UpdatePortfolioDto
            {
                Name = "jake_jake",
                AssetIds = new List<int> { 91, 90 },
            };

            var dto = new PortfolioDto
            {
                Id = portfolioId,
                Name = request.Name,
                AssetIds = request.AssetIds,
            };

            _mockPortfolioService.Setup(e => e.Create(userId, request)).ReturnsAsync(dto);

            // Act
            var response = await _target.Create(request);
            var result = response as OkObjectResult;
            var value = result.Value as PortfolioDto;

            // Assert
            result.StatusCode.Should().Be(200);
            value.Should().Be(dto);
            _mockPortfolioService.Verify(e => e.Create(userId, request), Times.Once);
        }

        [Fact]
        public void Update_WhenUserIdNotPresent_Throws()
        {
            // Act
            Func<Task> act = async () => await _target.Update(1, new UpdatePortfolioDto());

            // Assert
            act.Should().Throw<Exception>();
            _mockPortfolioService.Verify(e => e.Update(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UpdatePortfolioDto>()), Times.Never);
        }

        [Fact]
        public async Task Update_CallsService()
        {
            // Arrange
            var portfolioId = 2;
            var userId = 3;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();

            var request = new UpdatePortfolioDto
            {
                Name = "jake_jake",
                AssetIds = new List<int> { 91, 90 },
            };

            // Act
            var response = await _target.Update(portfolioId, request);
            var result = response as NoContentResult;

            // Assert
            result.StatusCode.Should().Be(204);
            _mockPortfolioService.Verify(e => e.Update(userId, portfolioId, request), Times.Once);
        }

        [Fact]
        public void Delete_WhenUserIdNotPresent_Throws()
        {
            // Act
            Func<Task> act = async () => await _target.Delete(1);

            // Assert
            act.Should().Throw<Exception>();
            _mockPortfolioService.Verify(e => e.Delete(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Delete_CallsService()
        {
            // Arrange
            var portfolioId = 2;
            var userId = 3;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();

            // Act
            var response = await _target.Delete(portfolioId);
            var result = response as NoContentResult;

            // Assert
            result.StatusCode.Should().Be(204);
            _mockPortfolioService.Verify(e => e.Delete(userId, portfolioId), Times.Once);
        }
    }
}
