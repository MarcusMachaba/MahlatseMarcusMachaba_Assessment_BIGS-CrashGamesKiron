IF OBJECT_ID('dbo.sp_GetTableColumns', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetTableColumns;
GO

CREATE PROCEDURE dbo.sp_GetTableColumns (
    @TableName NVARCHAR(128)
)
AS
BEGIN
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
END