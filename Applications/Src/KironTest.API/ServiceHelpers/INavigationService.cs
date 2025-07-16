using Azure.Core;
using Core.ApplicationModels.KironTestAPI;
using KironTest.API.DataAccess;
using log4net;
using Microsoft.Extensions.Caching.Memory;

namespace KironTest.API.ServiceHelpers
{
    public interface INavigationService
    {
        Task<List<Navigation>> GetAllyAsync();
        Task<List<NavNode>> GetTreeAsync(List<Navigation> flat);
    }

    public class NavigationService : INavigationService
    {
        private readonly Logger.Logger mLog;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "NavigationHierarchy";

        public NavigationService(IMemoryCache cache)
        {
            mLog = Logger.Logger.GetLogger(typeof(AuthService));
            _cache = cache;
        }

        public async Task<List<Navigation>> GetAllyAsync()
        {
            if (_cache.TryGetValue(CacheKey, out List<Navigation> cached))
            {
                return cached;
            }
            using (var dp = new DataProvider())
            {
                try
                {
                    var allItems = (await dp.Navigations.ReadAsync(new {  })).OrderBy(n => n.ParentID).ToList();
                    
                    _cache.Set(CacheKey, allItems, TimeSpan.FromMinutes(30));
                    return allItems;
                }
                catch (Exception ex)
                {
                    mLog.Error($"Error retrieving Navigation-Hierarchy: {ex}");
                    return new List<Navigation>();
                }
            }
        }

        public Task<List<NavNode>> GetTreeAsync(List<Navigation> flat)
        {
            var lookup = flat
            .Select(e => new {
                e.ID,
                Node = new NavNode { Text = e.Text }
            })
            .ToDictionary(x => x.ID, x => x.Node);

            var roots = new List<NavNode>();

            foreach (var e in flat)
            {
                var node = lookup[e.ID];

                if (e.ParentID <= 0)      
                {
                    roots.Add(node);
                }
                else if (lookup.TryGetValue(e.ParentID, out var parent))
                {
                    parent.Children.Add(node);
                }
            }

            return Task.FromResult(roots);
        }
    }
}
