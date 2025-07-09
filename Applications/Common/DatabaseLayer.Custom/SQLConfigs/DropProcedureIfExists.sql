IF OBJECT_ID('dbo.DropProcedureIfExists','P') IS NOT NULL
    DROP PROCEDURE dbo.DropProcedureIfExists;
GO
CREATE PROCEDURE dbo.DropProcedureIfExists
    @SchemaName    SYSNAME       = 'dbo',
    @ProcedureName SYSNAME
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(MAX)
      = N'IF EXISTS (SELECT * FROM sys.objects 
                      WHERE object_id = OBJECT_ID(N'''
        + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@ProcedureName)
        + N''') AND type IN (N''P'',N''PC'')) '
        + N'DROP PROCEDURE ' + QUOTENAME(@SchemaName) + N'.'
        + QUOTENAME(@ProcedureName) + N';';
    EXEC sp_executesql @sql;
END;
GO