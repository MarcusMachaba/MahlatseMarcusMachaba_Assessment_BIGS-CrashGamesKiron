using DatabaseLayer.Interfaces;
using System.Text.RegularExpressions;

namespace KironTest.API.DataAccess.Procs
{
    /// <summary>
    /// A tiny IStoredProcedure wrapper for a single CREATE PROCEDURE batch.
    /// </summary>
    internal class BatchStoredProcedure : IStoredProcedure
    {
        private static readonly Regex _nameRx =
            new Regex(
                @"CREATE\s+(?:OR\s+ALTER\s+)?PROCEDURE\s+(?:\[dbo\]\.)?\[?(?<nm>[^\s\(\]]+)\]?",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            );

        public string StoredProcedureCreateText { get; }
        public string StoredProcedureName { get; }

        public BatchStoredProcedure(string sqlBatch)
        {
            StoredProcedureCreateText = sqlBatch;

            var m = _nameRx.Match(sqlBatch);
            if (!m.Success)
                throw new InvalidOperationException(
                    "Could not extract procedure name from:\n" + sqlBatch
                );
            StoredProcedureName = m.Groups["nm"].Value;
        }
    }

}
