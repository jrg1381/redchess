SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE FUNCTION [dbo].[MayClaimDraw] 
(@gameId INT)
RETURNS INT
BEGIN
IF EXISTS(SELECT COUNT(*) AS RepeatCount
FROM    dbo.HistoryEntries
WHERE   GameId = @gameId
GROUP BY dbo.TrimAtLastSpace(Fen)
HAVING  COUNT(*) >= 3)
OR 50 < (SELECT TRY_PARSE(SUBSTRING(Fen ,LEN(Fen) - CHARINDEX(' ',REVERSE(Fen)) + 1, 4) AS INT) FROM dbo.BoardDtoes WHERE GameId = @gameId)

RETURN 1;

RETURN 0;
END
GO
