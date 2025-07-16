using DatabaseLayer.Interfaces;
using System.Reflection;
using System.Text.RegularExpressions;

namespace KironTest.API.DataAccess.Procs
{
    internal class EmbeddedSqlStoredProcedure : IStoredProcedure
    {
        public string StoredProcedureName { get; }
        public string StoredProcedureCreateText { get; }

        public EmbeddedSqlStoredProcedure(string resourceName)
        {
            using var stream = Assembly.GetExecutingAssembly()
                                       .GetManifestResourceStream(resourceName)
                               ?? throw new InvalidOperationException($"Resource not found: {resourceName}");
            using var rdr = new StreamReader(stream);
            StoredProcedureCreateText = rdr.ReadToEnd();
            var m = Regex.Match(StoredProcedureCreateText,
                       @"CREATE\s+PROCEDURE\s+(?:\[dbo\]\.)?\[?(?<nm>[^\s\(\]]+)\]?",
                       RegexOptions.IgnoreCase);
            StoredProcedureName = m.Success
                               ? m.Groups["nm"].Value
                               : throw new InvalidOperationException("Cannot parse proc name");
        }
    }

}
