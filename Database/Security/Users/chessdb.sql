IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'chessdb')
CREATE LOGIN [chessdb] WITH PASSWORD = 'p@ssw0rd'
GO
CREATE USER [chessdb] FOR LOGIN [chessdb]
GO
