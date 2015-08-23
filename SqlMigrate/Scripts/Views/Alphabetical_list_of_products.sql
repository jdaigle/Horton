/****** Object:  View [dbo].[Alphabetical list of products]    Script Date: 02/01/2010 14:51:06 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[Alphabetical list of products]'))
DROP VIEW [dbo].[Alphabetical list of products]
GO

/****** Object:  View [dbo].[Alphabetical list of products]    Script Date: 02/01/2010 14:51:06 ******/
create view [dbo].[Alphabetical list of products] AS
SELECT Products.*, Categories.CategoryName
FROM Categories INNER JOIN Products ON Categories.CategoryID = Products.CategoryID
WHERE (((Products.Discontinued)=0))
GO