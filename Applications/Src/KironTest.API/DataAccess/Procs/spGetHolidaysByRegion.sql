IF OBJECT_ID('dbo.spGetHolidaysByRegion','P') IS NOT NULL
    DROP PROCEDURE dbo.spGetHolidaysByRegion;
GO
CREATE PROCEDURE dbo.spGetHolidaysByRegion
    @RegionId INT
AS
BEGIN
    SELECT h.HolidayId,h.HolidayDate,h.Title
    FROM dbo.Holiday h
    INNER JOIN dbo.RegionHoliday rh ON rh.HolidayId = h.HolidayId
    WHERE rh.RegionId = @RegionId
    ORDER BY h.HolidayDate;
END
GO