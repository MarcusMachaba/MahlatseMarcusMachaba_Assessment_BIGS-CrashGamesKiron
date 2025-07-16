IF OBJECT_ID('dbo.GetIncludedIndexColumns', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetIncludedIndexColumns;
GO

CREATE PROCEDURE dbo.GetIncludedIndexColumns
    @IndexName SYSNAME
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        col.name AS [Column]
    FROM sys.indexes ind
    LEFT JOIN sys.index_columns ic 
        ON ind.object_id = ic.object_id 
       AND ind.index_id   = ic.index_id
    LEFT JOIN sys.columns col 
        ON ic.object_id   = col.object_id 
       AND ic.column_id   = col.column_id
    WHERE ind.name             = @IndexName
      AND ic.is_included_column = 1
    ORDER BY ic.key_ordinal;
END;
GO