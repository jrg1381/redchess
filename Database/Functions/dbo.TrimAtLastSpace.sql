SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE FUNCTION [dbo].[TrimAtLastSpace]
(@input NVARCHAR(MAX))
RETURNS NVARCHAR(max)
BEGIN
RETURN LEFT(@input ,LEN(@input) - CHARINDEX(' ',REVERSE(@input)))
END
GO
