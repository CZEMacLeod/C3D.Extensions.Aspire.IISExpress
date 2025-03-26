using C3D.Extensions.Aspire.OutputWatcher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

internal partial class Program
{
    private static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        var sql = builder.AddSqlServer("sql");
        var sqldb = sql.AddDatabase("sqldb");

        // Example of waiting for a console app to output a specific message
        // This could be an Executable of some sort - such as Node, Python, etc.
        // We wait for some pre-requisite to be ready, then we wait for the console app to output a specific message
        var console = builder.AddProject<Projects.WaitForConsole_ConsoleApp>("consoleapp")
            .WithReference(sqldb, "sqldb")
            .WaitFor(sqldb)
            .WithOutputWatcher(GetMagic(), true, "magic");

        // The GetMagic Regex will capture the magic number from the console app output and store it as a property
        // The output string will be of the format "{number} is the magic number!"
        // Once it is detected, we can store it in a ReferenceExpression to be used as an environment variable in the next project
        //ReferenceExpression? magicNumber = null;

        var magicNumber = console.GetReferenceExpression("magic");

        var magicNumberSubscription = builder.Eventing.Subscribe<OutputMatchedEvent>(console.Resource, (o, c) =>
        {
            if (o.Key == "magic")
            {
                // Get the magic number from the console app
                // The regex match group capture is 'magic' and will be stored as a property.
                var number = int.Parse(o.Properties["magic"].ToString()!);
                
                o.ServiceProvider.GetRequiredService<ILogger<Program>>().LogInformation($"{number} is the magic number!");
                //var reb = new ReferenceExpressionBuilder();
                //reb.AppendFormatted($"{number}");
                //magicNumber = reb.Build();
            }

            return Task.CompletedTask;
        });

        // webapp won't start until console has output the message "Ready Now..."
        // Note that 'console' does not have to exit, it just has to output the message
        builder.AddProject<Projects.WaitForConsole_WebApp>("webapp")
            .WithReference(sqldb, "sqldb")
            .WaitFor(sqldb)
            .WaitForOutput(console, m => m == "Ready Now...")
            .WithEnvironment("MAGIC_NUMBER", magicNumber);

        builder.Build().Run();
    }

    [GeneratedRegex("^(?<magic>.*\\d)(?:( is the magic number!))$")]
    public static partial Regex GetMagic();
}

