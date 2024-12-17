### Migration from v5.x to 6.0.0

- `CorrelateOptions.RequestHeaders` no longer has a default value of `X-Correlation-ID`. However, if this property is set to `null` (or not configured explicitly at all), internally the behavior remains unchanged and `X-Correlation-ID` is assumed. See [#128](https://github.com/skwasjer/Correlate/issues/128).

### Migration from v4.x to 5.0.0

- Replace `Correlate.AspNetCore.Middleware` namespace with `Correlate.AspNetCore`.
