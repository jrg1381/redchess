
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE PROCEDURE [dbo].[UpdateEloTable] AS
BEGIN
SET TRANSACTION ISOLATION LEVEL READ COMMITTED
BEGIN TRANSACTION
DELETE FROM dbo.EloHistory
DECLARE @Users TABLE
    (
      Id INT NOT NULL
             PRIMARY KEY ,
      Elo INT NOT NULL
    );

INSERT  INTO @Users
        SELECT  UserId ,
                900
        FROM    dbo.UserProfile;

DECLARE GameCursor CURSOR FORWARD_ONLY
FOR
    SELECT  UserIdWhite ,
            UserIdBlack ,
            UserIdWinner ,
            CompletionDate
    FROM    dbo.Boards
    WHERE   GameOver = 1 AND UserIdBlack != UserIdWhite -- Ignore analysis boards
    ORDER BY CompletionDate FOR READ ONLY;

/* Data from the Boards table */
DECLARE @b AS INT;
 -- userid of black
DECLARE @w AS INT;
 -- userid of white
DECLARE @winner AS INT;
 -- userid of winner
DECLARE @completionDate AS DATETIME;
 -- when game completed

/* Current Elo values for players */
DECLARE @whiteElo AS INT;
DECLARE @blackElo AS INT;

/* Expectation values */
DECLARE @evWhite AS FLOAT;
DECLARE @evBlack AS FLOAT;

/* Default score (no winner) */
DECLARE @scoreWhite AS FLOAT = 0.5;
DECLARE @scoreBlack AS FLOAT = 0.5;

/* Temporary calculation variables */
DECLARE @denominator AS FLOAT;
DECLARE @transformedBlackElo AS FLOAT;
DECLARE @transformedWhiteElo AS FLOAT;

OPEN GameCursor;
FETCH NEXT FROM GameCursor INTO @w, @b, @winner, @completionDate;
WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT  @whiteElo = ( SELECT    Player.Elo
                              FROM      @Users AS Player
                              WHERE     Player.Id = @w
                            );

        SELECT  @blackElo = ( SELECT    Player.Elo
                              FROM      @Users AS Player
                              WHERE     Player.Id = @b
                            );

        SELECT  @transformedBlackElo = POWER(10, @blackElo / 400);
        SELECT  @transformedWhiteElo = POWER(10, @whiteElo / 400);

        SELECT  @denominator = @transformedBlackElo + @transformedWhiteElo;
        SELECT  @evBlack = @transformedBlackElo / @denominator;
        SELECT  @evWhite = @transformedWhiteElo / @denominator;

        IF @winner = @w
            BEGIN
                SELECT  @scoreBlack = 0;
                SELECT  @scoreWhite = 1;
            END;
        
        IF @winner = @b
            BEGIN
                SELECT  @scoreBlack = 1;
                SELECT  @scoreWhite = 0;
            END;

        UPDATE  @Users
        SET     Elo = Elo + 32 * ( @scoreBlack - @evBlack )
        WHERE   Id = @b;
        UPDATE  @Users
        SET     Elo = Elo + 32 * ( @scoreWhite - @evWhite )
        WHERE   Id = @w;
    
        INSERT  INTO EloHistory
                ( Date, UserId, Elo )
        VALUES  ( @completionDate, @w, @whiteElo ),
                ( @completionDate, @b, @blackElo );

        FETCH NEXT FROM GameCursor INTO @w, @b, @winner, @completionDate;
    END;
CLOSE GameCursor;

DEALLOCATE GameCursor;

DELETE FROM dbo.Metadata WHERE [Key] = 'LastEloHistoryUpdate';
INSERT INTO dbo.Metadata ([Key], [Value]) VALUES ('LastEloHistoryUpdate',CONVERT(nvarchar(30), GETUTCDATE(), 126));

COMMIT TRANSACTION
END


GO
