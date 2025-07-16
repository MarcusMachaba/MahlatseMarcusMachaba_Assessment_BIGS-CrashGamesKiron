IF OBJECT_ID('dbo.spCheckImport','P') IS NOT NULL
    DROP PROCEDURE dbo.spCheckImport;
GO
CREATE PROCEDURE dbo.spCheckImport
AS
BEGIN
    DECLARE @flag BIT = 0;

    SELECT @flag = Initialized
    FROM dbo.BankHolidayImport
    WHERE ImportName = 'UK';

    -- if that SELECT returned no rows, @flag stays 0
    SELECT @flag AS Initialized;
END
GO