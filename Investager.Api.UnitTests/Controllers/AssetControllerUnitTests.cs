using AutoMapper;
using FluentAssertions;
using Investager.Api.Controllers;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Api.UnitTests.Controllers
{
    public class AssetControllerUnitTests
    {
        private readonly Mock<IDataProviderServiceFactory> _mockDataProviderServiceFactory = new Mock<IDataProviderServiceFactory>();
        private readonly Mock<IDataCollectionServiceFactory> _mockDataCollectionServiceFactory = new Mock<IDataCollectionServiceFactory>();
        private readonly Mock<ICoreUnitOfWork> _mockCoreUnitOfWork = new Mock<ICoreUnitOfWork>();
        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();

        private readonly Mock<IGenericRepository<Asset>> _mockAssetRepository = new Mock<IGenericRepository<Asset>>();

        private readonly AssetController _target;

        public AssetControllerUnitTests()
        {
            _mockCoreUnitOfWork.Setup(e => e.Assets).Returns(_mockAssetRepository.Object);

            _target = new AssetController(
                _mockDataProviderServiceFactory.Object,
                _mockDataCollectionServiceFactory.Object,
                _mockCoreUnitOfWork.Object,
                _mockMapper.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsExpectedDtos()
        {
            // Arrange
            var dto = new AssetSummaryDto
            {
                Symbol = "ZM",
                Exchange = "NASDAQ",
                Name = "Zoooom",
            };

            _mockAssetRepository.Setup(e => e.GetAll()).ReturnsAsync(new List<Asset>());
            _mockMapper.Setup(e => e.Map<IEnumerable<AssetSummaryDto>>(It.IsAny<object>())).Returns(new List<AssetSummaryDto> { dto });

            // Act
            var response = await _target.GetAll();

            // Assert
            var result = response as OkObjectResult;
            var value = result.Value as IEnumerable<AssetSummaryDto>;
            result.StatusCode.Should().Be(200);
            value.Count().Should().Be(1);
            value.First().Should().Be(dto);
        }

        [Fact]
        public async Task Scan_InvokesCorrectMethod()
        {
            // Arrange
            var serviceMock = new Mock<IDataProviderService>();
            _mockDataProviderServiceFactory.Setup(e => e.CreateService(It.IsAny<string>())).Returns(serviceMock.Object);

            // Act
            var response = await _target.Scan();

            // Assert
            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);
            serviceMock.Verify(e => e.ScanAssets(), Times.Once);
        }

        [Fact]
        public void Start_InvokesCorrectMethod()
        {
            // Arrange
            var serviceMock = new Mock<IDataCollectionService>();
            _mockDataCollectionServiceFactory.Setup(e => e.GetService(It.IsAny<string>())).Returns(serviceMock.Object);

            // Act
            var response = _target.Start();

            // Assert
            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);
            serviceMock.Verify(e => e.Start(), Times.Once);
        }

        [Fact]
        public void Stop_InvokesCorrectMethod()
        {
            // Arrange
            var serviceMock = new Mock<IDataCollectionService>();
            _mockDataCollectionServiceFactory.Setup(e => e.GetService(It.IsAny<string>())).Returns(serviceMock.Object);

            // Act
            var response = _target.Stop();

            // Assert
            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);
            serviceMock.Verify(e => e.Stop(), Times.Once);
        }
    }
}
