namespace DatabaseLayer.Interfaces
{
    public interface IStoredProcedure
    {
        string StoredProcedureName { get; }

        string StoredProcedureCreateText { get; }
    }
}
