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

namespace Investager.Infrastructure.UnitTests.Persistence;

public class TimeSeriesRepositoryUnitTests
{
    private readonly InvestagerTimeSeriesContext _context;
    private readonly InvestagerTimeSeriesContext _deleteContext;
    private readonly Mock<IDbContextFactory<InvestagerTimeSeriesContext>> _mockContextFactory
        = new Mock<IDbContextFactory<InvestagerTimeSeriesContext>>();

    private readonly TimeSeriesRepository _target;

    public TimeSeriesRepositoryUnitTests()
    {
        var contextOptions = new DbContextOptionsBuilder<InvestagerTimeSeriesContext>()
            .UseSqlite("Filename=TestTimeSeries.db")
            .Options;

        _context = new InvestagerTimeSeriesContext(new Mock<IConfiguration>().Object, contextOptions);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _mockContextFactory
            .Setup(e => e.CreateDbContext())
            .Returns(_context);

        _deleteContext = new InvestagerTimeSeriesContext(new Mock<IConfiguration>().Object, contextOptions);

        _target = new TimeSeriesRepository(_context, _mockContextFactory.Object);
    }

    [Fact]
    public async Task Get_ReturnsEmptyList_WhenNoEntries()
    {
        // Arrange
        var key = "NASDAQ:SE";

        // Act
        var response = await _target.Get(key);

        // Assert
        response.Key.Should().Be(key);
        response.Points.Any().Should().BeFalse();
    }

    [Fact]
    public async Task Get_ReturnsOrderedPoints()
    {
        // Arrange
        var key = "NASDAQ:SE";
        var point1 = new TimeSeriesPoint
        {
            Key = key,
            Time = new DateTime(1985, 10, 10, 13, 37, 00),
            Value = 38.5f,
        };

        var point2 = new TimeSeriesPoint
        {
            Key = key,
            Time = new DateTime(2001, 10, 10, 13, 37, 00),
            Value = 22.2f,
        };

        var point3 = new TimeSeriesPoint
        {
            Key = key,
            Time = new DateTime(1988, 10, 10, 13, 37, 00),
            Value = 13.7f,
        };

        await _target.InsertRange(new List<TimeSeriesPoint>
            {
                point1,
                point2,
                point3,
            });

        // Act
        var response = await _target.Get(key);

        // Assert
        response.Key.Should().Be(key);
        response.Points.Count.Should().Be(3);

        var responsePointsArray = response.Points.ToArray();
        responsePointsArray[0].Time.Should().Be(point2.Time);
        responsePointsArray[1].Time.Should().Be(point3.Time);
        responsePointsArray[2].Time.Should().Be(point1.Time);
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

    [Fact]
    public async Task DeleteSeries_WhenMultiplePointsExist_RemovesAllPoints()
    {
        // Arrange
        var existingPoints = new List<TimeSeriesPoint>
            {
                new TimeSeriesPoint
                {
                    Time = new DateTime(2021, 02, 02),
                    Key = "NASDAQ:ZM",
                    Value = 99.99f,
                },
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
        await _target.InsertRange(existingPoints);

        _mockContextFactory
            .Setup(e => e.CreateDbContext())
            .Returns(_deleteContext);

        // Act
        await _target.DeleteSeries("NASDAQ:ZM");

        // Assert
        var points = await _context.Set<TimeSeriesPoint>().ToListAsync();
        points.Count.Should().Be(1);
        points.ElementAt(0).Key.Should().Be("NYSE:PATH");
    }

    [Fact]
    public async Task DeleteSeries_WhenNoPointsExist_DoesNothing()
    {
        // Arrange
        var existingPoints = new List<TimeSeriesPoint>
            {
                new TimeSeriesPoint
                {
                    Time = new DateTime(2021, 02, 02),
                    Key = "NASDAQ:SE",
                    Value = 99.99f,
                },
                new TimeSeriesPoint
                {
                    Time = new DateTime(2021, 02, 03),
                    Key = "NYSE:PATH",
                    Value = 1001.01f,
                },
            };
        await _target.InsertRange(existingPoints);

        _mockContextFactory
            .Setup(e => e.CreateDbContext())
            .Returns(_deleteContext);

        // Act
        await _target.DeleteSeries("NASDAQ:ZM");

        // Assert
        var points = await _context.Set<TimeSeriesPoint>().ToListAsync();
        points.Count.Should().Be(2);
        points.ElementAt(0).Key.Should().Be("NASDAQ:SE");
        points.ElementAt(1).Key.Should().Be("NYSE:PATH");
    }
}
