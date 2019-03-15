using System.Collections.Generic;
using System.Net.Http.Headers;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace Correlate.FluentAssertions
{
	public class ResponseHeadersAssertions : ReferenceTypeAssertions<HttpResponseHeaders, ResponseHeadersAssertions>
	{
		public ResponseHeadersAssertions(HttpResponseHeaders actualValue)
		{
			Subject = actualValue;
		}

		protected override string Identifier => "ResponseHeaders";

		public WhichValueConstraint<ResponseHeadersAssertions, IEnumerable<string>> ContainCorrelationId(string headerName = CorrelationHttpHeaders.CorrelationId, string because = "", params object[] becauseArgs)
		{
			Execute.Assertion
				.BecauseOf(because, becauseArgs)
				.ForCondition(!string.IsNullOrWhiteSpace(headerName))
				.FailWith("Can't assert when no header name is provided.")
				.Then
				.WithExpectation("Expected {context:headers} to contain {0}{reason}, ", headerName)
				.Given(() =>
				{
					Subject.TryGetValues(headerName, out IEnumerable<string> values);
					return values;
				})
				.ForCondition(values => values != null)
				.FailWith("but found no matching header.")
				;

			Subject.TryGetValues(headerName, out IEnumerable<string> obj);
			return new WhichValueConstraint<ResponseHeadersAssertions, IEnumerable<string>>(this, obj);
		}
	}
}