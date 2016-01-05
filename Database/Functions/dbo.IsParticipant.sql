
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO


CREATE FUNCTION [dbo].[IsParticipant]
    (
      @gameId INT ,
      @userName NVARCHAR(MAX)
    )
RETURNS BIT
AS
    BEGIN
        IF EXISTS ( SELECT  NULL
                    FROM    dbo.Boards
                            JOIN dbo.UserProfile ON dbo.Boards.UserIdWhite = dbo.UserProfile.UserId
                                                    OR dbo.Boards.UserIdBlack = dbo.UserProfile.UserId
                    WHERE   dbo.UserProfile.UserName = @userName
                            AND dbo.Boards.GameId = @gameId )
            BEGIN
                RETURN CAST(1 AS BIT)
            END

        RETURN CAST(0 AS BIT)
    END




GO

GRANT EXECUTE ON  [dbo].[IsParticipant] TO [chessdb]
GO
