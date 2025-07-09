using DebugTester.DataAccessLayer.Tests.Models;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DebugTester.DataAccessLayer.Tests
{
    public class DAL_Tester
    {
        public static void TestCRUD()
        {
            LoggingDependencies.SetLog4NetDatabaseConnectionString(new DataProvider().ConnectionString);
            // Test the DataAccessLayer functionality here
            // For example, you can create instances of your models and perform CRUD operations
            // using the DataAccessLayer methods.
            Console.WriteLine("Data Access Layer Tests");



            using (var dp = new DataProvider())
            {
                //applies db-schema changes if models have been modified
                var result = dp.CompareModelToDatabase();
                if (result.TableDifferences.Count > 0 || result.StoredProcedureDifferences.Count > 0 || result.IndexDifferences.Count > 0)
                {
                    dp.Deploy(result);
                }

                try
                {
                    dp.StartTransaction();


                    // Create a new bank & Insert the new bank into the database
                    dp.Banks.Create(new Models.Bank
                    {
                        Name = "Test Bank",
                        SecondName = "Test Bank Second Name"
                    });

                    // Create a new bank & Insert the new bank into the database
                    dp.Banks.Create(new Models.Bank
                    {
                        Name = "Test Bank",
                        SecondName = "Test Bank Second Name"
                    });




                    // Retrieve all banks from the database
                    var banks = dp.Banks.Read(null);
                    // Display the retrieved banks
                    foreach (var bank in banks)
                    {
                        Console.WriteLine($"Bank ID: {bank.IdBank}, Name: {bank.Name}, Second Name: {bank.SecondName}");
                    }


                    // Update the first bank's name
                    if (banks.Any())
                    {
                        var secondBank = banks.FirstOrDefault(x=>x.IdBank > 1);
                        secondBank.Name = "Updated Bank Name";
                        dp.Banks.Update(secondBank);
                    }


                    // Delete the first bank
                    if (banks.Any())
                    {
                        var firstBank = banks.First();
                        dp.Banks.Delete(firstBank.IdBank);
                    }


                    dp.CommitTransaction();
                    Console.WriteLine("CRUD operations completed successfully.");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    dp.RollbackTransaction();
                    throw;
                }
            }


        }
    }
}
