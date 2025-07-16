IF OBJECT_ID('dbo.spSetImport','P') IS NOT NULL
    DROP PROCEDURE dbo.spSetImport;
GO
CREATE PROCEDURE dbo.spSetImport
    @Initialized BIT
AS
BEGIN
    MERGE dbo.BankHolidayImport AS tgt
    USING (SELECT 'UK' AS ImportName, @Initialized AS Initialized) AS src
        ON tgt.ImportName=src.ImportName
    WHEN MATCHED THEN 
        UPDATE SET Initialized=src.Initialized, LastRun=SYSDATETIME()
    WHEN NOT MATCHED THEN
        INSERT(ImportName, Initialized, LastRun) VALUES(src.ImportName, src.Initialized, SYSDATETIME());
END
GO