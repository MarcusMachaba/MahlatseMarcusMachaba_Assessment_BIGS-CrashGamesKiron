using DatabaseLayer.Attributes;
using System;

namespace Core.ApplicationModels.KironTestAPI
{
    [TableContract(Audited = false, PrimaryKey = nameof(ImportName))]
    public class BankHolidayImport
    {
        /// <summary>
        /// Always “UK” for this import.
        /// </summary>
        [ColumnContract(Length = 50, Required = true, Queryable = true)]
        public string ImportName { get; set; }

        /// <summary>
        /// Becomes 1 once the initial load has run.
        /// </summary>
        [ColumnContract(DefaultValue = false, Queryable = true)]
        public bool Initialized { get; set; }

        /// <summary>
        /// Last time we ran the fetch/update process.
        /// </summary>
        [ColumnContract(Queryable = true)]
        public DateTime? LastRun { get; set; }
    }

}
