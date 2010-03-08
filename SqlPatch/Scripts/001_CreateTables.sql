CREATE TABLE [dbo].[ProductsEx] (
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Inventory] [int] NOT NULL,
	[Notes] [text] NULL,
)