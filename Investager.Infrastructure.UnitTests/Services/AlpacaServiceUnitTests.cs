using FluentAssertions;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Core.Services;
using Investager.Infrastructure.Services;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Infrastructure.UnitTests.Services
{
    public class AlpacaServiceUnitTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        private readonly Mock<ICoreUnitOfWork> _mockCoreUnitOfWork = new Mock<ICoreUnitOfWork>();
        private readonly Mock<IGenericRepository<Asset>> _mockAssetRepository = new Mock<IGenericRepository<Asset>>();
        private readonly Mock<ITimeSeriesRepository> _mockTimeSeriesRepository = new Mock<ITimeSeriesRepository>();
        private readonly Mock<ITimeHelper> _mockTimeHelper = new Mock<ITimeHelper>();
        private readonly Mock<ICache> _mockCache = new Mock<ICache>();
        private readonly Asset _asset;

        private readonly AlpacaService _target;

        public AlpacaServiceUnitTests()
        {
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://www.fake.com")
            };

            _mockTimeHelper.Setup(e => e.GetUtcNow()).Returns(DateTime.Now);

            _asset = new Asset
            {
                Id = 3,
                Exchange = "NASDAQ",
                Symbol = "ZM",
                Provider = DataProviders.Alpaca,
            };

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(httpClient);

            _mockAssetRepository.Setup(e => e.GetAll()).ReturnsAsync(new List<Asset>());

            _mockCoreUnitOfWork.Setup(e => e.Assets).Returns(_mockAssetRepository.Object);

            _target = new AlpacaService(
                mockHttpClientFactory.Object,
                _mockCoreUnitOfWork.Object,
                _mockTimeSeriesRepository.Object,
                _mockTimeHelper.Object,
                _mockCache.Object);
        }

        [Fact]
        public async Task ScanAssets_MakesCorrectInserts()
        {
            var responseContent = await File.ReadAllTextAsync("TestData/AlpacaGetAssetsExample.json");
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent),
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var result = await _target.ScanAssets();

            result.ToList().Count.Should().Be(4);
            _mockAssetRepository.Verify(e => e.Insert(It.Is<Asset>(e => e.Symbol == "NES" && e.Exchange == "AMEX")), Times.Once);
            _mockAssetRepository.Verify(e => e.Insert(It.Is<Asset>(e => e.Symbol == "NETE" && e.Exchange == "NASDAQ")), Times.Once);
            _mockAssetRepository.Verify(e => e.Insert(It.Is<Asset>(e => e.Symbol == "NETI" && e.Exchange == "NYSE")), Times.Once);
            _mockAssetRepository.Verify(e => e.Insert(It.Is<Asset>(e => e.Symbol == "NETL" && e.Exchange == "ARCA")), Times.Once);
            _mockAssetRepository.Verify(e => e.Insert(It.IsAny<Asset>()), Times.Exactly(4));
            _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task ScanAssets_WhenAssetsExist_DoesNoInserts()
        {
            var responseContent = await File.ReadAllTextAsync("TestData/AlpacaGetAssetsExample.json");
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent),
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var existingAssets = new List<Asset>
            {
                new Asset { Symbol = "NES", Exchange = "AMEX" },
                new Asset { Symbol = "NETE", Exchange = "NASDAQ" },
                new Asset { Symbol = "NETI", Exchange = "NYSE" },
                new Asset { Symbol = "NETL", Exchange = "ARCA" },
            };

            _mockAssetRepository.Setup(e => e.GetAll()).ReturnsAsync(existingAssets);

            var result = await _target.ScanAssets();

            result.ToList().Count.Should().Be(0);
            _mockAssetRepository.Verify(e => e.Insert(It.IsAny<Asset>()), Times.Never);
            _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Never);
        }

        [Fact]
        public void ScanAssets_WhenAlpacaCallFails_Throws()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.TooManyRequests,
                Content = new StringContent("Retry back later."),
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            Func<Task> act = async () => await _target.ScanAssets();

            act.Should().Throw<HttpRequestException>();
            _mockAssetRepository.Verify(e => e.Insert(It.IsAny<Asset>()), Times.Never);
            _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Never);
        }

        [Fact]
        public void UpdateTimeSeriesData_WhenAlpacaCallFails_Throws()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.TooManyRequests,
                Content = new StringContent("Retry back later."),
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            Func<Task> act = async () => await _target.UpdateTimeSeriesData(_asset);

            // Assert
            act.Should().Throw<HttpRequestException>();
            _mockTimeSeriesRepository.Verify(e => e.InsertRange(It.IsAny<IEnumerable<TimeSeriesPoint>>()), Times.Never);
            _mockCache.Verify(e => e.Clear(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTimeSeriesData_AlpacaUrl_IsBuiltCorrectly_AndLastPriceUpdatedChanged()
        {
            // Arrange
            var timeNow = new DateTime(2021, 04, 11, 12, 16, 33, DateTimeKind.Utc);
            _mockTimeHelper.Setup(e => e.GetUtcNow()).Returns(timeNow);

            var responseContent = File.ReadAllText("TestData/AlpacaGetStockDataExample.json");
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent),
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            await _target.UpdateTimeSeriesData(_asset);

            // Assert
            _mockHttpMessageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(e => e.RequestUri != null && e.RequestUri.ToString().Contains($"stocks/{_asset.Symbol}/bars?start=2016-04-11T12:17:33.0000000Z&end=2021-04-11T11:16:33.0000000Z&timeframe=1Day&limit=10000")),
                    ItExpr.IsAny<CancellationToken>());

            _mockAssetRepository.Verify(e => e.Update(It.Is<Asset>(e => e.LastPriceUpdate == new DateTime(2021, 04, 11, 11, 16, 33, DateTimeKind.Utc))), Times.Once);
            _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task UpdateTimeSeriesData_ExpectedDataPoints_GetInsertedToTimeSeries()
        {
            // Arrange
            var timeNow = new DateTime(2021, 04, 11, 12, 16, 33, DateTimeKind.Utc);
            _mockTimeHelper.Setup(e => e.GetUtcNow()).Returns(timeNow);

            var responseContent = File.ReadAllText("TestData/AlpacaGetStockDataExample.json");
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent),
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            await _target.UpdateTimeSeriesData(_asset);

            // Assert
            _mockHttpMessageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(e => e.RequestUri != null && e.RequestUri.ToString().Contains($"stocks/{_asset.Symbol}/bars?start=2016-04-11T12:17:33.0000000Z&end=2021-04-11T11:16:33.0000000Z&timeframe=1Day&limit=10000")),
                    ItExpr.IsAny<CancellationToken>());

            _mockTimeSeriesRepository.Verify(e => e.InsertRange(It.Is<IEnumerable<TimeSeriesPoint>>(
                e => e.Count() == 5 &&
                e.All(e => e.Key == "NASDAQ:ZM") &&
                e.ToList().ElementAt(0).Time == new DateTime(2016, 04, 12, 04, 00, 00, DateTimeKind.Utc) && e.ToList().ElementAt(0).Value == 12.81F &&
                e.ToList().ElementAt(1).Time == new DateTime(2016, 04, 13, 04, 00, 00, DateTimeKind.Utc) && e.ToList().ElementAt(1).Value == 13.06F &&
                e.ToList().ElementAt(2).Time == new DateTime(2016, 04, 14, 04, 00, 00, DateTimeKind.Utc) && e.ToList().ElementAt(2).Value == 13.09F &&
                e.ToList().ElementAt(3).Time == new DateTime(2016, 04, 15, 04, 00, 00, DateTimeKind.Utc) && e.ToList().ElementAt(3).Value == 12.94F &&
                e.ToList().ElementAt(4).Time == new DateTime(2016, 04, 18, 04, 00, 00, DateTimeKind.Utc) && e.ToList().ElementAt(4).Value == 13.25F)),
                    Times.Once);

            _mockTimeSeriesRepository.Verify(e => e.InsertRange(It.IsAny<IEnumerable<TimeSeriesPoint>>()), Times.Once);
            _mockCache.Verify(e => e.Clear("NASDAQ:ZM"), Times.Once);
        }
    }
}
