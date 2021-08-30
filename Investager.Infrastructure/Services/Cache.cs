using Investager.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Investager.Infrastructure.Services
{
    public class Cache : ICache
    {
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(5);
        private readonly string _redisConnectionString;

        private IDatabase _database;
        private bool _connecting;

        public Cache(IConfiguration configuration)
        {
            _redisConnectionString = configuration.GetConnectionString("Redis");
            Task.Run(() => Connect());
        }

        public Task<T> Get<T>(string key, Func<Task<T>> dataRetriever)
        {
            return Get(key, CacheLifetime, dataRetriever);
        }

        public async Task<T> Get<T>(string key, TimeSpan ttl, Func<Task<T>> dataRetriever)
        {
            if (_database == null)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(() => Connect());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                var data = await dataRetriever.Invoke();

                return data;
            }

            var cacheValue = await _database.StringGetAsync(key);

            if (string.IsNullOrEmpty(cacheValue))
            {
                var data = await dataRetriever.Invoke();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () =>
                {
                    var serializedData = JsonSerializer.Serialize(data);
                    await _database.StringSetAsync(key, serializedData, ttl);
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                return data;
            }

            return JsonSerializer.Deserialize<T>(cacheValue);
        }

        public Task Clear(string key)
        {
            if (_database == null)
            {
                Task.Run(() => Connect());

                return Task.CompletedTask;
            }

            return _database.StringSetAsync(key, string.Empty);
        }

        private void Connect()
        {
            if (!_connecting)
            {
                _connecting = true;

                try
                {
                    var connection = ConnectionMultiplexer.Connect(_redisConnectionString);
                    _database = connection.GetDatabase();
                }
                catch (Exception)
                {
                    _connecting = false;
                }
            }
        }
    }
}
