# C3D.Extensions.SystemWeb.OpenTelemetry.Application

An HttpApplication derived object that configures OTLP etc. automatically.
`Application_Start` method is virtual and can be overridden.
OTLP exporter will automatically start and stop with the application.
You can customize each type of telemetry, by adding to or completely overriding each of
- `ConfigureResource` - Set the service name, version, and other attributes. Common to all types of telemetry.
- `ConfigureLogging`
- `ConfigureMetrics`
- `ConfigureTracing`

The following methods are also provided for ease of use
- `CreateServiceCollection` - Initialize the ServiceCollection
- `ConfigureServiceProvider` - This could be used to register your own objects in a DI container, or adjust things like logging.
- `UseServiceProvider` - This is called after the service provider is built, but before OTLP is started.