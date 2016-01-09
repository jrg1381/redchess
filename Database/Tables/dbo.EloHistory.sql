CREATE TABLE [dbo].[EloHistory]
(
[EloHistoryId] [int] NOT NULL IDENTITY(1, 1),
[Date] [datetime] NOT NULL,
[UserId] [int] NOT NULL,
[Elo] [int] NOT NULL
)
GO
ALTER TABLE [dbo].[EloHistory] ADD PRIMARY KEY CLUSTERED  ([EloHistoryId])
GO
CREATE NONCLUSTERED INDEX [Idx_UserId] ON [dbo].[EloHistory] ([UserId])
GO
