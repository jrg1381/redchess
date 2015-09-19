CREATE TABLE [dbo].[Boards]
(
[GameId] [int] NOT NULL IDENTITY(1, 1),
[UserIdWhite] [int] NOT NULL,
[UserIdBlack] [int] NOT NULL,
[Status] [nvarchar] (max) NULL,
[GameOver] [bit] NOT NULL,
[Fen] [nvarchar] (max) NULL,
[CreationDate] [datetime] NOT NULL DEFAULT (getutcdate()),
[CompletionDate] [datetime] NOT NULL DEFAULT (getutcdate()),
[LastMove] [nvarchar] (10) NULL,
[MayClaimDraw] AS (case  when [GameOver]=(1) then CONVERT([bit],(0),(0)) else [dbo].[MayClaimDraw]([GameId]) end),
[UserIdWinner] [int] NULL,
[MoveNumber] [int] NOT NULL DEFAULT ((0))
)
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE TRIGGER [dbo].[MoveTrigger] ON [dbo].[Boards]
    AFTER UPDATE
AS
    BEGIN
        DECLARE @gameId INT ,
            @moveNumber INT;
        SELECT  @gameId = ( SELECT TOP 1
                                    Inserted.GameId
                            FROM    Inserted
                                    INNER JOIN deleted ON inserted.GameId = deleted.GameId
                            WHERE   Inserted.Fen <> Deleted.Fen
                          );
        SELECT  @moveNumber = ( SELECT  Inserted.MoveNumber
                                FROM    Inserted
                                WHERE   Inserted.GameId = @gameId
                              );
        
        UPDATE  dbo.Boards
        SET     dbo.Boards.MoveNumber = @moveNumber + 1
        WHERE   dbo.Boards.GameId = @gameId;
    END;
GO

ALTER TABLE [dbo].[Boards] ADD
CONSTRAINT [UserIdWinner] FOREIGN KEY ([UserIdWinner]) REFERENCES [dbo].[UserProfile] ([UserId])
ALTER TABLE [dbo].[Boards] ADD
CONSTRAINT [UserIdBlack] FOREIGN KEY ([UserIdBlack]) REFERENCES [dbo].[UserProfile] ([UserId])
ALTER TABLE [dbo].[Boards] ADD
CONSTRAINT [UserIdWhite] FOREIGN KEY ([UserIdWhite]) REFERENCES [dbo].[UserProfile] ([UserId])
GO
ALTER TABLE [dbo].[Boards] ADD CONSTRAINT [PK_dbo.BoardDtoes] PRIMARY KEY CLUSTERED  ([GameId])
GO
