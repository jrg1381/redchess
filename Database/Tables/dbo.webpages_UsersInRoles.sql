CREATE TABLE [dbo].[webpages_UsersInRoles]
(
[UserId] [int] NOT NULL,
[RoleId] [int] NOT NULL
)
GO
ALTER TABLE [dbo].[webpages_UsersInRoles] ADD CONSTRAINT [PK__webpages__AF2760ADCB3C5423] PRIMARY KEY CLUSTERED  ([UserId], [RoleId])
GO
ALTER TABLE [dbo].[webpages_UsersInRoles] ADD CONSTRAINT [fk_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[webpages_Roles] ([RoleId])
GO
ALTER TABLE [dbo].[webpages_UsersInRoles] ADD CONSTRAINT [fk_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[UserProfile] ([UserId])
GO
