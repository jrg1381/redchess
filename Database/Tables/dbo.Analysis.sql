CREATE TABLE [dbo].[Analysis]
(
[GameId] [int] NOT NULL,
[MoveNumber] [int] NOT NULL,
[Evaluation] [int] NOT NULL,
[EvaluationType] [int] NOT NULL,
[AnalysisEntryId] [int] NOT NULL IDENTITY(1, 1),
[AnalysisLineId] [int] NULL
)
ALTER TABLE [dbo].[Analysis] ADD 
PRIMARY KEY CLUSTERED  ([AnalysisEntryId])

ALTER TABLE [dbo].[Analysis] ADD
CONSTRAINT [FK_CascadeAnalysis] FOREIGN KEY ([GameId]) REFERENCES [dbo].[Boards] ([GameId]) ON DELETE CASCADE

GO
