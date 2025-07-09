using DebugTester.DataAccessLayer.Tests;
using System;

namespace DebugTester
{
    class Program
    {
        static void Main(string[] args)
        {
			try
			{

				DAL_Tester.TestCRUD();
            }
			catch (Exception e)
			{
				throw;
			}
        }
    }
}
