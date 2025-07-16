USE [KironTest]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BankHolidayImport](
	[ImportName] [varchar](50) NOT NULL,
	[Initialized] [bit] NULL,
	[LastRun] [date] NULL,
 CONSTRAINT [PK_BankHolidayImport_ImportName] PRIMARY KEY CLUSTERED 
(
	[ImportName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BankHolidayImport] ADD  CONSTRAINT [DF_BankHolidayImport_Initialized]  DEFAULT ((0)) FOR [Initialized]
GO


