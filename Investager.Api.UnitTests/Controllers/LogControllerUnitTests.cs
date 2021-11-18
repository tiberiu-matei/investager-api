using Investager.Api.Controllers;
using Investager.Infrastructure.Logging;
using Investager.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace Investager.Api.UnitTests.Controllers;

public class LogControllerUnitTests
{
    private readonly Mock<ILogger<LogController>> _mockLogger = new Mock<ILogger<LogController>>();

    private readonly LogController _target;

    public LogControllerUnitTests()
    {
        _target = new LogController(_mockLogger.Object);
    }

    [Theory]
    [InlineData(UILogLevel.Information, LogLevel.Information)]
    [InlineData(UILogLevel.Debug, LogLevel.Debug)]
    [InlineData(UILogLevel.Warning, LogLevel.Warning)]
    [InlineData(UILogLevel.Error, LogLevel.Error)]
    public void Log_SecondsCorrectMessage(UILogLevel input, LogLevel expected)
    {
        // Arrange
        var request = new LogRequest
        {
            Level = input,
            Message = "ti don",
        };

        // Act
        _target.Log(request);

        // Assert
        _mockLogger.Verify(e => e.Log(
            expected,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((x, d) => x.ToString() == request.Message),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }
}
