# C3D.Extensions.Aspire.WaitForOutput

This is a simple utility that waits for a specific string to appear in the console output of a process before
starting the next process when using Aspire. 

It is useful for integration tests where you want to wait for a specific string to appear in the console output of a process before proceeding.

## Usage
```csharp
var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql");
var sqldb = sql.AddDatabase("sqldb");

// Example of waiting for a console app to output a specific message
// This could be an Executable of some sort - such as Node, Python, etc.
// We wait for some pre-requisite to be ready, then we wait for the console app to output a specific message
var console = builder.AddProject<Projects.WaitForConsole_ConsoleApp>("consoleapp")
    .WithReference(sqldb, "sqldb")
    .WaitFor(sqldb);

// webapp won't start until console has output the message "Ready Now..."
// Note that 'console' does not have to exit, it just has to output the message
builder.AddProject<Projects.WaitForConsole_WebApp>("webapp")
    .WithReference(sqldb, "sqldb")
    .WaitFor(sqldb)
    .WaitForOutput(console, m => m == "Ready Now...");

builder.Build().Run();
```
