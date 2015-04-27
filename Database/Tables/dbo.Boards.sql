CREATE TABLE [dbo].[Boards]
(
[GameId] [int] NOT NULL IDENTITY(1, 1),
[UserIdWhite] [int] NOT NULL,
[UserIdBlack] [int] NOT NULL,
[Status] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[GameOver] [bit] NOT NULL,
[Fen] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[CreationDate] [datetime] NOT NULL CONSTRAINT [DF__RG_Recove__Creat__73BA3083] DEFAULT (getutcdate()),
[CompletionDate] [datetime] NOT NULL CONSTRAINT [DF_BoardDtoes_CompletionDate] DEFAULT (getutcdate()),
[MayClaimDraw] AS (case when [GameOver]=(1) then CONVERT([bit],(0),(0)) else [dbo].[MayClaimDraw]([GameId]) end),
[LastMove] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[UserIdWinner] [int] NULL
)
ALTER TABLE [dbo].[Boards] ADD
CONSTRAINT [UserIdWinner] FOREIGN KEY ([UserIdWinner]) REFERENCES [dbo].[UserProfile] ([UserId])
ALTER TABLE [dbo].[Boards] ADD
CONSTRAINT [UserIdBlack] FOREIGN KEY ([UserIdBlack]) REFERENCES [dbo].[UserProfile] ([UserId])
ALTER TABLE [dbo].[Boards] ADD
CONSTRAINT [UserIdWhite] FOREIGN KEY ([UserIdWhite]) REFERENCES [dbo].[UserProfile] ([UserId])
GO
ALTER TABLE [dbo].[Boards] ADD CONSTRAINT [PK_dbo.BoardDtoes] PRIMARY KEY CLUSTERED  ([GameId])
GO
