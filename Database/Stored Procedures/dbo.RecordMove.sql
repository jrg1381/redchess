
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO





CREATE PROCEDURE [dbo].[RecordMove]
    @gameId INT ,
    @fen NVARCHAR(MAX) ,
    @lastMove NVARCHAR(10) ,
    @moveReceived DATETIME,
	@status NVARCHAR(32),
	@gameOver BIT,
	@userIdWinner INT
AS
    BEGIN
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        UPDATE  dbo.Boards
        SET     Fen = @fen ,
                LastMove = @lastMove ,
                MoveNumber = MoveNumber + 1,
				Status = @status,
				CompletionDate = CASE(@gameOver) WHEN 1 THEN @moveReceived ELSE CompletionDate END,
				GameOver = @gameOver,
				UserIdWinner = @userIdWinner
        WHERE   GameId = @gameId;

        INSERT  INTO dbo.HistoryEntries
                ( GameId ,
                  MoveNumber ,
                  Fen ,
                  Move
                )
        VALUES  ( @gameId , -- GameId - int
                  ( SELECT TOP 1
                            MoveNumber
                    FROM    dbo.Boards
                    WHERE   GameId = @gameId
                  ) , -- MoveNumber - int
                  @fen , -- Fen - nvarchar(max)
                  @lastMove  -- Move - nvarchar(10)
                );

        IF CHARINDEX('w', @fen) > 0 -- It is white's turn now
            UPDATE  dbo.Clocks
            SET     LastActionWhite = @moveReceived ,
                    TimeElapsedBlackMs = TimeElapsedBlackMs + DATEDIFF(ms,
                                                              LastActionBlack,
                                                              @moveReceived)
            WHERE   GameId = @gameId; 
        ELSE
            UPDATE  dbo.Clocks
            SET     LastActionBlack = @moveReceived ,
                    TimeElapsedWhiteMs = TimeElapsedWhiteMs + DATEDIFF(ms,
                                                              LastActionWhite,
                                                              @moveReceived)
            WHERE   GameId = @gameId;

        COMMIT;

		SELECT TOP 1 * FROM dbo.Boards WHERE GameId = @gameId
    END;



GO







GRANT EXECUTE ON  [dbo].[RecordMove] TO [chessdb]
GO
