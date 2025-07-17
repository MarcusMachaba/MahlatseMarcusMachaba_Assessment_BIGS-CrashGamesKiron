using Core.ApplicationModels.KironTestAPI;
using KironTest.API.ServiceHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace KironTest.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UKBankHolidaysController : ControllerBase
    {
        private readonly IBankHolidayService _bankHolidayService;
        // TODO: Read this TimeSpan from a config not hardcode
        private readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        public UKBankHolidaysController(IBankHolidayService svc)
        {
            _bankHolidayService = svc;
        }

        /// <summary>
        /// Imports the latest UK Bank Holidays from GOV.UK if not already imported.
        /// </summary>
        /// <returns>List of navigation nodes structured as a tree</returns>
        /// <response code="200">Returns the navigation tree</response>
        /// <response code="401">Unauthorized request</response>
        /// <response code="400">Bad request</response>
        [HttpPost("initialize")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(HttpStatusCode), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> Initialize()
        {
            var msg = await _bankHolidayService.InitializeAsync();
            if (msg.StartsWith("Already"))
                return BadRequest(msg);
            return Ok(msg);
        }

        /// <summary>
        /// Returns the list of regions with bank holidays.
        /// </summary>
        /// <response code="200">Returns the navigation tree</response>
        /// <response code="401">Unauthorized request</response>
        [HttpGet("regions")]
        [ProducesResponseType(typeof(List<Region>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(HttpStatusCode), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetRegions()
        {
            var regions = await _bankHolidayService.GetRegionsAsync();
            return Ok(regions);
        }

        /// <summary>
        /// Returns the holidays for a given region.
        /// </summary>
        /// <response code="200">Returns the navigation tree</response>
        /// <response code="401">Unauthorized request</response>
        [HttpGet("regions/{regionId}/holidays")]
        [ProducesResponseType(typeof(List<Holiday>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(HttpStatusCode), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetHolidays(int regionId)
        {
            var hols = await _bankHolidayService.GetHolidaysAsync(regionId);
            return Ok(hols);
        }
    }
}
