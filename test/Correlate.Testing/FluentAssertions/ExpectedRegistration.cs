using System;
using Microsoft.Extensions.DependencyInjection;

namespace Correlate.Testing.FluentAssertions
{
	public class ExpectedRegistration
	{
		public Type ServiceType { get; set; }
		public Type ImplementationType { get; set; }
		public ServiceLifetime Lifetime { get; set; }

		public override string ToString()
		{
			return $"{ServiceType.Name}, {ImplementationType?.Name ?? "<null>"}, {Lifetime}";
		}
	}

	public class ExpectedRegistration<TService> : ExpectedRegistration
	{
		public ExpectedRegistration(ServiceLifetime lifetime) 
		{
			ServiceType = typeof(TService);
			Lifetime = lifetime;
		}
	}

	public class ExpectedRegistration<TService, TImplementation> : ExpectedRegistration<TService>
	{
		public ExpectedRegistration(ServiceLifetime lifetime) : base(lifetime)
		{
			ImplementationType = typeof(TImplementation);
		}
	}
}