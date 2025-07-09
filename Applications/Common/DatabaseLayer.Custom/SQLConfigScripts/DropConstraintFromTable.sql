IF OBJECT_ID('dbo.DropConstraintFromTable','P') IS NOT NULL
    DROP PROCEDURE dbo.DropConstraintFromTable;
GO
CREATE PROCEDURE dbo.DropConstraintFromTable
    @SchemaName     SYSNAME       = 'dbo',
    @TableName      SYSNAME,
    @ConstraintName SYSNAME
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(MAX)
      = N'ALTER TABLE ' + QUOTENAME(@SchemaName) + N'.'
        + QUOTENAME(@TableName)
        + N' DROP CONSTRAINT ' + QUOTENAME(@ConstraintName) + N';';
    EXEC sp_executesql @sql;
END;
GO