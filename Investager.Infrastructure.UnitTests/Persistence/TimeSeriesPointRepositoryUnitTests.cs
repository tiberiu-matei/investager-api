using FluentAssertions;
using Investager.Core.Models;
using Investager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Infrastructure.UnitTests.Persistence
{
    public class TimeSeriesPointRepositoryUnitTests
    {
        private readonly InvestagerTimeSeriesContext _context;

        private readonly TimeSeriesPointRepository _target;

        public TimeSeriesPointRepositoryUnitTests()
        {
            var contextOptions = new DbContextOptionsBuilder<InvestagerTimeSeriesContext>()
                .UseSqlite("Filename=Test.db")
                .Options;

            _context = new InvestagerTimeSeriesContext(contextOptions);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _target = new TimeSeriesPointRepository(_context);
        }

        [Fact]
        public void Get_UntilCurrentTime_Throws()
        {
            // Act
            Func<Task> act = async () => await _target.Get("fake", new DateTime(2021, 01, 01));

            // Assert
            act.Should().Throw<NotImplementedException>();
        }

        [Fact]
        public void Get_WithTimeRange_Throws()
        {
            // Act
            Func<Task> act = async () => await _target.Get("fake", new DateTime(2021, 01, 01), new DateTime(2021, 03, 01));

            // Assert
            act.Should().Throw<NotImplementedException>();
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
