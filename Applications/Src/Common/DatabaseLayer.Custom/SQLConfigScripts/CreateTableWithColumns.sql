IF OBJECT_ID('dbo.CreateTableWithColumns','P') IS NOT NULL
    DROP PROCEDURE dbo.CreateTableWithColumns;
GO
CREATE PROCEDURE dbo.CreateTableWithColumns
    @SchemaName            SYSNAME       = 'dbo',
    @TableName             SYSNAME,
    @ColumnDefinitions     NVARCHAR(MAX),  -- comma-separated column lines, e.g. '[A] INT NOT NULL, [B] VARCHAR(50) NULL'
    @PrimaryKeyDefinition  NVARCHAR(MAX),  -- e.g. 'CONSTRAINT [PK_Tbl_A] PRIMARY KEY CLUSTERED ([A] ASC) WITH(...options...)'
    @FileGroup             SYSNAME       = 'PRIMARY'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(MAX)
      = N'CREATE TABLE ' + QUOTENAME(@SchemaName) + N'.'
        + QUOTENAME(@TableName)
        + N' (' + CHAR(13)+CHAR(10)
        + @ColumnDefinitions + N',' + CHAR(13)+CHAR(10)
        + @PrimaryKeyDefinition + CHAR(13)+CHAR(10)
        + N') ON ' + QUOTENAME(@FileGroup) + N';';
    EXEC sp_executesql @sql;
END;
GO