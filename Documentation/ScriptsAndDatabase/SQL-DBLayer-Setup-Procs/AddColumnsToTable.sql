IF OBJECT_ID('dbo.AddColumnsToTable','P') IS NOT NULL
    DROP PROCEDURE dbo.AddColumnsToTable;
GO
CREATE PROCEDURE dbo.AddColumnsToTable
    @SchemaName       SYSNAME       = 'dbo',
    @TableName        SYSNAME,
    @ColumnDefinitions NVARCHAR(MAX)  -- e.g. '[ColA] INT NOT NULL, [ColB] NVARCHAR(50) NULL'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(MAX)
      = N'ALTER TABLE ' + QUOTENAME(@SchemaName) + N'.'
        + QUOTENAME(@TableName)
        + N' ADD ' + @ColumnDefinitions + N';';
    EXEC sp_executesql @sql;
END;
GO