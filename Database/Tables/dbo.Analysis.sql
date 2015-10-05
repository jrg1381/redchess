CREATE TABLE [dbo].[Analysis]
(
[GameId] [int] NOT NULL,
[MoveNumber] [int] NOT NULL,
[Analysis] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Evaluation] [int] NOT NULL,
[EvaluationType] [int] NOT NULL,
[AnalysisEntryId] [int] NOT NULL IDENTITY(1, 1)
)
ALTER TABLE [dbo].[Analysis] ADD 
PRIMARY KEY CLUSTERED  ([AnalysisEntryId])

ALTER TABLE [dbo].[Analysis] ADD
CONSTRAINT [FK_CascadeAnalysis] FOREIGN KEY ([GameId]) REFERENCES [dbo].[Boards] ([GameId]) ON DELETE CASCADE

GO
