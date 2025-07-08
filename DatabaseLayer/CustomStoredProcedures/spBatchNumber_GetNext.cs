using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseLayer.CustomStoredProcedures
{
    internal class spBatchNumber_GetNext : IStoredProcedure
    {
        public spBatchNumber_GetNext(BaseDataProvider provider) => this.Provider = provider;

        public string StoredProcedureName => nameof(spBatchNumber_GetNext);

        public BaseDataProvider Provider { get; set; }

        public string StoredProcedureCreateText => "\r\nCREATE PROCEDURE spBatchNumber_GetNext\r\n(\r\n\t@BatchNumberType INT,\r\n\t@NumberOfBatchNumbers INT\r\n)\r\nAS\r\nBEGIN\r\n\tDECLARE @BatchNumbers TABLE \r\n\t(\r\n\t\tBatchNumber VARCHAR(255)\r\n\t)\r\n\r\n\tDECLARE @LockName VARCHAR(20) = 'BATCH' + RTRIM(@BatchNumberType);\r\n\r\n\tBEGIN TRANSACTION\r\n\tEXEC sp_getapplock @Resource = @LockName, @LockMode = 'Exclusive'; \r\n\t\r\n\tDECLARE @StartingBatchNumber VARCHAR(255)\r\n\tSELECT @StartingBatchNumber = CurrentBatchNumber FROM dbo." + this.Provider.GetTableName(typeof(BatchNumber)) + " WHERE BatchNumberType = @BatchNumberType\r\n\t\r\n\tDECLARE @Iteration INT = 0\r\n\tWHILE (@Iteration < @NumberOfBatchNumbers)\r\n\tBEGIN\r\n\t\tDECLARE @NewBatchNumber VARCHAR(255) = ''\r\n\t\t\r\n\t\tDECLARE @CurrentBatchNumber VARCHAR(255) = @StartingBatchNumber\r\n\r\n\t\t\r\n\t\tDECLARE @StillUpdating BIT = 1\r\n\t\tDECLARE @CurrentCharacter CHAR(1)\r\n\r\n\t\tWHILE(LEN(@CurrentBatchNumber) > 0 AND @StillUpdating = 1) \r\n\t\tBEGIN\r\n\t\t\tSET @CurrentCharacter = RIGHT(@CurrentBatchNumber, 1)\r\n\t\t\tIF (@CurrentCharacter LIKE '[0-8]' OR @CurrentCharacter LIKE '[A-Y]')\r\n\t\t\tBEGIN\r\n\t\t\t\tSET @CurrentCharacter = CHAR(ASCII(@CurrentCharacter) + 1)\r\n\t\t\t\tSET @StillUpdating = 0\r\n\t\t\tEND\r\n\t\t\tELSE IF (ASCII(@CurrentCharacter) = ASCII('Z'))\r\n\t\t\tBEGIN\r\n\t\t\t\tSET @CurrentCharacter = 'a';\r\n\t\t\tEND\r\n\t\t\tELSE IF (ASCII(@CurrentCharacter) = ASCII('z'))\r\n\t\t\tBEGIN\r\n\t\t\t\tSET @CurrentCharacter = 'A';\r\n\t\t\t\tSET @StillUpdating = 0;\r\n\t\t\tEND\r\n\t\t\tELSE IF (ASCII(@CurrentCharacter) = ASCII('9'))\r\n\t\t\tBEGIN\r\n\t\t\t\tSET @CurrentCharacter = '0';\r\n\t\t\tEND\r\n\t\t\tSET @NewBatchNumber = @CurrentCharacter + @NewBatchNumber\r\n\t\t\tSET @CurrentBatchNumber = LEFT(@CurrentBatchNumber, LEN(@CurrentBatchNumber) - 1)\r\n\t\tEND\r\n\r\n\t\tINSERT INTO @BatchNumbers VALUES (@CurrentBatchNumber + @NewBatchNumber)\r\n\t\tSET @StartingBatchNumber = @CurrentBatchNumber + @NewBatchNumber;\r\n\t\tSET @Iteration = @Iteration + 1\r\n\tEND\r\n\t\r\n\tUPDATE dbo." + this.Provider.GetTableName(typeof(BatchNumber)) + " SET CurrentBatchNumber = @StartingBatchNumber WHERE BatchNumberType = @BatchNumberType\r\n\r\n\tEXEC sp_releaseapplock @Resource = @LockName\r\n\tCOMMIT TRANSACTION\r\n\r\n\tSELECT BatchNumber FROM @BatchNumbers\r\nEND\r\n";
    }
}
