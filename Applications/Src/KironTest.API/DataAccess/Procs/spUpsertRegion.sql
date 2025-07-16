IF OBJECT_ID('dbo.spUpsertRegion','P') IS NOT NULL
    DROP PROCEDURE dbo.spUpsertRegion;
GO
CREATE PROCEDURE dbo.spUpsertRegion
    @RegionKey NVARCHAR(50),
    @RegionName NVARCHAR(100)
AS
BEGIN
    MERGE dbo.Region AS tgt
    USING (SELECT @RegionKey AS RegionKey, @RegionName AS RegionName) AS src
        ON tgt.RegionKey = src.RegionKey
    WHEN MATCHED THEN 
        UPDATE SET RegionName = src.RegionName
    WHEN NOT MATCHED THEN
        INSERT(RegionKey,RegionName) VALUES(src.RegionKey,src.RegionName);
END
GO