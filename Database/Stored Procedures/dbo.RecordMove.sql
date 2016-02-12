SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [dbo].[RecordMove]
    @fen NVARCHAR(MAX) ,
    @status NVARCHAR(MAX) ,
    @lastMove NVARCHAR(10) ,
    @gameId INT ,
    @lastActionBlack DATETIME = NULL ,
    @lastActionWhite DATETIME = NULL
AS
    BEGIN
    	SET XACT_ABORT ON
        BEGIN TRANSACTION;
        UPDATE  dbo.Boards
        SET     Fen = @fen ,
                Status = @status ,
                LastMove = @lastMove
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

        UPDATE  dbo.Clocks
        SET     LastActionBlack = @lastActionBlack ,
                LastActionWhite = @lastActionWhite
        WHERE   GameId = @gameId;
		
        COMMIT;
    END;
GO
GRANT EXECUTE ON  [dbo].[RecordMove] TO [chessdb]
GO
