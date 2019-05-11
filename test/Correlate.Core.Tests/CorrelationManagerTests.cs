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
			Func<Task> act = () => _sut.CorrelateAsync(null, null, null);

			// Assert
			act.Should()
				.Throw<ArgumentNullException>()
				.Which.ParamName.Should()
				.Be("correlatedTask");
		}

		[Fact]
		public void When_provided_task_throws_should_not_wrap_exception()
		{
			var exception = new Exception();
			Task ThrowingTask() => throw exception;

			// Act
			Func<Task> act = () => _sut.CorrelateAsync(null, ThrowingTask);

			// Assert
			act.Should().Throw<Exception>().Which.Should().Be(exception);
		}

		[Fact]
		public void When_provided_task_throws_should_enrich_exception_with_correlationId()
		{
			var exception = new Exception();
			Task ThrowingTask() => throw exception;

			// Act
			Func<Task> act = () => _sut.CorrelateAsync(null, ThrowingTask);

			// Assert
			IDictionary exceptionData = act.Should().Throw<Exception>().Which.Data;
			exceptionData.Keys.Should().Contain(CorrelateConstants.CorrelationIdKey);
			exceptionData[CorrelateConstants.CorrelationIdKey].Should().Be(GeneratedCorrelationId);
		}

		[Fact]
		public void When_handling_exception_with_delegate_should_not_throw()
		{
			var exception = new Exception();
			const bool handlesException = true;

			// Act
			Func<Task> act = () => _sut.CorrelateAsync(
				null,
				() => throw exception,
				(ctx, ex) =>
				{
					ctx.CorrelationId.Should().Be(GeneratedCorrelationId);
					ex.Should().Be(exception);
					return handlesException;
				});

			// Assert
			act.Should().NotThrow();
		}

		[Fact]
		public void When_not_handling_exception_with_delegate_should_still_throw()
		{
			var exception = new Exception();
			const bool handlesException = false;

			// Act
			Func<Task> act = () => _sut.CorrelateAsync(
				() => throw exception,
				(ctx, ex) => handlesException
			);

			// Assert
			act.Should().Throw<Exception>().Which.Should().Be(exception);
		}
	}
}