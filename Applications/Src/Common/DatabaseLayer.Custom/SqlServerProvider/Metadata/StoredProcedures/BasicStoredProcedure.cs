using DatabaseLayer.Interfaces;

namespace DatabaseLayer.SqlServerProvider.Metadata.StoredProcedures
{
    internal class BasicStoredProcedure : IStoredProcedure
    {
        public BasicStoredProcedure(string storedProcedureName, string storedProcedureCreateText)
        {
            this.StoredProcedureName = storedProcedureName;
            this.StoredProcedureCreateText = storedProcedureCreateText;
        }

        public string StoredProcedureName { get; set; }

        public string StoredProcedureCreateText { get; set; }
    }
}
