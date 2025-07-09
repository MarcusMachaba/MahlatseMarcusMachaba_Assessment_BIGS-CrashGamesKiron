IF OBJECT_ID('dbo.AlterColumnInTable','P') IS NOT NULL
    DROP PROCEDURE dbo.AlterColumnInTable;
GO
CREATE PROCEDURE dbo.AlterColumnInTable
    @SchemaName       SYSNAME       = 'dbo',
    @TableName        SYSNAME,
    @ColumnDefinition NVARCHAR(MAX)  -- e.g. '[ColA] NVARCHAR(100) NOT NULL'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(MAX)
      = N'ALTER TABLE ' + QUOTENAME(@SchemaName) + N'.'
        + QUOTENAME(@TableName)
        + N' ALTER COLUMN ' + @ColumnDefinition + N';';
    EXEC sp_executesql @sql;
END;
GO
