using FluentAssertions;
using Investager.Core.Models;
using Investager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Infrastructure.UnitTests.Persistence
{
    public class TimeSeriesRepositoryUnitTests
    {
        private readonly InvestagerTimeSeriesContext _context;
        private readonly Mock<IDbContextFactory<InvestagerTimeSeriesContext>> _mockContextFactory = new Mock<IDbContextFactory<InvestagerTimeSeriesContext>>();

        private readonly TimeSeriesRepository _target;

        public TimeSeriesRepositoryUnitTests()
        {
            var contextOptions = new DbContextOptionsBuilder<InvestagerTimeSeriesContext>()
                .UseSqlite("Filename=Test.db")
                .Options;

            _context = new InvestagerTimeSeriesContext(new Mock<IConfiguration>().Object, contextOptions);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _target = new TimeSeriesRepository(_context, _mockContextFactory.Object);
        }

        [Fact]
        public async Task InsertRange_WhenNoItems_NothingIsAdded()
        {
            // Act
            await _target.InsertRange(new List<TimeSeriesPoint>());

            // Assert
            var points = await _context.Set<TimeSeriesPoint>().ToListAsync();
            points.Count.Should().Be(0);
        }

        [Fact]
        public async Task InsertRange_WithOneItem_OneItemIsAdded()
        {
            // Arrange
            var pointsToAdd = new List<TimeSeriesPoint>
            {
                new TimeSeriesPoint
                {
                    Time = new DateTime(2021, 02, 02),
                    Key = "NASDAQ:ZM",
                    Value = 99.99f,
                },
            };

            // Act
            await _target.InsertRange(pointsToAdd);

            // Assert
            var points = await _context.Set<TimeSeriesPoint>().ToListAsync();
            points.Count.Should().Be(1);
            points.First().Should().BeEquivalentTo(pointsToAdd.First());
        }

        [Fact]
        public async Task InsertRange_WithMultipleItems_AllAreAdded()
        {
            // Arrange
            var listWithOnePoint = new List<TimeSeriesPoint>
            {
                new TimeSeriesPoint
                {
                    Time = new DateTime(2021, 02, 02),
                    Key = "NASDAQ:ZM",
                    Value = 99.99f,
                },
            };
            var listWithTwoPoints = new List<TimeSeriesPoint>
            {
                new TimeSeriesPoint
                {
                    Time = new DateTime(2021, 02, 01),
                    Key = "NASDAQ:ZM",
                    Value = 97.99f,
                },
                new TimeSeriesPoint
                {
                    Time = new DateTime(2021, 02, 03),
                    Key = "NYSE:PATH",
                    Value = 1001.01f,
                },
            };


            // Act
            await _target.InsertRange(listWithOnePoint);
            await _target.InsertRange(listWithTwoPoints);

            // Assert
            var points = await _context.Set<TimeSeriesPoint>().ToListAsync();
            points.Count.Should().Be(3);
            points.ElementAt(0).Should().BeEquivalentTo(listWithOnePoint.First());
            points.ElementAt(1).Should().BeEquivalentTo(listWithTwoPoints.First());
            points.ElementAt(2).Should().BeEquivalentTo(listWithTwoPoints.Last());
        }
    }
}
