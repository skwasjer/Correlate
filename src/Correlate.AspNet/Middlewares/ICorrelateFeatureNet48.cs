using System.Web;

namespace Correlate.AspNet.Middlewares;

public interface ICorrelateFeatureNet48
{
    void StartCorrelating(HttpContext httpContext);

    void StopCorrelating(HttpContext httpContext);
}
