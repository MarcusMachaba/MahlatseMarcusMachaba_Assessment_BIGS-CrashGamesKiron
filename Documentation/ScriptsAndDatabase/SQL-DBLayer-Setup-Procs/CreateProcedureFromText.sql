IF OBJECT_ID('dbo.CreateProcedureFromText','P') IS NOT NULL
    DROP PROCEDURE dbo.CreateProcedureFromText;
GO
CREATE PROCEDURE dbo.CreateProcedureFromText
    @ProcedureCreateText NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    EXEC sp_executesql @ProcedureCreateText;
END;
GO