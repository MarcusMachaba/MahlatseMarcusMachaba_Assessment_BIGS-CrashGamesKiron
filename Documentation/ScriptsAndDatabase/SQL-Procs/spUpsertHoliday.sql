IF OBJECT_ID('dbo.spUpsertHoliday','P') IS NOT NULL
    DROP PROCEDURE dbo.spUpsertHoliday;
GO
CREATE PROCEDURE dbo.spUpsertHoliday
    @HolidayDate DATE,
    @Title       NVARCHAR(200)
AS
BEGIN
    MERGE dbo.Holiday AS tgt
    USING (SELECT @HolidayDate,@Title) AS src(HolidayDate,Title)
        ON tgt.HolidayDate = src.HolidayDate AND tgt.Title = src.Title
    WHEN MATCHED THEN 
        UPDATE SET Title = src.Title
    WHEN NOT MATCHED THEN
        INSERT(HolidayDate,Title) VALUES(src.HolidayDate,src.Title);
END
GO