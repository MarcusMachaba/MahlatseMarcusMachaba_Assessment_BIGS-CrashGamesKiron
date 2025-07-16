IF OBJECT_ID('dbo.ExecuteDynamicSql','P') IS NOT NULL
    DROP PROCEDURE dbo.ExecuteDynamicSql;
GO
CREATE PROCEDURE dbo.ExecuteDynamicSql
    @Sql NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    EXEC sp_executesql @Sql;
END;
GO