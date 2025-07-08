namespace DatabaseLayer.SqlServerProvider.Metadata.StoredProcedures.Templates
{
    public class DeleteTemplate : BaseTemplate
    {
        public override string Name => "sp" + this.FileGroup + "_" + this.Title + "_Delete";

        public override string Template => "CREATE PROCEDURE [dbo].[" + this.Name + "]\r\n(\r\n\t@" + this.Primary.Name + " " + this.GetSqlDataType(this.Primary) + "\r\n)\r\nAS\r\nBEGIN\r\n\tDELETE FROM [dbo].[" + this.TableName + "]\r\n\tWHERE\r\n\t\t(dbo.[" + this.TableName + "].[" + this.Primary.Name + "] = @" + this.Primary.Name + ")\r\nEND";
    }
}
