CREATE TABLE [dbo].[BoardDtoes]
(
[GameId] [int] NOT NULL IDENTITY(1, 1),
[UserIdWhite] [int] NOT NULL,
[UserIdBlack] [int] NOT NULL,
[Status] [nvarchar] (max) NULL,
[GameOver] [bit] NOT NULL,
[Fen] [nvarchar] (max) NULL,
[MayClaimDraw] AS ([dbo].[MayClaimDraw]([GameId]))
)
GO
ALTER TABLE [dbo].[BoardDtoes] ADD CONSTRAINT [PK_dbo.BoardDtoes] PRIMARY KEY CLUSTERED  ([GameId])
GO
