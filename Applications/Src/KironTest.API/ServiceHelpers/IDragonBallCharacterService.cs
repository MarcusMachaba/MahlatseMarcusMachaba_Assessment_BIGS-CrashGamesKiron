using Core.ApplicationModels.KironTestAPI;
using KironTest.API.Controllers;
using log4net;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace KironTest.API.ServiceHelpers
{
    public interface IDragonBallCharacterService
    {
        Task<List<DragonBallCharacter>> GetCharactersAsync();
    }

    public class DragonBallCharacterService : IDragonBallCharacterService
    {
        private readonly IHttpClientFactory _http;
        private readonly IMemoryCache _cache;
        private static readonly SemaphoreSlim _lock = new(1, 1);
        private const string CacheKey = "DragonBallCharacters";
        private readonly TimeSpan SlidingExpiration = TimeSpan.FromHours(1);
        private readonly Logger.Logger mLog;

        public DragonBallCharacterService(IHttpClientFactory http, IMemoryCache cache)
        {
            _http = http;
            _cache = cache;
            mLog = Logger.Logger.GetLogger(typeof(DragonBallCharacterService));
        }

        public async Task<List<DragonBallCharacter>> GetCharactersAsync()
        {
            if (_cache.TryGetValue(CacheKey, out List<DragonBallCharacter> characters))
            {
                // Accessed, extend sliding expiration
                _cache.Set(CacheKey, characters, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = SlidingExpiration
                });
                return characters;
            }

            await _lock.WaitAsync(); // Ensure thread-safe fetch
            try
            {
                // Double-check after acquiring lock
                if (_cache.TryGetValue(CacheKey, out characters))
                {
                    return characters;
                }

                try
                {
                    var jsonString = await _http.CreateClient().GetStringAsync("https://dragonball-api.com/api/characters?limit=1000");
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var response = JsonSerializer.Deserialize<DragonBallApiResponse>(jsonString, options);
                    characters = response.Items ??= new List<DragonBallCharacter>();


                }
                catch (Exception ex)
                {
                    mLog.Error($"Error fetching data externally", ex); ;
                }
                

                _cache.Set(CacheKey, characters, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = SlidingExpiration
                });

                return characters;
            }
            finally
            {
                _lock.Release();
            }
        }
    }

}
