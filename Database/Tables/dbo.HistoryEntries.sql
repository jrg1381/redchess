CREATE TABLE [dbo].[HistoryEntries]
(
[HistoryId] [int] NOT NULL IDENTITY(1, 1),
[GameId] [int] NOT NULL,
[MoveNumber] [int] NOT NULL,
[Fen] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)
GO
ALTER TABLE [dbo].[HistoryEntries] ADD CONSTRAINT [PK_dbo.HistoryEntries] PRIMARY KEY CLUSTERED  ([HistoryId])
GO
CREATE NONCLUSTERED INDEX [FK_gameId] ON [dbo].[HistoryEntries] ([GameId])
GO
ALTER TABLE [dbo].[HistoryEntries] ADD CONSTRAINT [FK_Cascade] FOREIGN KEY ([GameId]) REFERENCES [dbo].[BoardDtoes] ([GameId]) ON DELETE CASCADE
GO
