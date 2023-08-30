# Changelog

## Unreleased

- Added .NET 7 target framework
- Refactored ASP.NET Core integration to be based on `DiagnosticsListener`, by subscribing an observer and watch for HttpRequestIn (activity) events. This gives us the soonest opportunity to create the correlation context. Previously, `.UseCorrelate()` would register middleware. There were a few downsides with that approach, all having to do with _when_ the correlation context actually was being created:
  - it was depending on the registration order when considering other middleware. F.ex. if `UseCorrelate()` was not called before any other middleware was registered, the correlation context was also not created as soon as possible.
  - the ASP.NET Core its own built-in request-start and request-stop log messages did not have a Correlation ID property attached because the log scope created by Correlate is created after/disposed before these are emitted.
  - for unhandled exceptions, the correlation context was already gone in the global exception handler because the request middleware pipeline completes before it gets there (unless you registered a custom one!). Again, this means those (default) log messages would not have a correlation ID property (see previous point).
  - the middleware had a little too many allocations to my liking (considering that it runs in a fast-path).  

## v4.0.0

- Added .NET 6 target framework
- (breaking) Dropped .NET 5.0 support (non-LTS)
- (breaking) Removed .NET Framework 4.6 and .NET Standard below 2.0.
- (breaking) Removed `CorrelationManager` obsoleted constructors.
- (breaking) Removed obsolete `CorrelateRequests` overload accepting an `IConfiguration` parameter.

## v3.3.0

- Added .NET 5 target framework
- Deprecated `CorrelateRequests` extension accepting `IConfiguration` instance.

## v3.2.0

- Added .NET Standard 2.1 support (for .NET Core 3.x, with newer Microsoft.* dependencies)
- Updated API contracts with non-nullable ref types
- Fix several potential NRE's in extension methods that were not guarded with ANE's.

## v3.1.0
- Fixed threading issue for child contexts, where child could inherit from wrong parent.

## v3.0.0

- (breaking) It is no longer a requirement for logging or diagnostics to be enabled, Correlate now just works regardless. This now allows integrations to be less dependent on the log provider (`ILoggerFactory`/`ILogger`) and makes unit testing easier (`Microsoft.Extensions.Logging.Abstractions.NullLogger<T>` can now be used). Note however, that for production environments, `ILoggerFactory` is still required, in order for the `CorrelationId` property to be added to each log event.
- (breaking) Reworked `CorrelationManager`. It now has synchronous support for codebases that do not support asynchronous code. Also added overloads `Func<T>` and `Func<Task<T>>` allowing the return of values. New interfaces `ICorrelationManager` and `IAsyncCorrelationManager` are introduced for better separation, DI and unit testing. Reworked `OnException` delegate to be type safe and allowing a return value to be provided, if needed.
- Middleware no longer calls internal method in other assembly, but now uses the new and public `IAsyncCorrelationManager` making it more resilient to version discrepancies.

## v2.4.0

- Fixes starting nested context overwriting parent. Only create new context if correlation id differs. Keep track of nested contexts using stack, and restore the parent context when the nested (child) context completes.
- Added .NET Standard 1.3 and .NET 4.6 support
- Added ctors to `CorrelationManager` that accept `ICorrelationContextAccessor`. Old ctors are obsolete.

## v2.3.0

- Remove dependency constraints.
- Added ability to handle exception in correlation context scope.
- Middleware request header is now optional.
- Fix: potential async/await deadlock.

## v2.2.0

- Changed lifetime of manager to transient.

## v2.1.0

- DiagnosticListener is now an optional dependency.
- Ensure correlation context is disposed.
- Fix for logging a line without parameter.

## v2.0.0

- Fix namespaces
- General improvements

## v1.0.0

- Initial
