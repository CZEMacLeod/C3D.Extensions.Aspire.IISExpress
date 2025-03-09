# C3D.Extensions.Aspire.VisualStudioDebug

A way to get VisualStudio to attach the debugger for the AspireHost to a running executable project (such as an `IISExpressProjectResource`).
Uses a BackgroundService to montior for the Process Id (via `executable.pid`) and `C3D.Extensions.VisualStudioDebug` to manipulate VisualStudio.
Appropriate debug engines can be enabled, such as `Managed (.NET Framework 4.x)` to allow debugging of IIS Express based ASP.Net 4.x Projects.
This mechanism could be extended to attach the debugger to apps in docker containers, or other languages such as nodejs.

## Eventing
The `C3D.Extensions.Aspire.VisualStudioDebug` package has eventing enabled that fires
`BeforeDebugEvent` and `AfterDebugEvent`. 
This could be listened for, and used to make an http call to any IIS Express based application to start them up after the debugger has attached,
or to launch a browser to the endpoint etc.

## Health Checks
A Debugger Attached health check is available which can be added to a resource such that it does not go healthy until the debugger is attached.
