using KironTest.API.DataAccess;
using log4net;
using Microsoft.AspNetCore.Mvc;

namespace KironTest.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly Logger.Logger mLog;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            mLog = Logger.Logger.GetLogger(typeof(WeatherForecastController));
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            using (var dp = new DataProvider())
            {
                _logger.LogInformation("Ms-Logger info test");
                mLog.Debug("MyCustom logger debug log message from WeatherForecastController.");
                var testTable = dp.TestTable.Read(null).ToList();
                var testTable2 = dp.TestTable2.Read(null).ToList();
                var t = 9;

            }
            _logger.LogInformation("Getting weather forecast for the next 5 days.");
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
