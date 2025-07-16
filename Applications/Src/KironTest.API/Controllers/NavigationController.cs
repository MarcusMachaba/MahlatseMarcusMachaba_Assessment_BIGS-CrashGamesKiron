using KironTest.API.ServiceHelpers;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KironTest.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NavigationController : ControllerBase
    {
        private readonly INavigationService _navigationService;
        public NavigationController(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [HttpGet("navigation")]
        [ResponseCache(Duration = 1800)] // 30 minutes client-side caching
        public async Task<IActionResult> GetNavigationHierarchy()
        {
            var allItems = await _navigationService.GetAllyAsync();
            var tree = await _navigationService.GetTreeAsync(allItems);

            return Ok(tree);
        }
    }
}
