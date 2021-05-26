using AutoMapper;
using FluentAssertions;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Core.UnitTests.Services
{
    public class AssetServiceUnitTests
    {
        private readonly Mock<ICoreUnitOfWork> _mockCoreUnitOfWork = new Mock<ICoreUnitOfWork>();
        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
        private readonly Mock<ICache> _mockCache = new Mock<ICache>();

        private readonly Mock<IGenericRepository<Asset>> _mockAssetRepository = new Mock<IGenericRepository<Asset>>();

        private readonly AssetService _target;

        public AssetServiceUnitTests()
        {
            _mockCoreUnitOfWork.Setup(e => e.Assets).Returns(_mockAssetRepository.Object);

            _target = new AssetService(_mockCoreUnitOfWork.Object, _mockMapper.Object, _mockCache.Object);
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

            _mockCache.Setup(e => e.Get(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>())).ReturnsAsync(new List<AssetSummaryDto> { dto });

            // Act
            var response = await _target.GetAll();

            // Assert
            response.Count().Should().Be(1);
            response.Single().Should().Be(dto);
            _mockCache.Verify(e => e.Get("AssetDtos", TimeSpan.FromMinutes(30), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()), Times.Once);
        }
    }
}
