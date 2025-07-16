IF OBJECT_ID('dbo.DropColumnFromTable','P') IS NOT NULL
    DROP PROCEDURE dbo.DropColumnFromTable;
GO
CREATE PROCEDURE dbo.DropColumnFromTable
    @SchemaName SYSNAME       = 'dbo',
    @TableName  SYSNAME,
    @ColumnName SYSNAME
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(MAX)
      = N'ALTER TABLE ' + QUOTENAME(@SchemaName) + N'.'
        + QUOTENAME(@TableName)
        + N' DROP COLUMN ' + QUOTENAME(@ColumnName) + N';';
    EXEC sp_executesql @sql;
END;
GO