CREATE TABLE [dbo].[Administrators]
(
[AdministratorId] [int] NOT NULL
)
GO
ALTER TABLE [dbo].[Administrators] ADD PRIMARY KEY CLUSTERED  ([AdministratorId])
GO
ALTER TABLE [dbo].[Administrators] ADD FOREIGN KEY ([AdministratorId]) REFERENCES [dbo].[UserProfile] ([UserId]) ON DELETE CASCADE
GO
