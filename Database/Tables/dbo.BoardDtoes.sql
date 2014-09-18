CREATE TABLE [dbo].[BoardDtoes]
(
[GameId] [int] NOT NULL IDENTITY(1, 1),
[UserIdWhite] [int] NOT NULL,
[UserIdBlack] [int] NOT NULL,
[Status] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[GameOver] [bit] NOT NULL,
[Fen] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[MayClaimDraw] AS (case when [GameOver]=(1) then CONVERT([bit],(0),0) else [dbo].[MayClaimDraw]([GameId]) end)
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[BoardDtoes] ADD CONSTRAINT [PK_dbo.BoardDtoes] PRIMARY KEY CLUSTERED  ([GameId])
GO
