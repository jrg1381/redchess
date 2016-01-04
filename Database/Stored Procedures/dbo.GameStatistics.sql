SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [dbo].[GameStatistics] AS
BEGIN
DECLARE @AllPairs TABLE
    (
      UserId1 INT NOT NULL ,
      UserId2 INT NOT NULL
    );

INSERT  INTO @AllPairs
        SELECT  X.UserId AS UserId1 ,
                Y.UserId AS UserId2
        FROM    dbo.UserProfile AS Y
                CROSS JOIN dbo.UserProfile AS X
        WHERE   X.UserId != Y.UserId; -- Discard matches with self

SELECT  BlackProfile.UserName AS Black ,
        WhiteProfile.UserName AS White ,
        WinnerProfile.UserName AS Winner,
        COUNT(Wins.GameId) AS Count
FROM    @AllPairs AS AllPairs
        JOIN dbo.Boards AS Wins ON Wins.UserIdBlack = AllPairs.UserId1
                                   AND Wins.UserIdWhite = AllPairs.UserId2
        JOIN dbo.UserProfile AS BlackProfile ON BlackProfile.UserId = Wins.UserIdBlack
        JOIN dbo.UserProfile AS WhiteProfile ON WhiteProfile.UserId = Wins.UserIdWhite
        LEFT OUTER JOIN dbo.UserProfile AS WinnerProfile ON WinnerProfile.UserId = Wins.UserIdWinner
GROUP BY BlackProfile.UserName ,
        WhiteProfile.UserName ,
        WinnerProfile.UserName;
END

GO
