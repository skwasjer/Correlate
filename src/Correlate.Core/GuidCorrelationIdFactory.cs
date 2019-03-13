using System;
using Correlate.Abstractions;

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
			return Guid.NewGuid().ToString("D");
		}
	}
}
