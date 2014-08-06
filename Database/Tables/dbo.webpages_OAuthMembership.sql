CREATE TABLE [dbo].[webpages_OAuthMembership]
(
[Provider] [nvarchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[ProviderUserId] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[UserId] [int] NOT NULL
)
GO
ALTER TABLE [dbo].[webpages_OAuthMembership] ADD CONSTRAINT [PK__webpages__F53FC0EDE9BC1988] PRIMARY KEY CLUSTERED  ([Provider], [ProviderUserId])
GO
