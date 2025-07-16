using Core.ApplicationModels.KironTestAPI;
using DatabaseLayer;
using KironTest.API.DataAccess;
using System.Data;
using System.Text.Json;

namespace KironTest.API.ServiceHelpers
{
    public interface IBankHolidayService
    {
        Task<string> InitializeAsync();
        Task<List<Region>> GetRegionsAsync();
        Task<List<Holiday>> GetHolidaysAsync(int regionId);
    }

    public class BankHolidayService : IBankHolidayService
    {
        private readonly IHttpClientFactory _http;
        private readonly DataProvider _dp;
        private readonly SemaphoreSlim _initLock = new(1, 1);

        public BankHolidayService(IHttpClientFactory http)
        {
            _http = http;
            _dp = new DataProvider();
        }

        public async Task<string> InitializeAsync()
        {
            await _initLock.WaitAsync();
            try
            {
                // 1) have we already imported?
                bool done = (await _dp.ExecuteRetrievalProcedureAsync<bool>("spCheckImport", r => r.GetBoolean(0))).FirstOrDefault();
                if (done)
                    return "Already initialized—and now managed by background updater.";

                // 2) fetch JSON
                var json = await _http
                  .CreateClient()
                  .GetStringAsync("https://www.gov.uk/bank-holidays.json");

                using var doc = JsonDocument.Parse(json);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    try
                    {
                        var key = prop.Name;               // e.g. “england-and-wales”
                        var name = prop.Value.GetProperty("division").GetString();
                        // upsert region
                        await _dp.ExecuteStoredProcedureAsync(
                          "spUpsertRegion",
                          new QueryParameter("@RegionKey", SqlDbType.NVarChar, key),
                          new QueryParameter("@RegionName", SqlDbType.NVarChar, name)
                        );

                        // iterate holidays
                        foreach (var ev in prop.Value.GetProperty("events").EnumerateArray())
                        {
                            var date = ev.GetProperty("date").GetDateTime();
                            var title = ev.GetProperty("title").GetString();
                            // upsert holiday & join
                            await _dp.ExecuteStoredProcedureAsync(
                              "spUpsertHoliday",
                              new QueryParameter("@HolidayDate", SqlDbType.Date, date),
                              new QueryParameter("@Title", SqlDbType.NVarChar, title)
                            );
                            await _dp.ExecuteStoredProcedureAsync(
                              "spUpsertRegionHoliday",
                              new QueryParameter("@RegionKey", SqlDbType.NVarChar, key),
                              new QueryParameter("@HolidayDate", SqlDbType.Date, date),
                              new QueryParameter("@Title", SqlDbType.NVarChar, title)
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                       // TODO: Add logging
                    }
                    
                }

                // 3) mark as done
                await _dp.ExecuteStoredProcedureAsync(
                  "spSetImport",
                  new QueryParameter("@Initialized", SqlDbType.Bit, 1)
                );

                return "Import complete and scheduled for background refresh.";
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task<List<Region>> GetRegionsAsync()
        {
            return await _dp.Regions.ReadAsync(new { });
        }

        public async Task<List<Holiday>> GetHolidaysAsync(int regionId)
        {
            var result = await _dp.ExecuteRetrievalProcedureAsync<Holiday>(
               "spGetHolidaysByRegion",
               rdr => new Holiday
               {
                   HolidayId = rdr.GetInt32(0),
                   HolidayDate = rdr.GetDateTime(1),
                   Title = rdr.GetString(2)
               },
               new QueryParameter("@RegionId", SqlDbType.Int, regionId)
            );
            return result;
        }
    }
}
