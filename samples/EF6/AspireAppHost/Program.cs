using Aspire.Hosting;
using C3D.Extensions.Aspire.IISExpress;
using C3D.Extensions.Aspire.IISExpress.Resources;

var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions()
{
    Args = args,
    AllowUnsecuredTransport = true
});

var sql = builder.AddSqlServer("sql")
    .WithHealthCheck();
var sqldb = sql.AddDatabase("sqldb");

builder.AddIISExpressConfiguration(ThisAssembly.Project.SolutionName, ThisAssembly.Project.SolutionDir);

builder.AddIISExpressProject<Projects.EF6WebApp>()
    .WithReference(sqldb, "BloggingContext")
    .WaitFor(sqldb)
    .WithDebugger(DebugMode.None)
    ;

builder.Build().Run();
