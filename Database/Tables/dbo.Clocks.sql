CREATE TABLE [dbo].[Clocks]
(
[ClockId] [int] NOT NULL IDENTITY(1, 1),
[GameId] [int] NOT NULL,
[LastActionBlack] [datetime] NOT NULL,
[LastActionWhite] [datetime] NOT NULL,
[TimeElapsedBlackMs] [int] NOT NULL,
[TimeElapsedWhiteMs] [int] NOT NULL,
[TimeLimitMs] [int] NOT NULL,
[PlayersReady] [int] NOT NULL
)
GO
ALTER TABLE [dbo].[Clocks] ADD CONSTRAINT [PK_dbo.Clocks] PRIMARY KEY CLUSTERED  ([ClockId])
GO
ALTER TABLE [dbo].[Clocks] ADD CONSTRAINT [FK_Cascade_clocks] FOREIGN KEY ([GameId]) REFERENCES [dbo].[BoardDtoes] ([GameId]) ON DELETE CASCADE
GO
