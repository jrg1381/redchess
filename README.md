redchess
========

## Building

* Requires Visual Studio 2015. 
* Building a new version of the parser requires a Java runtime to run Antlr, the parser generator. 
* The web service deployment script requires the Azure Powershell cmdlets.

## Running

The website can be published to Azure or run under IIS or IIS Express. It needs a SQL Server database, by default an instance called `.\2014`, but the connection string is editable in `web.config`.

If running in IIS, the website user must be given execute permissions on the stored procedures in the database, in addition to membership of the `db_datareader / db_datawriter` roles.

## Restoring the database

The SQL files are stored under

https://github.com/jrg1381/redchess/tree/master/Database 

and can be restored using Redgate's SQL Compare tool (commercial) or executed manually. Run order is probably important :smile:
