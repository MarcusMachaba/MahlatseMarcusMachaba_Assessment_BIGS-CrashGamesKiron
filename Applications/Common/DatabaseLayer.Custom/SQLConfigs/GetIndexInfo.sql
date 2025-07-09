IF OBJECT_ID('dbo.GetIndexInfo', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetIndexInfo;
GO

CREATE PROCEDURE dbo.GetIndexInfo 
    @IndexName SYSNAME

AS
BEGIN
    SET NOCOUNT ON;
  
    SELECT 
        t.name      AS TableName,
        ind.is_unique AS IsUnique
    FROM sys.indexes ind
    INNER JOIN sys.tables t 
        ON ind.object_id = t.object_id
    WHERE ind.name = @IndexName;
END;
GO