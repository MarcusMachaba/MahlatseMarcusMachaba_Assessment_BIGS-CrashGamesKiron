IF OBJECT_ID('dbo.CreateIndexOnTable', 'P') IS NOT NULL
    DROP PROCEDURE dbo.CreateIndexOnTable;
GO

CREATE PROCEDURE dbo.CreateIndexOnTable
    @SchemaName      SYSNAME       = 'dbo',
    @TableName       SYSNAME,
    @IndexName       SYSNAME,
    @IsUnique        BIT,
    @KeyColumns      NVARCHAR(MAX),                                 -- e.g. '[ColA] ASC, [ColB] DESC'
    @IncludedColumns NVARCHAR(MAX) = ''                             -- e.g. '[Incl1], [Incl2]'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sql NVARCHAR(MAX)
        = N'CREATE '
        + CASE WHEN @IsUnique = 1 THEN N'UNIQUE ' ELSE N'' END
        + N'NONCLUSTERED INDEX '
        + QUOTENAME(@IndexName)
        + N' ON '
        + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName)
        + N' (' + @KeyColumns + N')'
        + CASE 
            WHEN LEN(@IncludedColumns) > 0 
            THEN N' INCLUDE (' + @IncludedColumns + N')' 
            ELSE N'' 
          END
        + N' WITH ('
        + N'PAD_INDEX = OFF, '
        + N'STATISTICS_NORECOMPUTE = OFF, '
        + N'SORT_IN_TEMPDB = OFF, '
        + N'DROP_EXISTING = OFF, '
        + N'ONLINE = OFF, '
        + N'ALLOW_ROW_LOCKS = ON, '
        + N'ALLOW_PAGE_LOCKS = ON'
        + N') ON [PRIMARY]';

    EXEC sp_executesql @sql;
END;
GO