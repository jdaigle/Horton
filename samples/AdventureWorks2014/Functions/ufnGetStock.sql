IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetStock]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[ufnGetStock]
GO

CREATE FUNCTION [dbo].[ufnGetStock](@ProductID [int])
RETURNS [int] 
AS 
-- Returns the stock level for the product. This function is used internally only
BEGIN
    DECLARE @ret int;
    
    SELECT @ret = SUM(p.[Quantity]) 
    FROM [Production].[ProductInventory] p 
    WHERE p.[ProductID] = @ProductID 
        AND p.[LocationID] = '6'; -- Only look at inventory in the misc storage
    
    IF (@ret IS NULL) 
        SET @ret = 0
    
    RETURN @ret
END;
GO