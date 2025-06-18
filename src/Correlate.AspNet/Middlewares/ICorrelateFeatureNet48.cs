using System.Web;

namespace Correlate.AspNet.Middlewares;

public interface ICorrelateFeatureNet48
{
    void StartCorrelating(HttpContextBase httpContext);

    void StopCorrelating(HttpContextBase httpContext);
}
