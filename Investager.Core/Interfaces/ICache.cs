using System;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface ICache
    {
        /// <summary>
        /// Gets the value from the cache.
        /// If not present, uses the func to retrieve the value and store it in the cache.
        /// </summary>
        /// <typeparam name="T">The stored type.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="dataRetriever">The func to get data if not present in cache.</param>
        /// <returns></returns>
        Task<T> Get<T>(string key, Func<Task<T>> dataRetriever);

        Task Clear(string key);
    }
}
