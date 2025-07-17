using CachingLayer.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CachingLayer.CacheStore
{
    public interface  IMemoryCacheService : ICacheService
    {
    }

    public class MemoryCacheService : IMemoryCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _defaultOptions;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
            _defaultOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null)
        {
            if (_cache.TryGetValue<T>(key, out T cached))
                return cached;

            var data = await factory();
            var opts = absoluteExpiration.HasValue
                ? new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpiration }
                : _defaultOptions;

            _cache.Set(key, data, opts);
            return data;
        }

        public T GetOrAdd<T>(string key, Func<T> factory, TimeSpan? absoluteExpirationRelativeToNow = null)
        {
            if (_cache.TryGetValue(key, out T existing))
                return existing;

            var value = factory();
            var opts = absoluteExpirationRelativeToNow.HasValue
                ? new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow }
                : _defaultOptions;

            _cache.Set(key, value, opts);
            return value;
        }

        public void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null)
        {
            var opts = absoluteExpiration.HasValue
                ? new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpiration }
                : _defaultOptions;

            _cache.Set(key, value, opts);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            return _cache.TryGetValue(key, out value);
        }
    }
}
