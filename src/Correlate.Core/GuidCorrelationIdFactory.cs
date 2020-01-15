using System;
using System.Globalization;

namespace Correlate
{
	/// <summary>
	/// Produces a correlation id by generating a new guid.
	/// </summary>
	public class GuidCorrelationIdFactory : ICorrelationIdFactory
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GuidCorrelationIdFactory"/> class.
		/// </summary>
		// ReSharper disable once EmptyConstructor
		public GuidCorrelationIdFactory()
		{
		}

		/// <inheritdoc />
		public string Create()
		{
#if NETSTANDARD1_3
			return Guid.NewGuid().ToString("D");
#else
			return Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture);
#endif
		}
	}
}
