IF OBJECT_ID('dbo.AlterColumnInTable','P') IS NOT NULL
    DROP PROCEDURE dbo.AlterColumnInTable;
GO

CREATE PROCEDURE dbo.AlterColumnInTable
    @SchemaName       SYSNAME       = N'dbo',           -- e.g. dbo
    @TableName        SYSNAME,                         -- e.g. MyTable
    @ColumnDefinition NVARCHAR(MAX)  = NULL            -- e.g. "[Col] NVARCHAR(50) NULL DEFAULT('X')"
AS
BEGIN
    SET NOCOUNT ON;

    ----------------------------------------------------
    -- 1) Build the full object name
    ----------------------------------------------------
    DECLARE @FullName NVARCHAR(258);
    SET @FullName = QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName);

    ----------------------------------------------------
    -- 2) Extract the column name from [Col]
    ----------------------------------------------------
    DECLARE @ColumnName SYSNAME;
    SET @ColumnName = SUBSTRING(
        @ColumnDefinition,
        2,
        CHARINDEX(N']', @ColumnDefinition) - 2
    );

    ----------------------------------------------------
    -- 3) Verify the column actually exists
    ----------------------------------------------------
    IF NOT EXISTS (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID(@FullName)
          AND name      = @ColumnName
    )
    BEGIN
        RAISERROR(
          'Column "%s" not found in %s',
          16, 1,
          @ColumnName, @FullName
        );
        RETURN;
    END

    ----------------------------------------------------
    -- 4) Drop any existing DEFAULT constraint
    ----------------------------------------------------
    DECLARE @DefName SYSNAME;
    SELECT @DefName = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c 
      ON dc.parent_object_id = c.object_id
     AND dc.parent_column_id  = c.column_id
    WHERE dc.parent_object_id = OBJECT_ID(@FullName)
      AND c.name               = @ColumnName;

    IF @DefName IS NOT NULL
    BEGIN
        DECLARE @dropSql NVARCHAR(MAX)
            = N'ALTER TABLE ' + @FullName
            + N' DROP CONSTRAINT ' + QUOTENAME(@DefName) + N';';
        EXEC(@dropSql);
    END

    ----------------------------------------------------
    -- 5) Pull out a DEFAULT(...) clause if present
    ----------------------------------------------------
    DECLARE @DefaultPos    INT;
    DECLARE @DefaultClause NVARCHAR(MAX);

    SET @DefaultPos = CHARINDEX(N'DEFAULT', @ColumnDefinition);
    IF @DefaultPos > 0
    BEGIN
        SET @DefaultClause =
            LTRIM(RTRIM(
              SUBSTRING(
                @ColumnDefinition,
                @DefaultPos,
                LEN(@ColumnDefinition) - @DefaultPos + 1
              )
            ));

        SET @ColumnDefinition =
            LTRIM(RTRIM(
              LEFT(@ColumnDefinition, @DefaultPos - 1)
            ));
    END
    ELSE
        SET @DefaultClause = NULL;

    ----------------------------------------------------
    -- 6) Perform the ALTER COLUMN
    ----------------------------------------------------
    DECLARE @alterSql NVARCHAR(MAX)
        = N'ALTER TABLE ' + @FullName
        + N' ALTER COLUMN ' + @ColumnDefinition + N';';
    EXEC sp_executesql @alterSql;

    ----------------------------------------------------
    -- 7) Re-add the DEFAULT if we pulled one out
    ----------------------------------------------------
    IF @DefaultClause IS NOT NULL
    BEGIN
        DECLARE @addDefSql NVARCHAR(MAX)
            = N'ALTER TABLE ' + @FullName
            + N' ADD CONSTRAINT DF_' + @TableName + N'_' + @ColumnName
            + N' ' + @DefaultClause
            + N' FOR ' + QUOTENAME(@ColumnName) + N';';
        EXEC sp_executesql @addDefSql;
    END
END;
GO
