# C3D.Extensions.Aspire.OutputWatcher

A service that can be injected to monitor the output of a resource for a specific string, regex, or other predicate.
Upon detection, the eventing system will raise the `OutputMatchedEvent` event and the `OutputWatcherAnnotation` will store the last match.

This can be used as the basis for other extensions, such as NodeJS debugging by watching for the `Debugger listening on ws://` message, or a healthcheck waiting for a specific string from a console application that perhaps does not terminate.