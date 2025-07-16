IF OBJECT_ID('dbo.spUpsertRegionHoliday','P') IS NOT NULL
    DROP PROCEDURE dbo.spUpsertRegionHoliday;
GO
CREATE PROCEDURE dbo.spUpsertRegionHoliday
    @RegionKey   NVARCHAR(50),
    @HolidayDate DATE,
    @Title       NVARCHAR(200)
AS
BEGIN
    DECLARE @rid INT, @hid INT;
    SELECT @rid = RegionId FROM dbo.Region     WHERE RegionKey = @RegionKey;
    SELECT @hid = HolidayId FROM dbo.Holiday   WHERE HolidayDate=@HolidayDate AND Title=@Title;
    IF NOT EXISTS(SELECT 1 FROM dbo.RegionHoliday  WHERE RegionId=@rid AND HolidayId=@hid)
    INSERT dbo.RegionHoliday(RegionId,HolidayId) VALUES(@rid,@hid);
END
GO