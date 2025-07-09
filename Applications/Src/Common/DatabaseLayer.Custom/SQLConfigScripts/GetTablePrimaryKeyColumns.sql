IF OBJECT_ID('dbo.GetTablePrimaryKeyColumns', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetTablePrimaryKeyColumns;
GO

CREATE PROCEDURE dbo.GetTablePrimaryKeyColumns 
    @TableName  SYSNAME                                             -- SYSNAME is aliased to NVARCHAR(128)     
AS
BEGIN
  SET NOCOUNT ON;                                                   -- prevent the “(1 row(s) affected)” messages             

  SELECT 
        ccu.TABLE_NAME,
        ccu.COLUMN_NAME,
        ccu.CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
    INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE AS ccu 
    ON tc.CONSTRAINT_NAME = ccu.CONSTRAINT_NAME
    WHERE tc.TABLE_NAME = @TableName
        AND ccu.CONSTRAINT_NAME LIKE 'PK_%';
END
GO