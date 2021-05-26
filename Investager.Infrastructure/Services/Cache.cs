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
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromDays(1);
        private readonly IDatabase _database;

        public Cache(IConfiguration configuration)
        {
            var connection = ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis"));
            _database = connection.GetDatabase();
        }

        public Task<T> Get<T>(string key, Func<Task<T>> dataRetriever)
        {
            return Get(key, CacheLifetime, dataRetriever);
        }

        public async Task<T> Get<T>(string key, TimeSpan ttl, Func<Task<T>> dataRetriever)
        {
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
            return _database.StringSetAsync(key, string.Empty);
        }
    }
}
