using KironTest.API.ServiceHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace KironTest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UKBankHolidaysController : ControllerBase
    {
        private readonly IBankHolidayService _svc;
        private readonly IMemoryCache _cache;
        private const string RegionsKey = "BankHolidays_Regions";
        private const string HolidaysKey = "BankHolidays_Hols";

        public UKBankHolidaysController(IBankHolidayService svc, IMemoryCache cache)
        {
            _svc = svc;
            _cache = cache;
        }

        // init (POST)
        [HttpPost("initialize")]
        public async Task<IActionResult> Initialize()
        {
            var msg = await _svc.InitializeAsync();
            if (msg.StartsWith("Already"))
                return BadRequest(msg);
            return Ok(msg);
        }

        // list regions (GET) – cache 30m
        // TODO: Read this TimeSpan from a config not hardcode
        [HttpGet("regions")]
        public async Task<IActionResult> GetRegions()
        {
            var regions = await _cache.GetOrCreateAsync(
              RegionsKey, entry => {
                  entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                  return _svc.GetRegionsAsync();
              });
            return Ok(regions);
        }

        // holidays by region (GET) – cache 30m per region
        // TODO: Read this TimeSpan from a config not hardcode
        [HttpGet("regions/{regionId}/holidays")]
        public async Task<IActionResult> GetHolidays(int regionId)
        {
            var key = $"{HolidaysKey}_{regionId}";
            var hols = await _cache.GetOrCreateAsync(
              key, entry => {
                  entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                  return _svc.GetHolidaysAsync(regionId);
              });
            return Ok(hols);
        }
    }
}
