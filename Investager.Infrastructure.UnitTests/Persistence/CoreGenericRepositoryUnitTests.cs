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
    public class CoreGenericRepositoryUnitTests
    {
        private readonly InvestagerCoreContext _context;

        private readonly CoreGenericRepository<User> _target;

        public CoreGenericRepositoryUnitTests()
        {
            var contextOptions = new DbContextOptionsBuilder<InvestagerCoreContext>()
                .UseSqlite("Filename=TestCore.db")
                .Options;

            _context = new InvestagerCoreContext(new Mock<IConfiguration>().Object, contextOptions);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _target = new CoreGenericRepository<User>(_context);
        }

        [Fact]
        public async Task GetAll_ReturnsExpectedEntities()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 385,
                    DisplayName = "uncultured",
                    DisplayEmail = "LighTthEmebAd@email.com",
                    Email = "lightthemebad@email.com",
                    Theme = Theme.Light,
                    PasswordHash = new byte[] {11, 13},
                    PasswordSalt = new byte[] { 99, 101 },
                },
                new User
                {
                    Id = 104,
                    DisplayName = "Dhurata",
                    DisplayEmail = "dhuraTa@email.com",
                    Email = "dhurata@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] {11, 14},
                    PasswordSalt = new byte[] { 99, 102 },
                },
            };

            await AddUsers(users);

            // Act
            var response = await _target.GetAll();

            // Assert
            response.Should().BeEquivalentTo(users);
        }

        [Fact]
        public async Task GetAll_DoesNotIncludeRelationships()
        {
            // Arrange
            var user = new User
            {
                Id = 104,
                DisplayName = "Dhurata",
                DisplayEmail = "dhuraTa@email.com",
                Email = "dhurata@email.com",
                Theme = Theme.Dark,
                PasswordHash = new byte[] { 11, 14 },
                PasswordSalt = new byte[] { 99, 102 },
                Watchlists = new List<Watchlist>
                {
                    new Watchlist
                    {
                        Id = 13,
                        DisplayOrder = 51,
                        Name = "Default",
                    },
                },
            };

            await AddUsers(new List<User> { user });

            // Act
            var responseUsers = await _target.GetAll();

            // Assert
            var response = responseUsers.Single();
            response.Id.Should().Be(user.Id);
            response.DisplayName.Should().Be(user.DisplayName);
            response.Email.Should().Be(user.Email);
            response.Watchlists.Should().BeNull();
        }

        [Fact]
        public async Task GetAll_WhenNoEntry_ReturnsEmptyList()
        {
            // Act
            var response = await _target.GetAll();

            // Assert
            response.Count().Should().Be(0);
        }

        [Fact]
        public async Task GetAllWithIncludable_IncludesRelationships()
        {
            // Arrange
            var user = new User
            {
                Id = 104,
                DisplayName = "Dhurata",
                DisplayEmail = "dhuraTa@email.com",
                Email = "dhurata@email.com",
                Theme = Theme.Dark,
                PasswordHash = new byte[] { 11, 14 },
                PasswordSalt = new byte[] { 99, 102 },
                RefreshTokens = new List<RefreshToken>
                {
                    new RefreshToken
                    {
                        Id = 385,
                        CreatedAt = new DateTime(1985, 10, 10),
                        EncodedValue = "real-token",
                        LastUsedAt = new DateTime(2021, 10, 10),
                    },
                },
                Watchlists = new List<Watchlist>
                {
                    new Watchlist
                    {
                        Id = 13,
                        DisplayOrder = 51,
                        Name = "Default",
                    },
                },
            };

            await AddUsers(new List<User> { user });

            // Act
            var responseUsers = await _target.GetAll(e => e.Include(x => x.Watchlists));

            // Assert
            var response = responseUsers.Single();
            response.Id.Should().Be(user.Id);
            response.DisplayName.Should().Be(user.DisplayName);
            response.Email.Should().Be(user.Email);

            var watchlist = user.Watchlists.Single();
            var responseWatchlist = response.Watchlists.Single();
            responseWatchlist.Id.Should().Be(watchlist.Id);
            responseWatchlist.Name.Should().Be(watchlist.Name);

            response.RefreshTokens.Should().BeNull();
        }

        [Fact]
        public async Task GetAllWithIncludable_WhenNoEntry_ReturnsEmptyList()
        {
            // Act
            var response = await _target.GetAll(e => e.Include(x => x.Watchlists));

            // Assert
            response.Count().Should().Be(0);
        }

        [Fact]
        public async Task GetByIdWithTracking_AllowsEdits()
        {
            // Arrange
            var user = new User
            {
                Id = 104,
                DisplayName = "Dhurata",
                DisplayEmail = "dhuraTa@email.com",
                Email = "dhurata@email.com",
                Theme = Theme.Dark,
                PasswordHash = new byte[] { 11, 14 },
                PasswordSalt = new byte[] { 99, 102 }
            };

            await AddUsers(new List<User> { user });

            // Act
            var response = await _target.GetByIdWithTracking(user.Id);

            // Assert
            response.Id.Should().Be(user.Id);
            response.DisplayName.Should().Be(user.DisplayName);
            response.Email.Should().Be(user.Email);

            var newName = "gigino";
            response.DisplayName = newName;
            await _context.SaveChangesAsync();

            var updatedUser = await _target.GetByIdWithTracking(user.Id);
            updatedUser.DisplayName.Should().Be(newName);
        }

        [Fact]
        public void GetByIdWithTracking_WhenEntityNotFound_Throws()
        {
            // Act
            Func<Task> act = async () => await _target.GetByIdWithTracking(2);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("Entity not found.");
        }

        [Fact]
        public async Task Find_ReturnsExpectedEntities()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 385,
                    DisplayName = "uncultured",
                    DisplayEmail = "LighTthEmebAd@email.com",
                    Email = "lightthemebad@email.com",
                    Theme = Theme.Light,
                    PasswordHash = new byte[] {11, 13},
                    PasswordSalt = new byte[] { 99, 101 },
                },
                new User
                {
                    Id = 104,
                    DisplayName = "Dhurata",
                    DisplayEmail = "dhuraTa@email.com",
                    Email = "dhurata@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] {11, 14},
                    PasswordSalt = new byte[] { 99, 102 },
                },
                new User
                {
                    Id = 139,
                    DisplayName = "zeynep",
                    DisplayEmail = "zeynep@email.com",
                    Email = "zeynep@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] {11, 14},
                    PasswordSalt = new byte[] { 99, 102 },
                },
            };

            await AddUsers(users);

            // Act
            var response = await _target.Find(e => e.Theme == Theme.Dark);

            // Assert
            var usersArray = users.ToArray();
            response.Count().Should().Be(2);

            var responseArray = response.ToArray();
            responseArray[0].Should().BeEquivalentTo(usersArray[1]);
            responseArray[1].Should().BeEquivalentTo(usersArray[2]);
        }

        [Fact]
        public async Task Find_WhenNoMatch_ReturnsEmptyArray()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 104,
                    DisplayName = "Dhurata",
                    DisplayEmail = "dhuraTa@email.com",
                    Email = "dhurata@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] {11, 14},
                    PasswordSalt = new byte[] { 99, 102 },
                },
            };

            await AddUsers(users);

            // Act
            var response = await _target.Find(e => e.Id == 101);

            // Assert
            response.Count().Should().Be(0);
        }

        [Fact]
        public async Task Find_WhenNoEntities_ReturnsEmptyArray()
        {
            // Act
            var response = await _target.Find(e => e.Id == 101);

            // Assert
            response.Count().Should().Be(0);
        }

        [Fact]
        public async Task FindWithInclude_ReturnsExpectedEntities()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 385,
                    DisplayName = "uncultured",
                    DisplayEmail = "LighTthEmebAd@email.com",
                    Email = "lightthemebad@email.com",
                    Theme = Theme.Light,
                    PasswordHash = new byte[] {11, 13},
                    PasswordSalt = new byte[] { 99, 101 },
                },
                new User
                {
                    Id = 104,
                    DisplayName = "Dhurata",
                    DisplayEmail = "dhuraTa@email.com",
                    Email = "dhurata@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] {11, 14},
                    PasswordSalt = new byte[] { 99, 102 },
                    RefreshTokens = new List<RefreshToken>
                    {
                        new RefreshToken
                        {
                            Id = 385,
                            CreatedAt = new DateTime(1985, 10, 10),
                            EncodedValue = "real-token",
                            LastUsedAt = new DateTime(2021, 10, 10),
                        },
                    },
                    Watchlists = new List<Watchlist>
                    {
                        new Watchlist
                        {
                            Id = 13,
                            DisplayOrder = 51,
                            Name = "Default",
                            Assets = new List<WatchlistAsset>
                            {
                                new WatchlistAsset
                                {
                                    Asset = new Asset
                                    {
                                        Id = 123,
                                        Name = "Sea Limited",
                                        Symbol = "SE",
                                        Exchange = "NASDAQ",
                                        Currency = "USD",
                                    },
                                },
                            },
                        },
                    },
                },
            };

            await AddUsers(users);

            // Act
            var response = await _target.Find(
                e => e.Id == 104,
                e => e.Include(x => x.Watchlists)
                    .ThenInclude(x => x.Assets));

            // Assert
            var responseUser = response.Single();
            responseUser.RefreshTokens.Should().BeNull();
            var watchlistAssets = responseUser.Watchlists.Single().Assets;
            watchlistAssets.Single().Asset.Should().BeNull();
        }

        [Fact]
        public async Task FindWithInclude_WhenNoMatch_ReturnsEmptyArray()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 104,
                    DisplayName = "Dhurata",
                    DisplayEmail = "dhuraTa@email.com",
                    Email = "dhurata@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] {11, 14},
                    PasswordSalt = new byte[] { 99, 102 },
                    RefreshTokens = new List<RefreshToken>
                    {
                        new RefreshToken
                        {
                            Id = 385,
                            CreatedAt = new DateTime(1985, 10, 10),
                            EncodedValue = "real-token",
                            LastUsedAt = new DateTime(2021, 10, 10),
                        },
                    },
                },
            };

            await AddUsers(users);

            // Act
            var response = await _target.Find(
                e => e.Id == 101,
                e => e.Include(x => x.RefreshTokens));

            // Assert
            response.Count().Should().Be(0);
        }

        [Fact]
        public async Task FindWithInclude_WhenNoEntities_ReturnsEmptyArray()
        {
            // Act
            var response = await _target.Find(
                e => e.Id == 101,
                e => e.Include(x => x.RefreshTokens));

            // Assert
            response.Count().Should().Be(0);
        }

        [Fact]
        public async Task FindWithTake_ReturnsExpectedEntities()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 385,
                    DisplayName = "uncultured",
                    DisplayEmail = "LighTthEmebAd@email.com",
                    Email = "lightthemebad@email.com",
                    Theme = Theme.Light,
                    PasswordHash = new byte[] {11, 13},
                    PasswordSalt = new byte[] { 99, 101 },
                },
                new User
                {
                    Id = 104,
                    DisplayName = "Dhurata",
                    DisplayEmail = "dhuraTa@email.com",
                    Email = "dhurata@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] {11, 14},
                    PasswordSalt = new byte[] { 99, 102 },
                },
                new User
                {
                    Id = 139,
                    DisplayName = "zeynep",
                    DisplayEmail = "zeynep@email.com",
                    Email = "zeynep@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] {11, 14},
                    PasswordSalt = new byte[] { 99, 102 },
                },
            };

            await AddUsers(users);

            // Act
            var response = await _target.Find(e => e.Theme == Theme.Dark, 1);

            // Assert
            var usersArray = users.ToArray();

            response.Single().Should().BeEquivalentTo(usersArray[1]);
        }

        [Fact]
        public async Task FindWithTake_WhenNoMatch_ReturnsEmptyArray()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 104,
                    DisplayName = "Dhurata",
                    DisplayEmail = "dhuraTa@email.com",
                    Email = "dhurata@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] {11, 14},
                    PasswordSalt = new byte[] { 99, 102 },
                },
            };

            await AddUsers(users);

            // Act
            var response = await _target.Find(e => e.Id == 101, 5);

            // Assert
            response.Count().Should().Be(0);
        }

        [Fact]
        public async Task FindWithTake_WhenNoEntities_ReturnsEmptyArray()
        {
            // Act
            var response = await _target.Find(e => e.Id == 101, 5);

            // Assert
            response.Count().Should().Be(0);
        }

        [Fact]
        public async Task FindWithOrderAndTake_ReturnsExpectedEntities()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 385,
                    DisplayName = "uncultured",
                    DisplayEmail = "LighTthEmebAd@email.com",
                    Email = "lightthemebad@email.com",
                    Theme = Theme.Light,
                    PasswordHash = new byte[] {11, 13},
                    PasswordSalt = new byte[] { 99, 101 },
                },
                new User
                {
                    Id = 104,
                    DisplayName = "Dhurata",
                    DisplayEmail = "dhuraTa@email.com",
                    Email = "dhurata@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] {11, 14},
                    PasswordSalt = new byte[] { 99, 102 },
                },
                new User
                {
                    Id = 139,
                    DisplayName = "zeynep",
                    DisplayEmail = "zeynep@email.com",
                    Email = "zeynep@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] {11, 14},
                    PasswordSalt = new byte[] { 99, 102 },
                },
            };

            await AddUsers(users);

            // Act
            var response = await _target.Find(e => e.Theme == Theme.Dark, e => e.OrderByDescending(x => x.Id), 1);

            // Assert
            var usersArray = users.ToArray();

            response.Single().Should().BeEquivalentTo(usersArray[2]);
        }

        [Fact]
        public async Task FindWithOrderAndTake_WhenNoMatch_ReturnsEmptyArray()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 104,
                    DisplayName = "Dhurata",
                    DisplayEmail = "dhuraTa@email.com",
                    Email = "dhurata@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] {11, 14},
                    PasswordSalt = new byte[] { 99, 102 },
                },
            };

            await AddUsers(users);

            // Act
            var response = await _target.Find(e => e.Id == 101, e => e.OrderByDescending(x => x.Id), 5);

            // Assert
            response.Count().Should().Be(0);
        }

        [Fact]
        public async Task FindWithOrderAndTake_WhenNoEntities_ReturnsEmptyArray()
        {
            // Act
            var response = await _target.Find(e => e.Id == 101, e => e.OrderByDescending(x => x.Id), 5);

            // Assert
            response.Count().Should().Be(0);
        }

        [Fact]
        public async Task Add_WhenEntityDoesNotExist_AddsEntity()
        {
            // Arrange
            var user = new User
            {
                Id = 385,
                DisplayName = "Dhurata",
                DisplayEmail = "dhuraTa@email.com",
                Email = "dhurata@email.com",
                Theme = Theme.Dark,
                PasswordHash = new byte[] { 11, 14 },
                PasswordSalt = new byte[] { 99, 102 },
            };

            // Act
            _target.Add(user);
            await _context.SaveChangesAsync();

            // Assert
            var response = await _target.GetAll();

            response.Single().Should().BeEquivalentTo(user);
        }

        [Fact]
        public async Task Add_WhenEntityExists_Throws()
        {
            // Arrange
            var user = new User
            {
                Id = 385,
                DisplayName = "Dhurata",
                DisplayEmail = "dhuraTa@email.com",
                Email = "dhurata@email.com",
                Theme = Theme.Dark,
                PasswordHash = new byte[] { 11, 14 },
                PasswordSalt = new byte[] { 99, 102 },
            };

            var users = new List<User>
            {
                user,
            };

            await AddUsers(users);

            // Act
            _target.Add(user);
            Func<Task> act = async () => await _context.SaveChangesAsync();

            // Assert
            act.Should().Throw<DbUpdateException>();
        }

        [Fact]
        public async Task Add_WhenProvidingRelatedEntityId_CreatesEntityCorrectly()
        {
            // Arrange
            var currencies = new List<Currency>
            {
                new Currency
                {
                    Id = 385,
                    Code = "usd",
                    Name = "KKona Dollar",
                    Type = CurrencyType.Fiat,
                },
                new Currency
                {
                    Id = 104,
                    Code = "eth",
                    Name = "Ethereum",
                    Type = CurrencyType.Crypto,
                },
            };

            await AddCurrencies(currencies);

            var target = new CoreGenericRepository<CurrencyPair>(_context);

            var pairToAdd = new CurrencyPair
            {
                FirstCurrencyId = 104,
                SecondCurrencyId = 385,
                HasTimeData = true,
                Provider = DataProviders.CoinGecko,
            };

            // Act
            target.Add(pairToAdd);
            await _context.SaveChangesAsync();

            // Assert
            _context.ChangeTracker.Clear();
            var pairs = await target.GetAll(e => e.Include(x => x.FirstCurrency).Include(x => x.SecondCurrency));
            var pair = pairs.Single();
            pair.FirstCurrency.Should().BeEquivalentTo(currencies.Single(e => e.Code == "eth"));
            pair.SecondCurrency.Should().BeEquivalentTo(currencies.Single(e => e.Code == "usd"));
            pair.HasTimeData.Should().BeTrue();
            pair.Provider.Should().Be(DataProviders.CoinGecko);
        }

        [Fact]
        public void Add_WhenCreatingNewRelatedEntities_Throws()
        {
            // Arrange
            var firstPair = new CurrencyPair
            {
                FirstCurrency = new Currency
                {
                    Code = "eth",
                    Name = "Ethereum",
                    Type = CurrencyType.Crypto,
                },
                SecondCurrency = new Currency
                {
                    Code = "usd",
                    Name = "KKona Dollar",
                    Type = CurrencyType.Fiat,
                },
                HasTimeData = true,
                Provider = DataProviders.CoinGecko,
            };

            var secondPair = new CurrencyPair
            {
                FirstCurrency = new Currency
                {
                    Code = "btc",
                    Name = "Bitcoin",
                    Type = CurrencyType.Crypto,
                },
                SecondCurrency = new Currency
                {
                    Code = "usd",
                    Name = "KKona Dollar",
                    Type = CurrencyType.Fiat,
                },
                HasTimeData = true,
                Provider = DataProviders.CoinGecko,
            };

            var target = new CoreGenericRepository<CurrencyPair>(_context);

            // Act
            target.Add(firstPair);
            target.Add(secondPair);
            Func<Task> act = async () => await _context.SaveChangesAsync();

            // Assert
            act.Should().Throw<DbUpdateException>();
        }

        [Fact]
        public async Task Update_ModifiesFields()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 385,
                    DisplayName = "Dhurata",
                    DisplayEmail = "dhuraTa@email.com",
                    Email = "dhurata@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] { 11, 14 },
                    PasswordSalt = new byte[] { 99, 102 },
                },
            };

            await AddUsers(users);

            var updatedUser = new User
            {
                Id = 385,
                DisplayName = "Dhurata Dora",
                DisplayEmail = "dhuraTa@email.com",
                Email = "dhurata@email.com",
                Theme = Theme.Dark,
                PasswordHash = new byte[] { 11, 14 },
                PasswordSalt = new byte[] { 99, 102 },
            };

            // Act
            _target.Update(updatedUser);
            await _context.SaveChangesAsync();

            // Assert
            var response = await _target.GetAll();
            response.Single().Should().BeEquivalentTo(updatedUser);
        }

        [Fact]
        public void Update_WhenEntityDoesNotExist_Throws()
        {
            // Arrange
            var updatedUser = new User
            {
                Id = 385,
                DisplayName = "Dhurata Dora",
                DisplayEmail = "dhuraTa@email.com",
                Email = "dhurata@email.com",
                Theme = Theme.Dark,
                PasswordHash = new byte[] { 11, 14 },
                PasswordSalt = new byte[] { 99, 102 },
            };

            // Act
            _target.Update(updatedUser);
            Func<Task> act = async () => await _context.SaveChangesAsync();

            // Assert
            act.Should().Throw<DbUpdateException>();
        }

        [Fact]
        public async Task DeleteById_WhenEntityExists_RemovesEntity()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 385,
                    DisplayName = "Dhurata",
                    DisplayEmail = "dhuraTa@email.com",
                    Email = "dhurata@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] { 11, 14 },
                    PasswordSalt = new byte[] { 99, 102 },
                },
            };

            await AddUsers(users);

            // Act
            _target.Delete(users.First().Id);
            await _context.SaveChangesAsync();

            // Assert
            var response = await _target.GetAll();
            response.Count().Should().Be(0);
        }

        [Fact]
        public void DeleteById_WhenEntityDoesNotExist_Throws()
        {
            // Act
            Action act = () => _target.Delete(385);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task DeleteByEntity_WhenEntityUntracked_RemovesEntity()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 385,
                    DisplayName = "Dhurata",
                    DisplayEmail = "dhuraTa@email.com",
                    Email = "dhurata@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] { 11, 14 },
                    PasswordSalt = new byte[] { 99, 102 },
                },
            };

            await AddUsers(users);

            // Act
            _target.Delete(users.First());
            await _context.SaveChangesAsync();

            // Assert
            var response = await _target.GetAll();
            response.Count().Should().Be(0);
        }

        [Fact]
        public async Task DeleteByEntity_WhenEntityTracked_RemovesEntity()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 385,
                    DisplayName = "Dhurata",
                    DisplayEmail = "dhuraTa@email.com",
                    Email = "dhurata@email.com",
                    Theme = Theme.Dark,
                    PasswordHash = new byte[] { 11, 14 },
                    PasswordSalt = new byte[] { 99, 102 },
                },
            };

            await AddUsers(users);

            var user = await _target.GetByIdWithTracking(users.First().Id);

            // Act
            _target.Delete(user);
            await _context.SaveChangesAsync();

            // Assert
            var response = await _target.GetAll();
            response.Count().Should().Be(0);
        }

        [Fact]
        public void DeleteByEntity_WhenEntityDoesNotExist_Throws()
        {
            // Arrange
            var user = new User
            {
                Id = 385,
                DisplayName = "Dhurata",
                DisplayEmail = "dhuraTa@email.com",
                Email = "dhurata@email.com",
                Theme = Theme.Dark,
                PasswordHash = new byte[] { 11, 14 },
                PasswordSalt = new byte[] { 99, 102 },
            };

            // Act
            _target.Delete(user);
            Func<Task> act = async () => await _context.SaveChangesAsync();

            // Assert
            act.Should().Throw<DbUpdateException>();
        }

        private async Task AddUsers(IList<User> users)
        {
            await _context.AddRangeAsync(users);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }

        private async Task AddCurrencies(IList<Currency> currencies)
        {
            await _context.AddRangeAsync(currencies);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }
    }
}
