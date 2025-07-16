IF OBJECT_ID('dbo.spGetRegions','P') IS NOT NULL
    DROP PROCEDURE dbo.spGetRegions;
GO
CREATE PROCEDURE dbo.spGetRegions
AS
BEGIN
    SELECT RegionId, RegionKey, RegionName FROM dbo.Region ORDER BY RegionName;
END
GO