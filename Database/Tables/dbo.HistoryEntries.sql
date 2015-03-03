CREATE TABLE [dbo].[HistoryEntries]
(
[HistoryId] [int] NOT NULL IDENTITY(1, 1),
[GameId] [int] NOT NULL,
[MoveNumber] [int] NOT NULL,
[Fen] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Move] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
)
CREATE NONCLUSTERED INDEX [FK_GameId] ON [dbo].[HistoryEntries] ([GameId])

ALTER TABLE [dbo].[HistoryEntries] ADD 
CONSTRAINT [PK_dbo.HistoryEntries] PRIMARY KEY CLUSTERED  ([HistoryId])
ALTER TABLE [dbo].[HistoryEntries] ADD
CONSTRAINT [FK_Cascade] FOREIGN KEY ([GameId]) REFERENCES [dbo].[Boards] ([GameId]) ON DELETE CASCADE
GO
