CREATE TABLE [dbo].[Boards]
(
[GameId] [int] NOT NULL IDENTITY(1, 1),
[UserIdWhite] [int] NOT NULL,
[UserIdBlack] [int] NOT NULL,
[Status] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[GameOver] [bit] NOT NULL,
[Fen] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[CreationDate] [datetime] NOT NULL CONSTRAINT [DF__RG_Recove__Creat__73BA3083] DEFAULT (getutcdate()),
[CompletionDate] [datetime] NOT NULL CONSTRAINT [DF_BoardDtoes_CompletionDate] DEFAULT (getutcdate()),
[MayClaimDraw] AS (case when [GameOver]=(1) then CONVERT([bit],(0),(0)) else [dbo].[MayClaimDraw]([GameId]) end),
[LastMove] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)
GO
ALTER TABLE [dbo].[Boards] ADD CONSTRAINT [PK_dbo.BoardDtoes] PRIMARY KEY CLUSTERED  ([GameId])
GO
