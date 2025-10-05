using Correlate.Http.Server;

namespace Correlate.AspNetCore;

internal interface ICorrelateFeature
{
    void StartCorrelating(IHttpListenerContext context);

    void StopCorrelating(IHttpListenerContext context);
}
