# Changelog

## v3.0.1
- Fixed threading issue for child contexts, where child could inherit from wrong parent.

## v3.0

- (breaking) It is no longer a requirement for logging or diagnostics to be enabled, Correlate now just works regardless. This now allows integrations to be less dependent on the log provider (`ILoggerFactory`/`ILogger`) and makes unit testing easier (`Microsoft.Extensions.Logging.Abstractions.NullLogger<T>` can now be used). Note however, that for production environments, `ILoggerFactory` is still required, in order for the `CorrelationId` property to be added to each log event.
- (breaking) Reworked `CorrelationManager`. It now has synchronous support for codebases that do not support asynchronous code. Also added overloads `Func<T>` and `Func<Task<T>>` allowing the return of values. New interfaces `ICorrelationManager` and `IAsyncCorrelationManager` are introduced for better separation, DI and unit testing. Reworked `OnException` delegate to be type safe and allowing a return value to be provided, if needed.
- Middleware no longer calls internal method in other assembly, but now uses the new and public `IAsyncCorrelationManager` making it more resilient to version discrepancies.

## v2.4

- Fixes starting nested context overwriting parent. Only create new context if correlation id differs. Keep track of nested contexts using stack, and restore the parent context when the nested (child) context completes.
- Added .NET Standard 1.3 and .NET 4.6 support
- Added ctors to `CorrelationManager` that accept `ICorrelationContextAccessor`. Old ctors are obsolete.

## v2.3

- Remove dependency constraints.
- Added ability to handle exception in correlation context scope.
- Middleware request header is now optional.
- Fix: potential async/await deadlock.

## v2.2

- Changed lifetime of manager to transient.

## v2.1

- DiagnosticListener is now an optional dependency.
- Ensure correlation context is disposed.
- Fix for logging a line without parameter.

## v2.0

- Fix namespaces
- General improvements

## v1.0

- Initial