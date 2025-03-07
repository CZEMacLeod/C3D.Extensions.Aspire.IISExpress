var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions()
{
    Args = args,
    AllowUnsecuredTransport = true
});

var sql = builder.AddSqlServer("sql");
var sqldb = sql.AddDatabase("sqldb");

builder.AddIISExpressProject<Projects.EF6WebApp>()
    .WithReference(sqldb, "BloggingContext")
    .WaitFor(sql)
    ;

builder.Build().Run();
