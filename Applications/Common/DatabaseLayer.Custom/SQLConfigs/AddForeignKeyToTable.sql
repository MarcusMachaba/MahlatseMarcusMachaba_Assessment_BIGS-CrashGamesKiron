IF OBJECT_ID('dbo.AddForeignKeyToTable','P') IS NOT NULL
    DROP PROCEDURE dbo.AddForeignKeyToTable;
GO
CREATE PROCEDURE dbo.AddForeignKeyToTable
    @SchemaName            SYSNAME       = 'dbo',
    @TableName             SYSNAME,
    @ForeignKeyDefinition  NVARCHAR(MAX)  -- full 'CONSTRAINT [FK...] FOREIGN KEY ...'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(MAX)
      = N'ALTER TABLE ' + QUOTENAME(@SchemaName) + N'.'
        + QUOTENAME(@TableName)
        + N' ADD ' + @ForeignKeyDefinition + N';';
    EXEC sp_executesql @sql;
END;
GO