using FluentAssertions;
using Investager.Infrastructure.Logging;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using Xunit;

namespace Investager.Infrastructure.UnitTests.Logging
{
    public class LokiFormatterUnitTests
    {
        private readonly MessageTemplateParser _messageTemplateParser = new MessageTemplateParser();

        private readonly LokiFormatter _target;

        public LokiFormatterUnitTests()
        {
            _target = new LokiFormatter();
        }

        [Fact]
        public void Format_WithSimpleMessage_GeneratesCorrectJson()
        {
            // Arrange
            var logEvent = new LogEvent(
                DateTime.UtcNow,
                LogEventLevel.Warning,
                null,
                _messageTemplateParser.Parse("bad things happened"),
                new List<LogEventProperty>());

            // Act
            var formatted = _target.Format(logEvent);

            // Assert
            var expected = "{\"@mt\":\"bad things happened\",\"@l\":\"Warning\"}" + Environment.NewLine;
            formatted.Should().Be(expected);
        }
    }
}
