using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Correlate.Http;
using Microsoft.Extensions.Options;

namespace Correlate.WebApiTestNet48.ActionFilters;

public class CorrelationIdActionFilter(ICorrelationContextAccessor correlationContextAccessor, IOptions<CorrelateClientOptions> options, IActivityFactory activityFactory)
    : ActionFilterAttribute
{
    private readonly CorrelateClientOptions _options = options.Value;
    private IActivity? _activity;
    
    public override void OnActionExecuting(HttpActionContext actionContext)
    {
        if (correlationContextAccessor.CorrelationContext is null)
        {
            _activity = activityFactory.CreateActivity();
            correlationContextAccessor.CorrelationContext = activityFactory.StartActivity(null, _activity);
        }

        if (actionContext.Request.Headers.Contains(_options.RequestHeader))
        {
            correlationContextAccessor.CorrelationContext.CorrelationId = actionContext.Request.Headers.GetValues(_options.RequestHeader).FirstOrDefault();
        }
        
        base.OnActionExecuting(actionContext);
    }

    public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
    {
        if (correlationContextAccessor.CorrelationContext is not null)
        {
            actionExecutedContext.Response.Headers.TryAddWithoutValidation(_options.RequestHeader, correlationContextAccessor.CorrelationContext.CorrelationId);
            _activity?.Stop();
            correlationContextAccessor.CorrelationContext = null;
        }

        base.OnActionExecuted(actionExecutedContext);
    }
}
