CREATE TABLE [dbo].[webpages_Roles]
(
[RoleId] [int] NOT NULL IDENTITY(1, 1),
[RoleName] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
)
GO
ALTER TABLE [dbo].[webpages_Roles] ADD CONSTRAINT [PK__webpages__8AFACE1A41E55927] PRIMARY KEY CLUSTERED  ([RoleId])
GO
ALTER TABLE [dbo].[webpages_Roles] ADD CONSTRAINT [UQ__webpages__8A2B6160D3D16F4E] UNIQUE NONCLUSTERED  ([RoleName])
GO
