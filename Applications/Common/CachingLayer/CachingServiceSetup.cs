using CachingLayer.Abstractions;
using CachingLayer.CacheStore;
using Microsoft.Extensions.DependencyInjection;

namespace CachingLayer
{
    /// <summary>
    /// Registers the caching layer into any IServiceCollection.
    /// </summary>
    public static class CachingServiceSetup
    {
        public static IServiceCollection SetupEnvironmentAddCachingLayer(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
            return services;
        }
    }
}
