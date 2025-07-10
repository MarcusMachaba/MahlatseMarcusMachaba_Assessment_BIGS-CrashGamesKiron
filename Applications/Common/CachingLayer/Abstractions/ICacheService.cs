using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CachingLayer.Abstractions
{
    public interface ICacheService
    {
        /// <summary>
        /// Get from cache if present, otherwise invoke factory(), cache the result, and return it.
        /// </summary>
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpirationRelativeToNow = null);

        /// <summary>
        /// Same as GetOrAddAsync but synchronous.
        /// </summary>
        T GetOrAdd<T>(string key, Func<T> factory, TimeSpan? absoluteExpirationRelativeToNow = null);

        /// <summary>
        /// Force‐set a value in cache.
        /// </summary>
        void Set<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null);

        /// <summary>
        /// Remove an entry from cache.
        /// </summary>
        void Remove(string key);
    }

}
