IF OBJECT_ID('dbo.DropIndexOnTable', 'P') IS NOT NULL
    DROP PROCEDURE dbo.DropIndexOnTable;
GO

CREATE PROCEDURE dbo.DropIndexOnTable
    @SchemaName SYSNAME       = 'dbo',      -- can parameterize this if needed
    @TableName  SYSNAME,
    @IndexName  SYSNAME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sql NVARCHAR(MAX)
        = N'DROP INDEX '
        + QUOTENAME(@IndexName)
        + N' ON '
        + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName);

    EXEC sp_executesql @sql;
END;
GO
