IF OBJECT_ID('dbo.CheckStoredProcedureExists','P') IS NOT NULL
    DROP PROCEDURE dbo.CheckStoredProcedureExists;
GO

CREATE PROCEDURE dbo.CheckStoredProcedureExists
    @ObjectName SYSNAME                               -- e.g. '[dbo].[MyProc]'
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM sys.objects
        WHERE object_id = OBJECT_ID(@ObjectName)
          AND type IN (N'P', N'PC')
    )
        SELECT 1;
    ELSE
        SELECT 0;
END;
GO