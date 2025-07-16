IF OBJECT_ID('dbo.GetIndexColumns', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetIndexColumns;
GO

CREATE PROCEDURE dbo.GetIndexColumns
    @IndexName SYSNAME
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        CASE ic.is_descending_key WHEN 1 THEN 'DESC' ELSE 'ASC' END AS Sorting,
        col.name                                       AS [Column]
    FROM sys.indexes ind
    LEFT JOIN sys.index_columns ic 
        ON ind.object_id = ic.object_id 
       AND ind.index_id   = ic.index_id
    LEFT JOIN sys.columns col 
        ON ic.object_id   = col.object_id 
       AND ic.column_id   = col.column_id
    WHERE ind.name             = @IndexName
      AND ic.is_included_column = 0
    ORDER BY ic.key_ordinal;
END;
GO