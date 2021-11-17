using FluentAssertions;
using Investager.Core.Models;
using Investager.Infrastructure.Logging;
using Investager.Infrastructure.Models;
using Investager.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Infrastructure.UnitTests.Logging
{
    public class LokiSinkUniTests
    {
        private const string EnvironmentName = "Dev";
        private const string LogMessage = "ja kena nis";

        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        private readonly Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
        private readonly Mock<ILokiFormatter> _mockLokiFormatter = new Mock<ILokiFormatter>();
        private readonly LokiSettings _lokiSettings = new LokiSettings { BatchInterval = TimeSpan.FromMilliseconds(100) };

        private LokiSink _target;

        [Fact]
        public async Task WhenNoLogsAreSent_NoHttpRequestsAreMade()
        {
            // Arrange
            CreateTarget();

            // Act
            await Task.Delay(300);

            // Assert
            _mockHttpMessageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Never(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task WhenOneLogSent_OnlyOneBatchSent()
        {
            // Arrange
            var logEvent = BuildEvent(new DateTime(1985, 10, 10, 0, 0, 0, DateTimeKind.Utc));
            string httpContent = null;

            CreateTarget();

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent,
                Content = new StringContent(""),
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback((HttpRequestMessage message, CancellationToken token) => httpContent = message.Content.ReadAsStringAsync().Result)
                .ReturnsAsync(response);

            // Act
            _target.Emit(logEvent);
            await Task.Delay(300);

            // Assert
            var expectedRequest = new LokiRequest
            {
                Streams = new List<LokiStream>
                {
                    new LokiStream
                    {
                        Stream = new Dictionary<string, string>
                        {
                            { "host", Dns.GetHostName() },
                            { "env", EnvironmentName }
                        },
                        Values = new List<string[]>
                        {
                            new string[2] { "497750400000000000", LogMessage }
                        },
                    }
                }
            };
            var expectedContent = JsonConvert.SerializeObject(expectedRequest);

            httpContent.Should().Be(expectedContent);
            _mockHttpMessageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(e => e.RequestUri == new Uri("http://www.fake.com/loki/api/v1/push")
                        && e.Method == HttpMethod.Post
                        && e.Content.Headers.ContentType.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task WhenEventsExceedOneBatch_MultipleAreSent()
        {
            // Arrange
            _lokiSettings.BatchInterval = TimeSpan.FromMilliseconds(300);
            _lokiSettings.MaxBatchSize = 2;

            CreateTarget();

            // Act
            _target.Emit(BuildEvent(DateTime.UtcNow));
            _target.Emit(BuildEvent(DateTime.UtcNow));
            _target.Emit(BuildEvent(DateTime.UtcNow));
            await Task.Delay(500);

            // Assert
            _mockHttpMessageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Exactly(2),
                    ItExpr.Is<HttpRequestMessage>(e => e.RequestUri == new Uri("http://www.fake.com/loki/api/v1/push")
                        && e.Method == HttpMethod.Post
                        && e.Content.Headers.ContentType.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
        }

        private void CreateTarget()
        {
            _lokiSettings.BatchInterval = TimeSpan.FromMilliseconds(100);

            _mockConfiguration.Setup(e => e[ConfigKeys.Environment]).Returns(EnvironmentName);

            _mockLokiFormatter.Setup(e => e.Format(It.IsAny<LogEvent>())).Returns(LogMessage);

            var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://www.fake.com")
            };

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent,
                Content = new StringContent(""),
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            _target = new LokiSink(
                _mockConfiguration.Object,
                _mockLokiFormatter.Object,
                mockHttpClientFactory.Object,
                _lokiSettings);
        }

        private LogEvent BuildEvent(DateTime dateTime)
        {
            return new LogEvent(
                dateTime,
                LogEventLevel.Information,
                null,
                new MessageTemplate(new List<MessageTemplateToken>()),
                new List<LogEventProperty>());
        }
    }
}
