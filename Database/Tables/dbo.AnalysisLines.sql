CREATE TABLE [dbo].[AnalysisLines]
(
[AnalysisLineId] [int] NOT NULL IDENTITY(1, 1),
[GameId] [int] NOT NULL
)
GO
ALTER TABLE [dbo].[AnalysisLines] ADD PRIMARY KEY CLUSTERED  ([AnalysisLineId])
GO
ALTER TABLE [dbo].[AnalysisLines] ADD CONSTRAINT [FK_CascadeAnalysisLines] FOREIGN KEY ([GameId]) REFERENCES [dbo].[Boards] ([GameId]) ON DELETE CASCADE
GO
