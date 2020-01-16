using System.Collections.Generic;
using System.Linq;

namespace Correlate.Testing.TestCases
{
	public class NullArgumentTestCases : List<DelegateTestCase>
	{
		public IEnumerable<object[]> Flatten()
			=> this
				.SelectMany(tc => tc.GetNullArgumentTestCases())
				.ToList();
	}
}
