using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Correlate
{
	public class CorrelationContextAccessorTests
	{
		private readonly CorrelationContextAccessor _sut;
		private readonly Random _rnd;

		public CorrelationContextAccessorTests()
		{
			_sut = new CorrelationContextAccessor();
			_rnd = new Random();
		}

		[Fact]
		public void Given_context_is_uninitialized_when_setting_context_to_null_it_should_not_throw()
		{
			Action act = () => _sut.CorrelationContext = null;

			// Assert
			act.Should().NotThrow();
		}

		[Fact]
		public async Task When_running_multiple_tasks_in_nested_context_should_inherit_from_ambient_parent()
		{
			const string rootName = "root-context";
			var expectedNestedCorrelationIds = new List<string>
			{
				$"{rootName} > 1:0 | 1:0 > 1:1 | 1:1 > 1:2 | 1:2 > 1:3 | ",
				$"{rootName} > 2:0 | 2:0 > 2:1 | 2:1 > 2:2 | 2:2 > 2:3 | ",
				$"{rootName} > 3:0 | 3:0 > 3:1 | 3:1 > 3:2 | 3:2 > 3:3 | "
			};

			var rootContext = new CorrelationContext
			{
				CorrelationId = rootName
			};
			_sut.CorrelationContext = rootContext;

			// Act
			IEnumerable<Task<string>> tasks = Enumerable.Range(1, 3)
				.Select(i => RunChildTask(i.ToString()));

			string[] actual = await Task.WhenAll(tasks);
			_sut.CorrelationContext = null;

			// Assert
			actual.Should().BeEquivalentTo(expectedNestedCorrelationIds, "each task that creates new context should inherit from parent, and all inner tasks from their respective parent");
			_sut.CorrelationContext.Should().BeNull();
		}

		private async Task<string> RunChildTask(string id, int level = 0)
		{
			const int recursiveRuns = 3;

			// Set new context.
			_sut.CorrelationContext = new CorrelationContext
			{
				CorrelationId = $"{id}:{level}"
			};
			string correlationId = _sut.CorrelationContext.CorrelationId;

			// Simulate random work.
			await Task.Run(() => Task.Delay(_rnd.Next(100)));

			// Do nested run.
			string childId = null;
			if (level < recursiveRuns)
			{
				childId = await RunChildTask(id, level + 1);
			}

			// Even though we set to null, the net result should be restoring the parent context.
			_sut.CorrelationContext = null;

			return $"{_sut.CorrelationContext?.CorrelationId} > {correlationId} | {childId}";
		}
	}
}
