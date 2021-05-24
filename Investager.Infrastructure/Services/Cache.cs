using Investager.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Investager.Infrastructure.Services
{
    public class Cache : ICache
    {
        public Task<T> Get<T>(string key, Func<Task<T>> dataRetriever)
        {
            throw new NotImplementedException();
        }

        public Task Clear(string key)
        {
            throw new NotImplementedException();
        }
    }
}
