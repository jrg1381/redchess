CREATE TABLE [dbo].[webpages_Membership]
(
[UserId] [int] NOT NULL,
[CreateDate] [datetime] NULL,
[ConfirmationToken] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[IsConfirmed] [bit] NULL CONSTRAINT [DF__webpages___IsCon__38996AB5] DEFAULT ((0)),
[LastPasswordFailureDate] [datetime] NULL,
[PasswordFailuresSinceLastSuccess] [int] NOT NULL CONSTRAINT [DF__webpages___Passw__398D8EEE] DEFAULT ((0)),
[Password] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[PasswordChangedDate] [datetime] NULL,
[PasswordSalt] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[PasswordVerificationToken] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[PasswordVerificationTokenExpirationDate] [datetime] NULL
)
GO
ALTER TABLE [dbo].[webpages_Membership] ADD CONSTRAINT [PK__webpages__1788CC4C3FB3F7CD] PRIMARY KEY CLUSTERED  ([UserId])
GO
