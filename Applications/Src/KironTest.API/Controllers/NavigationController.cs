using Core.ApplicationModels.KironTestAPI;
using KironTest.API.ServiceHelpers;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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

        /// <summary>
        /// Retrieves the navigation hierarchy as a tree structure.
        /// </summary>
        /// <remarks>
        /// This endpoint returns a tree of navigation items grouped by parent-child relationships.
        /// The data is client-side cached for 30 minutes to reduce traffic.
        /// </remarks>
        /// <returns>List of navigation nodes structured as a tree</returns>
        /// <response code="200">Returns the navigation tree</response>
        /// <response code="401">Unauthorized request</response>
        [HttpGet("navigation")]
        [ResponseCache(Duration = 1800)] // 30 minutes client-side caching
        [ProducesResponseType(typeof(List<NavNode>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(HttpStatusCode), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetNavigationHierarchy()
        {
            var allItems = await _navigationService.GetAllyAsync();
            var tree = await _navigationService.GetTreeAsync(allItems);

            return Ok(tree);
        }
    }
}
