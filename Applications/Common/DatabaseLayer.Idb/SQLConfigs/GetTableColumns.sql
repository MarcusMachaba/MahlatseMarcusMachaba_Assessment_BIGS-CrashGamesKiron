IF OBJECT_ID('dbo.GetTableColumns', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetTableColumns;
GO

CREATE PROCEDURE dbo.GetTableColumns 
    @TableName SYSNAME                                              -- SYSNAME ≃ NVARCHAR(128)

AS
BEGIN
  SET NOCOUNT ON;                                                   -- Prevent the "1 row(s) affected" messages
  SELECT
        COLUMN_NAME,
        IS_NULLABLE,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH,
        COLUMN_DEFAULT,
        NUMERIC_PRECISION,
        NUMERIC_SCALE
   FROM 
        INFORMATION_SCHEMA.COLUMNS
   WHERE 
        TABLE_NAME = @TableName;
END;
GO