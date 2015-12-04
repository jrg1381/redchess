CREATE TABLE [dbo].[HistoryEntries]
(
[HistoryId] [int] NOT NULL IDENTITY(1, 1),
[GameId] [int] NOT NULL,
[MoveNumber] [int] NOT NULL,
[Fen] [nvarchar] (max) NOT NULL,
[Move] [nvarchar] (10) NOT NULL
)
CREATE NONCLUSTERED INDEX [gamemove] ON [dbo].[HistoryEntries] ([GameId], [MoveNumber])

CREATE NONCLUSTERED INDEX [FK_GameId] ON [dbo].[HistoryEntries] ([GameId])

ALTER TABLE [dbo].[HistoryEntries] ADD 
CONSTRAINT [PK_dbo.HistoryEntries] PRIMARY KEY CLUSTERED  ([HistoryId])
ALTER TABLE [dbo].[HistoryEntries] ADD
CONSTRAINT [FK_Cascade] FOREIGN KEY ([GameId]) REFERENCES [dbo].[Boards] ([GameId]) ON DELETE CASCADE
GO
