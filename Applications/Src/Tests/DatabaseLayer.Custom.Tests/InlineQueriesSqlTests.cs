using System.Reflection;
using System.Text.RegularExpressions;

namespace DatabaseLayer.Custom.Tests
{
    public class InlineQueriesSqlTests
    {
        static string GetDataLayerPath()
        {
            // 1) Find where the test assembly lives
            var asmFolder = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location
            );
            // 2) Walk up from
            //    ...\Tests\DatabaseLayer.Custom.Tests\bin\Debug\netX.Y\
            //    up 5 levels to the solution root
            var solutionRoot = Path.GetFullPath(Path.Combine(
                asmFolder,
                "..", "..", "..", "..", ".."
            ));
            // 3) Build the path to your code
            var dataLayer = Path.Combine(
                solutionRoot,
                "Common",
                "DatabaseLayer.Custom"
            );
            if (!Directory.Exists(dataLayer))
                throw new DirectoryNotFoundException($"Could not find '{dataLayer}'");
            return dataLayer;
        }

        private const string TestConnString = @"Server=(localdb)\mssqllocaldb;Integrated Security=true;";

        // adjust to point at your data‐layer src folder
        private readonly string DataLayerPath = GetDataLayerPath();

        [Fact]
        public void No_InlineSql_In_DataLayer()
        {
            // look in all .cs files under DatabaseLayer
            var csFiles = Directory
                .EnumerateFiles(DataLayerPath, "*.cs", SearchOption.AllDirectories);

            // patterns that indicate inline SQL
            var bannedPatterns = new[]
            {
                new Regex(@"\.CommandType\s*=\s*CommandType\.Text", RegexOptions.Compiled),
                new Regex(@"new\s+SqlCommand\(\s*""(SELECT|INSERT|UPDATE|DELETE)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(@"\bExecute(Reader|NonQuery|Scalar)\s*\(\s*""(SELECT|INSERT|UPDATE|DELETE)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            };

            foreach (var file in csFiles)
            {
                var text = File.ReadAllText(file);
                foreach (var pat in bannedPatterns)
                {
                    if (pat.IsMatch(text))
                    {
                        Assert.False(true, $"Inline SQL query found in {Path.GetFileName(file)}: “{pat}”");
                    }
                }
            }
        }
    }
}