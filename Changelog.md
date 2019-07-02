# Changelog

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