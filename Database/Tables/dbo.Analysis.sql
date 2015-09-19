CREATE TABLE [dbo].[Analysis]
(
[GameId] [int] NOT NULL,
[MoveNumber] [int] NOT NULL,
[Analysis] [nvarchar] (50) NULL
)
GO
ALTER TABLE [dbo].[Analysis] ADD PRIMARY KEY CLUSTERED  ([GameId])
GO
