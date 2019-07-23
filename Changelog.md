# Changelog

## v3.0 - WIP

- (breaking) Reworked `CorrelationManager`. It now has synchronous support, and has overloads for `Func<T>` and `Func<Task<T>>` allowing the return of values. New interfaces `ICorrelationManager` and `IAsyncCorrelationManager` are introduced for better separation, DI and unit testing. Reworked `OnException` delegate to be type safe and allowing a return value to be provided, if needed.
- Middleware no longer calls internal method, but now uses the new and public `IAsyncCorrelationManager` making it more resilient to version discrepancies.

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