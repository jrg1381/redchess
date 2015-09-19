CREATE TABLE [dbo].[Analysis]
(
[GameId] [int] NOT NULL,
[MoveNumber] [int] NOT NULL,
[Analysis] [nvarchar] (50) NULL,
[AnalysisEntryId] [int] NOT NULL IDENTITY(1, 1)
)
ALTER TABLE [dbo].[Analysis] ADD 
PRIMARY KEY CLUSTERED  ([AnalysisEntryId])
GO
