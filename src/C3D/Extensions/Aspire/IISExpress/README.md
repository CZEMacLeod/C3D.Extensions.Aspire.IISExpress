# C3D.Extensions.Aspire.IISExpress

A way to reference and execute an IIS Express based project (ASP.NET 4.x) using Aspire.
Connects to the instance of VisualStudio running the AprireHost and attaches the debugger to the IIS Express instance so that the project can be debugged as normal.
Adds a healthcheck to the IIS Express resource to show whether the debugger has been attached.
A future option would be to send the initial request to spin up the site once the debugger is attached.

## Known Issues
- The `$(SolutionDir)\.vs\$(SolutionName)\config\applicationhost.config` file is not checked in as part of the source so you will probably have to manually select each web application and run it once manually to setup the appropriate information.
- There is no 'easy' way to automatically start up the IIS Express based website (it might be possible to do via a healthcheck poll or similar but that is out of scope at this time.)