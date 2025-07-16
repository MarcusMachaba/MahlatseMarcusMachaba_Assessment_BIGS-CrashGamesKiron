USE [KironTest]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RegionHoliday](
	[RegionId] [int] NOT NULL,
	[HolidayId] [int] NOT NULL,
 CONSTRAINT [PK_RegionHoliday] PRIMARY KEY CLUSTERED 
(
	[RegionId] ASC,
	[HolidayId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RegionHoliday]  WITH CHECK ADD  CONSTRAINT [FK_RegionHoliday_HolidayId_Holiday_HolidayId] FOREIGN KEY([HolidayId])
REFERENCES [dbo].[Holiday] ([HolidayId])
GO

ALTER TABLE [dbo].[RegionHoliday] CHECK CONSTRAINT [FK_RegionHoliday_HolidayId_Holiday_HolidayId]
GO


