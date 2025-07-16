using Core.ApplicationModels.KironTestAPI.Tests;
using KironTest.API.DataAccess;

namespace KironTest.API.Interfaces
{
    public interface IDefaultDataExtension
    {
        void SetupDefaultTestData();
    }

    internal class DefaultDataExtension : IDefaultDataExtension
    {
        private readonly Logger.Logger mLog;
        private readonly IServiceCollection mServices;
        public DefaultDataExtension(IServiceCollection services)
        {
            mLog = Logger.Logger.GetLogger(typeof(DefaultDataExtension));
            mServices = services;
        }
        public void SetupDefaultTestData()
        {
            mLog.Debug($"In DefaultDataExtension {nameof(SetupDefaultTestData)}");
            try
            {
                using (var dp = new DataProvider())
                {
                    //Uncomment this to test database layer data seeding/ initialize data / migration to adddata from outside (Also uncomment the DB models sitting in Core / Tests..)
                    //SetupDefaultTestTableData(dp);
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"Error in {nameof(SetupDefaultTestData)}. Error: {ex}");
                mLog.Error($"Error in DefaultDataExtension {nameof(SetupDefaultTestData)}. If you are seeing this line check for an error in {nameof(DefaultDataExtension)} inside the kironTestApi application");
                throw;
            }
        }

        #region Uncomment this to test database layer data seeding/initialize data/ migration to adddata from outside (Also uncomment the DB models sitting in Core/Tests..)
        /*
        private void SetupDefaultTestTableData(DataProvider dp)
        {
            int createdCount = 0;
            var testTables = new List<TestTable>
            {
                new TestTable
                {
                    Email = "smaka1236@gmail.com",
                    UserName = "jack",
                    DefaultTheme = "Light",
                    IsActive = true,
                    TwoFactorType = CommunicationMethods.ColdCall
                },
                new TestTable
                {
                    Email = "lebi@gmail.com",
                    UserName = "lebo",
                    DefaultTheme = "Dark",
                    IsActive = true,
                    TwoFactorType = CommunicationMethods.Email
                },
                new TestTable
                {
                    Email = "bari@gmail.com",
                    UserName = "sophy",
                    DefaultTheme = "Blue",
                    IsActive = true,
                    TwoFactorType = CommunicationMethods.PUSH
                },
                new TestTable
                {
                    Email = "peter@gmail.com",
                    UserName = "darren",
                    DefaultTheme = "Green",
                    IsActive = true,
                    TwoFactorType = CommunicationMethods.Email
                },
                new TestTable
                {
                    Email = "lorraine@gmail.com",
                    UserName = "Nozuko",
                    DefaultTheme = "Yellow",
                    IsActive = true,
                    TwoFactorType = CommunicationMethods.SMS
                }
            };

            var tasks = testTables
                .Select(async table =>
                {
                    var id = dp.TestTable.CreateAsync(table).Result;
                    return id > 0 ? 1 : 0;
                })
                .ToList();

            var results = Task.WhenAll(tasks).Result;
            createdCount = results.Sum();

            mLog.Debug($"Created {createdCount} TestTable records.");

            // Now create TestTable2 records with foreign key references to TestTable

            int createdTestTables2Count = 0;
            var testTables2 = new List<TestTable2>
            {
                new TestTable2
                {
                    Email = "smaka1236@gmail.com",
                    UserName = "jack",
                    CreatedDater = DateTime.UtcNow
                },
                new TestTable2
                {
                    Email = "lebi@gmail.com",
                    UserName = "lebo",
                    CreatedDater = DateTime.UtcNow

                },
                new TestTable2
                {
                    Email = "bari@gmail.com",
                    UserName = "sophy",
                    CreatedDater = DateTime.UtcNow
                },
                new TestTable2
                {
                    Email = "peter@gmail.com",
                    UserName = "darren",
                    CreatedDater = DateTime.UtcNow
                },
                new TestTable2
                {
                    Email = "lorraine@gmail.com",
                    UserName = "Nozuko",
                    CreatedDater = DateTime.UtcNow
                }
            };

            var tasksz = testTables2
                .Select(async table =>
                {
                    var id = dp.TestTable2.CreateAsync(table).Result;
                    return id > 0 ? 1 : 0;
                })
                .ToList();

            var resultss = Task.WhenAll(tasksz).Result;
            createdTestTables2Count = resultss.Sum();

            mLog.Debug($"Created {createdTestTables2Count} TestTable2 records.");
        }
        */
        #endregion

    }
}
