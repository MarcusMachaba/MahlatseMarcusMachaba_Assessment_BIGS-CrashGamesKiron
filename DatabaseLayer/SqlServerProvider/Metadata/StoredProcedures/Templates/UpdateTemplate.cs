namespace DatabaseLayer.SqlServerProvider.Metadata.StoredProcedures.Templates
{
    public class UpdateTemplate : BaseTemplate
    {
        public override string Name => "sp" + this.FileGroup + "_" + this.Title + "_Update";

        public override string Template => "CREATE PROCEDURE [dbo].[" + this.Name + "]\r\n(\r\n" + this.SqlParameters() + "\r\n)\r\nAS\r\nBEGIN\r\n\tUPDATE [dbo].[" + this.TableName + "] SET\r\n" + this.UpdateParameters() + "\r\n\tWHERE\r\n\t\t([dbo].[" + this.TableName + "].[" + this.Primary.Name + "] = @" + this.Primary.Name + ")\r\nEND";
    }
}
