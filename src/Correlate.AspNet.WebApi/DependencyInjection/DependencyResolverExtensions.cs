using System.Diagnostics.CodeAnalysis;
using System.Web.Http.Dependencies;
using Correlate.Http.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Correlate.DependencyInjection;

internal static class DependencyResolverExtensions
{
    internal static DefaultHttpListener GetHttpListener(this IDependencyResolver resolver)
    {
        // Check for default resolver used by Web API, which means this resolver will not provide necessary services.
        if (resolver.GetType().Name == "EmptyResolver")
        {
            Throw();
        }

        try
        {
            return ActivatorUtilities.CreateInstance<DefaultHttpListener>(new ServiceProviderAdapter(resolver));
        }
        catch (InvalidOperationException ex)
        {
            Throw(ex);
            return null;
        }

        [DoesNotReturn]
        static void Throw(Exception? exception = null)
        {
            throw new InvalidOperationException("Correlate was unable to resolve its dependencies. Please configure IDependencyResolver to use Microsoft.Extensions.DependencyInjection or similar.", exception);
        }
    }

    private sealed class ServiceProviderAdapter(IDependencyResolver dependencyResolver) : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            return dependencyResolver.GetService(serviceType);
        }
    }
}
