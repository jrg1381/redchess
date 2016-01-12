CREATE TABLE [dbo].[Metadata]
(
[Key] [nvarchar] (25) COLLATE Latin1_General_CI_AS NOT NULL,
[Value] [nvarchar] (128) COLLATE Latin1_General_CI_AS NOT NULL
)
ALTER TABLE [dbo].[Metadata] ADD 
PRIMARY KEY CLUSTERED  ([Key])
GO
