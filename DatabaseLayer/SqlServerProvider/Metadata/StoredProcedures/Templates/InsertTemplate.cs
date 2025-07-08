namespace DatabaseLayer.SqlServerProvider.Metadata.StoredProcedures.Templates
{
    public class InsertTemplate : BaseTemplate
    {
        public override string Name => "sp" + this.FileGroup + "_" + this.Title + "_Insert";

        public override string Template => "CREATE PROCEDURE [dbo].[" + this.Name + "]\r\n(\r\n" + this.SqlParameters(false) + "\r\n)\r\nAS\r\nBEGIN\r\n    INSERT INTO [dbo].[" + this.TableName + "]\r\n    (\r\n" + this.InsertParameters() + "\r\n    )\r\n    RETURN SCOPE_IDENTITY()\r\nEND";
    }
}
