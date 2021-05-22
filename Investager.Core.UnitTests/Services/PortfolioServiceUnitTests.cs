using FluentAssertions;
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
    public class PortfolioServiceUnitTests
    {
        private readonly Mock<ICoreUnitOfWork> _mockUnitOfWork = new Mock<ICoreUnitOfWork>();
        private readonly Mock<IGenericRepository<Portfolio>> _mockPortfolioRepository = new Mock<IGenericRepository<Portfolio>>();

        private readonly PortfolioService _target;

        public PortfolioServiceUnitTests()
        {
            _mockUnitOfWork.Setup(e => e.Portfolios).Returns(_mockPortfolioRepository.Object);

            _target = new PortfolioService(_mockUnitOfWork.Object);
        }

        [Fact]
        public void GetById_WhenPortfolioNotFound_Throws()
        {
            // Arrange
            _mockPortfolioRepository.Setup(e => e.Find(It.IsAny<Expression<Func<Portfolio, bool>>>(), It.IsAny<string>())).ReturnsAsync(new List<Portfolio>());

            // Act
            Func<Task> act = async () => await _target.GetById(1, 1);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("Sequence contains no elements");
        }

        [Fact]
        public async Task GetById_ReturnsPortfolio()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "portfolio",
                PortfolioAssets = new List<PortfolioAsset>
                {
                    new PortfolioAsset { PortfolioId = 1, AssetId = 3 },
                    new PortfolioAsset { PortfolioId = 1, AssetId = 11 },
                },
                UserId = 1,
            };

            _mockPortfolioRepository.Setup(e => e.Find(It.IsAny<Expression<Func<Portfolio, bool>>>(), It.IsAny<string>())).ReturnsAsync(new List<Portfolio> { portfolio });

            // Act
            var portfolioDto = await _target.GetById(1, 1);

            // Assert
            portfolioDto.Name.Should().Be(portfolio.Name);
            portfolioDto.AssetIds.Should().Contain(portfolio.PortfolioAssets.First().AssetId);
            portfolioDto.AssetIds.Should().Contain(portfolio.PortfolioAssets.Last().AssetId);
        }

        [Fact]
        public async Task GetAll_WhenPortfoliosNotFound_ReturnsEmptyList()
        {
            // Arrange
            _mockPortfolioRepository.Setup(e => e.Find(It.IsAny<Expression<Func<Portfolio, bool>>>(), It.IsAny<string>())).ReturnsAsync(new List<Portfolio>());

            // Act
            var portfolios = await _target.GetAll(1);

            // Assert
            portfolios.Count().Should().Be(0);
        }

        [Fact]
        public async Task GetAll_ReturnsPortfolios()
        {
            // Arrange
            var portfolio1 = new Portfolio
            {
                Id = 1,
                Name = "portfolio1",
                PortfolioAssets = new List<PortfolioAsset>
                {
                    new PortfolioAsset { PortfolioId = 1, AssetId = 3 },
                    new PortfolioAsset { PortfolioId = 1, AssetId = 11 },
                },
                UserId = 1,
            };

            var portfolio2 = new Portfolio
            {
                Id = 2,
                Name = "portfolio2",
                PortfolioAssets = new List<PortfolioAsset>
                {
                    new PortfolioAsset { PortfolioId = 1, AssetId = 11 },
                    new PortfolioAsset { PortfolioId = 1, AssetId = 15 },
                },
                UserId = 1,
            };

            _mockPortfolioRepository.Setup(e => e.Find(It.IsAny<Expression<Func<Portfolio, bool>>>(), It.IsAny<string>())).ReturnsAsync(new List<Portfolio> { portfolio1, portfolio2 });

            // Act
            var portfolioDtos = await _target.GetAll(1);

            // Assert
            portfolioDtos.Count().Should().Be(2);
            portfolioDtos.First().AssetIds.Should().Contain(portfolio1.PortfolioAssets.First().AssetId);
            portfolioDtos.First().AssetIds.Should().Contain(portfolio1.PortfolioAssets.Last().AssetId);
            portfolioDtos.Last().AssetIds.Should().Contain(portfolio2.PortfolioAssets.First().AssetId);
            portfolioDtos.Last().AssetIds.Should().Contain(portfolio2.PortfolioAssets.Last().AssetId);
        }
    }
}
