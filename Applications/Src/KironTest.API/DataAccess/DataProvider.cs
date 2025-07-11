using Core.ApplicationModels.KironTestAPI;
using Core.ApplicationModels.KironTestAPI.Tests;
using DatabaseLayer;
using DatabaseLayer.Custom.Services;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Metadata;
using DatabaseLayer.SqlServerProvider.DataObjectInterfaces;
using log4net;
using System;
using System.Configuration;
using System.Reflection;

namespace KironTest.API.DataAccess
{
    public class DataProvider : BaseDataProvider
    {

        public SpDataObjectInterface<TestTable> TestTable { get; set; }
        public SpDataObjectInterface<TestTable2> TestTable2 { get; set; }
        public SpDataObjectInterface<Logs> Logs { get; set; }
        public override string ConnectionString { get; }
        public DataProvider(string connectionString = "")
        {
            this.ConnectionString = string.IsNullOrEmpty(connectionString) ? DALConfiguration.DbConnection : connectionString;
            InitializeDataObjects();
        }

        public override void ConfigureDefaultData()
        {
            var log = Logger.Logger.GetLogger(typeof(DataProvider));

            var existing1 = this.TestTable.Read(null).ToList();
            var existing2 = this.TestTable2.Read(null).ToList();

            var seed1 = new List<TestTable>
            {
                new TestTable { Email = "smaka1236@gmail.com", UserName = "jack",    DefaultTheme = "Light",  IsActive = true, TwoFactorType = CommunicationMethods.ColdCall },
                new TestTable { Email = "lebi@gmail.com",      UserName = "lebo",    DefaultTheme = "Dark",   IsActive = true, TwoFactorType = CommunicationMethods.Email },
                new TestTable { Email = "bari@gmail.com",      UserName = "sophy",   DefaultTheme = "Blue",   IsActive = true, TwoFactorType = CommunicationMethods.PUSH },
                new TestTable { Email = "peter@gmail.com",     UserName = "darren",  DefaultTheme = "Green",  IsActive = true, TwoFactorType = CommunicationMethods.Email },
                new TestTable { Email = "lorraine@gmail.com",  UserName = "Nozuko",  DefaultTheme = "Yellow", IsActive = true, TwoFactorType = CommunicationMethods.SMS },
                new TestTable { Email = "fred@gmail.com",  UserName = "Fred",  DefaultTheme = "Pink", IsActive = true, TwoFactorType = CommunicationMethods.PUSH }
            };

            if (existing1.Count > 0)
            {
                var filteredSeed1 = seed1.Except(existing1).ToList();
                if (filteredSeed1.Count > 0)
                {
                    log.Info($"Adding {filteredSeed1.Count} new records to TestTable.");
                    filteredSeed1.ForEach(s => this.TestTable.Create(s));
                    log.Info($"Added {filteredSeed1.Count} new records to TestTable successfully.");  
                }
                else
                    log.Info("No new records to add to TestTable.");

                try
                {
                    var allTestTables = this.TestTable.Read(null).ToList();
                    var emailToIdMap = seed1
                                        .ToDictionary(
                                            x => x.Email,
                                            x => allTestTables.First(t => t.Email == x.Email).Id,
                                            StringComparer.OrdinalIgnoreCase
                                        );

                    var seed2 = seed1
                        .Select(s => new TestTable2
                        {
                            IdTestTable = emailToIdMap[s.Email],
                            UserName = s.UserName,
                            Email = s.Email,
                            CreatedDater = DateTime.UtcNow
                        })
                        .ToList();

                    var newRecords2 = seed2
                        .Where(s => !existing2.Any(e =>
                            e.IdTestTable == s.IdTestTable &&
                            string.Equals(e.Email, s.Email, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    if (newRecords2.Count > 0)
                    {
                        log.Info($"Adding {newRecords2.Count} new records to TestTable2.");
                        newRecords2.ForEach(s => this.TestTable2.Create(s));
                        log.Info($"Added {newRecords2.Count} new records to TestTable2 successfully.");
                    }
                    else
                        log.Info("No new records to add to TestTable2.");
                }
                catch (Exception ex)
                {
                    log.Error("An error occurred while seeding TestTable2", ex);
                    throw;
                }
            }
            else
            {
                try
                {
                    if (seed1.Count > 0)
                    {
                        log.Info($"Adding {seed1.Count} new records to TestTable.");
                        seed1.ForEach(s => this.TestTable.Create(s));
                        log.Info($"Added {seed1.Count} new records to TestTable successfully.");
                    }
                }
                catch (Exception e)
                {
                    log.Error("An error ocurred while seeding the db TestTable for the first time", e);
                    throw;
                }
                try
                {
                    var allTestTables = this.TestTable.Read(null).ToList();
                    var emailToIdMap = seed1
                                        .ToDictionary(
                                            x => x.Email,
                                            x => allTestTables.First(t => t.Email == x.Email).Id,
                                            StringComparer.OrdinalIgnoreCase
                                        );

                    var seed2 = seed1
                        .Select(s => new TestTable2
                        {
                            IdTestTable = emailToIdMap[s.Email],
                            UserName = s.UserName,
                            Email = s.Email,
                            CreatedDater = DateTime.UtcNow
                        })
                        .ToList();

                    var newRecords2 = seed2
                        .Where(s => !existing2.Any(e =>
                            e.IdTestTable == s.IdTestTable &&
                            string.Equals(e.Email, s.Email, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    if (newRecords2.Count > 0)
                    {
                        log.Info($"Adding {newRecords2.Count} new records to TestTable2.");
                        newRecords2.ForEach(s => this.TestTable2.Create(s));
                        log.Info($"Added {newRecords2.Count} new records to TestTable2 successfully.");
                    }
                    else
                        log.Info("No new records to add to TestTable2.");
                }
                catch (Exception ex)
                {
                    log.Error("An error occurred while seeding TestTable2", ex);
                    throw;
                }
            }
        }

        public override string GetTableName(Type type)
        {
            return $"{type.Name}";
        }

        protected override DbIndex[] GetIndices()
        {
            return new DbIndex[0]
            {
                //DbIndex.Create(typeof(Users), nameof(Classes.Users.UserName)).AddColumns(nameof(Classes.Users.UserName)).IsUnique(),
                //DbIndex.Create(typeof(Role), nameof(Role.Name)).AddColumns(nameof(Role.Name)).IsUnique(),
            };
        }

        private void InitializeDataObjects()
        {
            // Generic Create SQL Data Interface Objects
            var tableFields = typeof(DataProvider)
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                .Where(f => typeof(IDataObjectInterface)
                .IsAssignableFrom(f.PropertyType))
                .ToList();

            var childType = this.GetType()
            .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
            .Where(f => typeof(IDataObjectInterface)
            .IsAssignableFrom(f.PropertyType))
            .ToList();


            tableFields.AddRange(childType);

            Parallel.ForEach(tableFields, field =>
            {
                var tableRef = Activator.CreateInstance(field.PropertyType, new Object[] { this });
                field.SetValue(this, tableRef);
            });
        }
    }
}
