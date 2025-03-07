# C3D.Extensions.Aspire.IISExpress

A way to reference and execute an IIS Express based project (ASP.NET 4.x) using Aspire

## C3D.Extensions.SystemWeb.OpenTelemetry.Application

An HttpApplication derived object that configures OTLP etc. automatically.
`Application_Start` method is virtual and can be overridden.
OTLP exporter will automatically start and stop with the application.
You can customize each type of telemetry, by adding to or completely overriding each of
- `ConfigureLogging`
- `ConfigureMetrics`
- `ConfigureTracing`

The following methods are also provided for ease of use
- `CreateServiceCollection` - Initialize the ServiceCollection
- `ConfigureServiceProvider` - This could be used to register your own objects in a DI container, or adjust things like logging.
- `UseServiceProvider` - This is called after the service provider is built, but before OTLP is started.

## Samples

### Simple
A basic project based on [MSBuild.SDK.SystemWeb](https://github.com/CZEMacLeod/MSBuild.SDK.SystemWeb)
which shows a very basic MVC5 application using the new SDK style, and can be launched by Aspire.
It supports OpenTelemetry and will correctly connect to the Aspire Dashboard.
Note that being IIS (Express) based, it does not start until the first web request, so you will need to launch it using the link on the dashboard.

### EF6
A more advanced project which uses Microsoft.Data.SqlClient to allow connections to any SQL Server using the latest technologies.
This has some code to allow connection strings to be copied from web.config to a Microsoft.Extensions.Configuration based version,
and creates a DependencyInjection container based on Microsoft.Extensions.DependencyInjection.
There are custom implementations of IServiceProvider for WebObjectActivator and IDependencyResolver for MVC5.
These allow you to inject ILogger and DbContext to a controller, filter etc.
This is a nieve implementation that does not deal with request scopes - all objects are either Singleton or Transient.
A localdb is created in the App_Data folder if you run the application directly and each time a new blog is created on startup.
If launched via aspire, it will wait for the sql server to start then start IISExpress. The database will be created afresh each time and there will only be a single blog entry.

## Debugging
Because Aspire doesn't know how to attach the debugger to IIS Express, two techniques can be used.
These can be set with 
```cs
	.WithDebugger(DebugMode.None)
```

- `None` means do not attach the debugger.
- `Environment` sets the environment variable `Launch_Debugger_On_Start` to `true`. 
This is checked for by a `PreApplicationStartMethod` which will attempt to launch a debugger.
- `VisualStudio` uses a hook and COM to get the running IDE and attach the debugger to IIS Express after it starts.

## Known Issues
- The `$(SolutionDir)\.vs\$(SolutionName)\config\applicationhost.config` file is not checked in as part of the source so you will probably have to manually select each web application and run it once manually to setup the appropriate information.
- There is no easy way to automatically start up the IIS Express based website (it might be possible to do via a healthcheck poll or similar but that is out of scope at this time.)
- It is not 'easy' to attach a debugger to IIS Express - the existing solutions are very basic.

