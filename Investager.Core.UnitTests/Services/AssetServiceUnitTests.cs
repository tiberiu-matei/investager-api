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
using System.Linq.Expressions;
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
        private readonly Mock<IGenericRepository<UserStarredAsset>> _mockUserStarredAssetRepository = new Mock<IGenericRepository<UserStarredAsset>>();

        private readonly AssetService _target;

        public AssetServiceUnitTests()
        {
            _mockCoreUnitOfWork.Setup(e => e.Assets).Returns(_mockAssetRepository.Object);
            _mockCoreUnitOfWork.Setup(e => e.UserStarredAssets).Returns(_mockUserStarredAssetRepository.Object);

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

            _mockCache.Setup(e => e.Get(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()))
                .ReturnsAsync(new List<AssetSummaryDto> { dto });

            // Act
            var response = await _target.GetAll();

            // Assert
            response.Count().Should().Be(1);
            response.Single().Should().Be(dto);
            _mockCache.Verify(e => e.Get("AssetDtos", TimeSpan.FromMinutes(30), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()), Times.Once);
        }

        [Fact]
        public void GetStarred_WhenRepositoryThrows_Throws()
        {
            // Arrange
            var errorMessage = "big oof";
            _mockUserStarredAssetRepository.Setup(e => e.Find(It.IsAny<Expression<Func<UserStarredAsset, bool>>>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            Func<Task> act = async () => await _target.GetStarred(5);

            // Assert
            act.Should().Throw<Exception>().WithMessage(errorMessage);
        }

        [Fact]
        public async Task GetStarred_WhenNoEntries_ReturnsEmptyList()
        {
            // Arrange
            var userId = 5;
            _mockUserStarredAssetRepository.Setup(e => e.Find(It.IsAny<Expression<Func<UserStarredAsset, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<UserStarredAsset>());

            // Act
            var response = await _target.GetStarred(userId);

            // Assert
            response.Any().Should().BeFalse();
            _mockUserStarredAssetRepository.Verify(e => e.Find(It.IsAny<Expression<Func<UserStarredAsset, bool>>>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetStarred_ReturnsExpectedItems()
        {
            // Arrange
            var userId = 5;
            var userStarredAsset1 = new UserStarredAsset
            {
                UserId = userId,
                AssetId = 101,
                DisplayOrder = 51,
            };

            var userStarredAsset2 = new UserStarredAsset
            {
                UserId = userId,
                AssetId = 385,
                DisplayOrder = 1,
            };

            _mockUserStarredAssetRepository.Setup(e => e.Find(It.IsAny<Expression<Func<UserStarredAsset, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<UserStarredAsset> { userStarredAsset1, userStarredAsset2 });

            // Act
            var response = await _target.GetStarred(userId);

            // Assert
            response.Count().Should().Be(2);

            var first = response.Single(e => e.AssetId == userStarredAsset1.AssetId);
            first.DisplayOrder.Should().Be(userStarredAsset1.DisplayOrder);

            var second = response.Single(e => e.AssetId == userStarredAsset2.AssetId);
            second.DisplayOrder.Should().Be(userStarredAsset2.DisplayOrder);

            _mockUserStarredAssetRepository.Verify(e => e.Find(It.IsAny<Expression<Func<UserStarredAsset, bool>>>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Star_WhenRepositoryThrows_Throws()
        {
            // Arrange
            var errorMessage = "big oof";
            _mockCoreUnitOfWork.Setup(e => e.SaveChanges())
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            Func<Task> act = async () => await _target.Star(5, new StarAssetRequest());

            // Assert
            act.Should().Throw<Exception>().WithMessage(errorMessage);
        }

        [Fact]
        public async Task Star_SavesCorrectData()
        {
            // Arrange
            var userId = 5;
            var request = new StarAssetRequest
            {
                AssetId = 385,
                DisplayOrder = 122,
            };

            // Act
            await _target.Star(userId, request);

            // Assert
            _mockUserStarredAssetRepository.Verify(x => x.Insert(
                It.Is<UserStarredAsset>(e => e.AssetId == request.AssetId && e.UserId == userId && e.DisplayOrder == request.DisplayOrder)),
                    Times.Once);
            _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpdateDisplaySortOrder_WhenEntryNotFound_Throws()
        {
            // Arrange
            var userId = 5;
            var request = new StarAssetRequest
            {
                AssetId = 385,
                DisplayOrder = 51,
            };

            _mockUserStarredAssetRepository.Setup(e => e.FindWithTracking(It.IsAny<Expression<Func<UserStarredAsset, bool>>>()))
                .ReturnsAsync(new List<UserStarredAsset>());

            // Act
            Func<Task> act = async () => await _target.UpdateStarDisplayOrder(userId, request);

            // Assert
            act.Should().Throw<Exception>();
            _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Never);
        }

        [Fact]
        public async Task UpdateDisplaySortOrder_UpdatesOrder()
        {
            // Arrange
            var userId = 5;
            var assetId = 385;
            var userStarredAsset = new UserStarredAsset
            {
                UserId = userId,
                AssetId = assetId,
                DisplayOrder = 51,
            };

            var request = new StarAssetRequest
            {
                AssetId = assetId,
                DisplayOrder = 222,
            };

            _mockUserStarredAssetRepository.Setup(e => e.FindWithTracking(It.IsAny<Expression<Func<UserStarredAsset, bool>>>()))
                .ReturnsAsync(new List<UserStarredAsset> { userStarredAsset });

            // Act
            await _target.UpdateStarDisplayOrder(userId, request);

            // Assert
            userStarredAsset.DisplayOrder.Should().Be(request.DisplayOrder);
            _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Unstar_WhenRepositoryThrows_Throws()
        {
            // Arrange
            var errorMessage = "big oof";
            var userId = 5;

            var userStarredAsset = new UserStarredAsset
            {
                UserId = userId,
                AssetId = 385,
                DisplayOrder = 51,
            };

            _mockUserStarredAssetRepository.Setup(e => e.Find(It.IsAny<Expression<Func<UserStarredAsset, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<UserStarredAsset> { userStarredAsset });

            _mockCoreUnitOfWork.Setup(e => e.SaveChanges())
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            Func<Task> act = async () => await _target.Unstar(userId, userStarredAsset.AssetId);

            // Assert
            act.Should().Throw<Exception>().WithMessage(errorMessage);
        }

        [Fact]
        public void Unstar_WhenEntryNotFound_Throws()
        {
            // Arrange
            var userId = 5;
            var assetId = 385;

            _mockUserStarredAssetRepository.Setup(e => e.Find(It.IsAny<Expression<Func<UserStarredAsset, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<UserStarredAsset>());

            // Act
            Func<Task> act = async () => await _target.Unstar(userId, assetId);

            // Assert
            act.Should().Throw<Exception>();
            _mockUserStarredAssetRepository.Verify(x => x.Delete(It.IsAny<UserStarredAsset>()), Times.Never);
            _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Never);
        }

        [Fact]
        public async Task Unstar_RemovesCorrectEntry()
        {
            // Arrange
            var userId = 5;
            var userStarredAsset = new UserStarredAsset
            {
                UserId = userId,
                AssetId = 385,
                DisplayOrder = 51,
            };

            _mockUserStarredAssetRepository.Setup(e => e.Find(It.IsAny<Expression<Func<UserStarredAsset, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<UserStarredAsset> { userStarredAsset });

            // Act
            await _target.Unstar(userId, userStarredAsset.AssetId);

            // Assert
            _mockUserStarredAssetRepository.Verify(x => x.Delete(
                It.Is<UserStarredAsset>(e => e.AssetId == userStarredAsset.AssetId && e.UserId == userId && e.DisplayOrder == userStarredAsset.DisplayOrder)),
                    Times.Once);
            _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);
        }
    }
}
