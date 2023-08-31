using Microsoft.AspNetCore.Http;

namespace Correlate.AspNetCore;

internal interface ICorrelateFeature
{
    void StartCorrelating(HttpContext httpContext);

    void StopCorrelating(HttpContext httpContext);
}
