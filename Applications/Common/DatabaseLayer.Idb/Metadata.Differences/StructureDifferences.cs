using DatabaseLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseLayer.Metadata.Differences
{
    public class StructureDifferences
    {
        public StructureDifferences()
        {
            this.TableDifferences = new List<TableDifference>();
            this.IndexDifferences = new List<IndexDifference>();
            this.StoredProcedureDifferences = new List<IStoredProcedure>();
        }

        public List<TableDifference> TableDifferences { get; private set; }

        public List<IndexDifference> IndexDifferences { get; private set; }

        public List<IStoredProcedure> StoredProcedureDifferences { get; private set; }

        public bool Match => !this.TableDifferences.Any<TableDifference>((Func<TableDifference, bool>)(td => !td.Match)) && this.IndexDifferences.Count == 0 && this.StoredProcedureDifferences.Count == 0;
    }
}
