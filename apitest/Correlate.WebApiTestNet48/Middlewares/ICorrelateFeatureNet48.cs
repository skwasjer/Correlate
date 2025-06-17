using System.Web;

namespace Correlate.WebApiTestNet48.Middlewares;

public interface ICorrelateFeatureNet48
{
    void StartCorrelating(HttpContext httpContext);

    void StopCorrelating(HttpContext httpContext);
}
