CREATE TABLE [dbo].[UserProfile]
(
[UserId] [int] NOT NULL IDENTITY(1, 1),
[UserName] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
)
GO
ALTER TABLE [dbo].[UserProfile] ADD CONSTRAINT [PK_dbo.UserProfile] PRIMARY KEY CLUSTERED  ([UserId])
GO
