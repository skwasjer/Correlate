using System;
using System.Collections;
using System.Threading.Tasks;
using Correlate.Testing;
using FluentAssertions;
using Moq;
using Xunit;

namespace Correlate
{
	public class CorrelationManagerTests
	{
		private readonly CorrelationContextAccessor _correlationContextAccessor;
		private readonly CorrelationManager _sut;
		private readonly Mock<ICorrelationIdFactory> _correlationIdFactoryMock;
		private const string GeneratedCorrelationId = "generated-correlation-id";

		public CorrelationManagerTests()
		{
			_correlationContextAccessor = new CorrelationContextAccessor();

			_correlationIdFactoryMock = new Mock<ICorrelationIdFactory>();
			_correlationIdFactoryMock
				.Setup(m => m.Create())
				.Returns(() => GeneratedCorrelationId)
				.Verifiable();

			_sut = new CorrelationManager(
				new CorrelationContextFactory(_correlationContextAccessor),
				_correlationIdFactoryMock.Object,
				new TestLogger<CorrelationManager>()
			);
		}

		[Fact]
		public async Task Given_a_task_should_run_task_inside_correlated_context()
		{
			// Pre-assert
			_correlationContextAccessor.CorrelationContext.Should().BeNull();

			// Act
			await _sut.CorrelateAsync(() =>
			{
				// Inline assert
				_correlationContextAccessor.CorrelationContext.Should().NotBeNull();
				return Task.CompletedTask;
			});

			// Post-assert
			_correlationContextAccessor.CorrelationContext.Should().BeNull();
		}

		[Fact]
		public async Task When_running_correlated_task_without_correlation_id_should_use_generate_one()
		{
			// Act
			await _sut.CorrelateAsync(() =>
			{
				// Inline assert
				_correlationContextAccessor.CorrelationContext.CorrelationId.Should().Be(GeneratedCorrelationId);
				return Task.CompletedTask;
			});

			_correlationIdFactoryMock.Verify();
		}

		[Fact]
		public async Task When_running_correlated_task_with_correlation_id_should_use_it()
		{
			const string correlationId = "my-correlation-id";

			// Act
			await _sut.CorrelateAsync(correlationId, () =>
			{
				// Inline assert
				_correlationContextAccessor.CorrelationContext.CorrelationId.Should().Be(correlationId);
				return Task.CompletedTask;
			});

			_correlationIdFactoryMock.Verify(m => m.Create(), Times.Never);
		}

		[Fact]
		public void When_not_providing_task_when_starting_correlation_should_throw()
		{
			// Act
			Func<Task> act = () => _sut.CorrelateAsync(null, null);

			// Assert
			act.Should()
				.Throw<ArgumentNullException>()
				.Which.ParamName.Should()
				.Be("correlatedTask");
		}
	}
}