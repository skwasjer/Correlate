using System.Web;

namespace Correlate.AspNet.Middlewares;

internal interface ICorrelateFeatureNet48
{
    void StartCorrelating(HttpContextBase httpContext);

    void StopCorrelating(HttpContextBase httpContext);
}
