using DatabaseLayer.SqlServerProvider.Metadata;

namespace DatabaseLayer.Metadata.Differences
{
    public class ColumnDifference
    {
        public ColumnMetaData ModelColumn { get; set; }

        public DatabaseColumn DatabaseColumn { get; set; }

        public bool ForeignKeyChanged { get; private set; }

        public bool ColumnChanged { get; private set; }

        public ColumnDifference(
          ColumnMetaData col,
          DatabaseColumn dbCol,
          bool columnChanged,
          bool foreignKeyChanged)
        {
            this.ModelColumn = col;
            this.DatabaseColumn = dbCol;
            this.ForeignKeyChanged = foreignKeyChanged;
            this.ColumnChanged = columnChanged;
        }
    }
}
