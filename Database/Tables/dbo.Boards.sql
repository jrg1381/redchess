CREATE TABLE [dbo].[Boards]
(
[GameId] [int] NOT NULL IDENTITY(1, 1),
[UserIdWhite] [int] NOT NULL,
[UserIdBlack] [int] NOT NULL,
[Status] [nvarchar] (max) NULL,
[GameOver] [bit] NOT NULL,
[Fen] [nvarchar] (max) NOT NULL,
[CreationDate] [datetime] NOT NULL DEFAULT (getutcdate()),
[CompletionDate] [datetime] NOT NULL DEFAULT (getutcdate()),
[MayClaimDraw] AS (case  when [GameOver]=(1) then CONVERT([bit],(0),(0)) else [dbo].[MayClaimDraw]([GameId]) end),
[LastMove] [nvarchar] (10) NULL,
[UserIdWinner] [int] NULL,
[MoveNumber] [int] NOT NULL DEFAULT ((0)),
[ClockId] [int] NULL
)
ALTER TABLE [dbo].[Boards] ADD
CONSTRAINT [ClockId] FOREIGN KEY ([ClockId]) REFERENCES [dbo].[Clocks] ([ClockId])
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
