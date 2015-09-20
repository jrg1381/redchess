CREATE TABLE [dbo].[Analysis]
(
[GameId] [int] NOT NULL,
[MoveNumber] [int] NOT NULL,
[Analysis] [nvarchar] (MAX) NULL,
[AnalysisEntryId] [int] NOT NULL IDENTITY(1, 1)
)
ALTER TABLE [dbo].[Analysis] ADD
CONSTRAINT [FK_CascadeAnalysis] FOREIGN KEY ([GameId]) REFERENCES [dbo].[Boards] ([GameId]) ON DELETE CASCADE
ALTER TABLE [dbo].[Analysis] ADD 
PRIMARY KEY CLUSTERED  ([AnalysisEntryId])
GO
