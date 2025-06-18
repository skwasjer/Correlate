using System.Web;
using Correlate.AspNet;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: PreApplicationStartMethod(typeof(PreApplicationStartCode), nameof(PreApplicationStartCode.Start))]
namespace Correlate.AspNet;

public static class PreApplicationStartCode
{
    private static bool _startWasCalled;

    public static void Start()
    {
        if (_startWasCalled)
        {
            return;
        }

        _startWasCalled = true;
        DynamicModuleUtility.RegisterModule(typeof(CorrelateHttpModule));
    }
}
