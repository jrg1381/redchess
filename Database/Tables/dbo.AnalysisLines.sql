CREATE TABLE [dbo].[AnalysisLines]
(
[AnalysisLineId] [int] NOT NULL IDENTITY(1, 1),
[AnalysisEntryId] [int] NOT NULL,
[GameId] [int] NOT NULL,
[Move] [nvarchar] (10) NOT NULL,
[Fen] [nvarchar] (max) NOT NULL,
[MoveNumber] [int] NOT NULL
)
CREATE NONCLUSTERED INDEX [Idx_GameId] ON [dbo].[AnalysisLines] ([GameId])

CREATE NONCLUSTERED INDEX [Idx_EntryId] ON [dbo].[AnalysisLines] ([AnalysisEntryId])

ALTER TABLE [dbo].[AnalysisLines] ADD 
PRIMARY KEY CLUSTERED  ([AnalysisLineId])
ALTER TABLE [dbo].[AnalysisLines] ADD
CONSTRAINT [FK_AnalysisEntry] FOREIGN KEY ([AnalysisEntryId]) REFERENCES [dbo].[Analysis] ([AnalysisEntryId])
GO

ALTER TABLE [dbo].[AnalysisLines] ADD CONSTRAINT [FK_CascadeAnalysisLines] FOREIGN KEY ([GameId]) REFERENCES [dbo].[Boards] ([GameId]) ON DELETE CASCADE
GO
