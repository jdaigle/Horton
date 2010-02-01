
/****** Object:  View [dbo].[Category Sales for 1997]    Script Date: 02/01/2010 14:52:30 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[Category Sales for 1997]'))
DROP VIEW [dbo].[Category Sales for 1997]
GO


create view [dbo].[Category Sales for 1997] AS
SELECT "Product Sales for 1997".CategoryName, Sum("Product Sales for 1997".ProductSales) AS CategorySales
FROM "Product Sales for 1997"
GROUP BY "Product Sales for 1997".CategoryName

GO


