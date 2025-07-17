using CachingLayer.Abstractions;
using Core.ApplicationModels.KironTestAPI;
using DatabaseLayer;
using KironTest.API.Controllers;
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
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly Logger.Logger mLog;
        private readonly ICacheService _cacheService;
        private readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        public BankHolidayService(IHttpClientFactory http, ICacheService cache)
        {
            _http = http;
            _dp = new DataProvider();
            mLog = Logger.Logger.GetLogger(typeof(BankHolidayService));
            _cacheService = cache;
        }

        public async Task<string> InitializeAsync()
        {
            await _lock.WaitAsync();
            try
            {
                // 1) have we already imported?
                bool done = (await _dp.ExecuteRetrievalProcedureAsync<bool>("spCheckImport", r => r.GetBoolean(0))).FirstOrDefault();
                if (done)
                {
                    mLog.Info($"{nameof(BankHolidayService)} Already initialized—and now managed by background updater.");
                    return "Already initialized—and now managed by background updater.";
                }

                // 2) fetch JSON
                var json = await _http.CreateClient().GetStringAsync("https://www.gov.uk/bank-holidays.json");

                using var doc = JsonDocument.Parse(json);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    try
                    {
                        var key = prop.Name;               
                        var name = prop.Value.GetProperty("division").GetString();
                        _dp.StartTransaction(); 
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
                        mLog.Error($"Error upserting Region or Holiday or RegionHoliday into the db", ex);
                        _dp.RollbackTransaction(); 
                    }

                }
                try
                {
                    // 3) mark as done
                    await _dp.ExecuteStoredProcedureAsync(
                      "spSetImport",
                      new QueryParameter("@Initialized", SqlDbType.Bit, 1)
                    );
                }
                catch (Exception e)
                {
                    mLog.Error($"Error marking the import as complete", e);
                    _dp.RollbackTransaction();
                }
                
                _dp.CommitTransaction();

                mLog.Info($"{nameof(BankHolidayService)} completed Import and scheduled for background refresh.");
                return "Import complete and scheduled for background refresh.";
            }
            finally
            {
                _lock.Release();
                _dp.Dispose();
            }
        }

        public async Task<List<Region>> GetRegionsAsync()
        {
            try
            {
                const string cacheKey = "AllRegions";
                if (_cacheService.TryGetValue(cacheKey, out List<Region> regions))
                    return regions;

                regions = await _dp.Regions.ReadAsync(new { });
                _cacheService.Set(cacheKey, regions, CacheDuration);
                return regions;
            }
            finally
            {
                _dp.Dispose();
            }
            
        }

        public async Task<List<Holiday>> GetHolidaysAsync(int regionId)
        {
            try
            {
                string cacheKey = $"Holidays_Region_{regionId}";
                if (_cacheService.TryGetValue(cacheKey, out List<Holiday> holidays))
                    return holidays;

                holidays = await _dp.ExecuteRetrievalProcedureAsync<Holiday>(
                   "spGetHolidaysByRegion",
                   rdr => new Holiday
                   {
                       HolidayId = rdr.GetInt32(0),
                       HolidayDate = rdr.GetDateTime(1),
                       Title = rdr.GetString(2)
                   },
                   new QueryParameter("@RegionId", SqlDbType.Int, regionId)
                );
                _cacheService.Set(cacheKey, holidays, CacheDuration);
                return holidays;
            }
            finally
            {
                _dp.Dispose();
            }
            
        }
    }
}
