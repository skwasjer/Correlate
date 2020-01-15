using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Correlate.Testing;
using Correlate.Testing.TestCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.TestCorrelator;
using Xunit;

namespace Correlate
{
	public class CorrelationManagerTests : IDisposable
	{
		private const string GeneratedCorrelationId = "generated-correlation-id";
		private readonly CorrelationContextAccessor _correlationContextAccessor;
		private readonly CorrelationManager _sut;
		private readonly Mock<ICorrelationIdFactory> _correlationIdFactoryMock;
		private readonly ILogger<CorrelationManager> _logger;
		private readonly SerilogLoggerProvider _logProvider;

		public CorrelationManagerTests()
		{
			_correlationContextAccessor = new CorrelationContextAccessor();

			_correlationIdFactoryMock = new Mock<ICorrelationIdFactory>();
			_correlationIdFactoryMock
				.Setup(m => m.Create())
				.Returns(() => GeneratedCorrelationId)
				.Verifiable();

			Logger serilogLogger = new LoggerConfiguration()
				.WriteTo.TestCorrelator()
				.CreateLogger();

			_logProvider = new SerilogLoggerProvider(serilogLogger);
			_logger = new TestLogger<CorrelationManager>(_logProvider.CreateLogger(nameof(CorrelationManager)));

			_sut = new CorrelationManager(
				new CorrelationContextFactory(_correlationContextAccessor),
				_correlationIdFactoryMock.Object,
				_correlationContextAccessor,
				_logger
			);
		}

		public void Dispose()
		{
			_logProvider.Dispose();
		}

		public class Async : CorrelationManagerTests
		{
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
				await _sut.CorrelateAsync(correlationId,
					() =>
					{
						// Inline assert
						_correlationContextAccessor.CorrelationContext.CorrelationId.Should().Be(correlationId);
						return Task.CompletedTask;
					});

				_correlationIdFactoryMock.Verify(m => m.Create(), Times.Never);
			}

			[Fact]
			public void When_provided_task_throws_should_not_wrap_exception()
			{
				var exception = new Exception();
				async Task ThrowingTask()
				{
					await Task.Yield();
					throw exception;
				}

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
					ctx =>
					{
						ctx.CorrelationContext.CorrelationId.Should().Be(GeneratedCorrelationId);
						ctx.Exception.Should().Be(exception);
						ctx.IsExceptionHandled = handlesException;
					});

				// Assert
				act.Should().NotThrow();
			}

			[Fact]
			public async Task When_handling_exception_by_returning_new_value_should_not_throw()
			{
				var exception = new Exception();
				async Task<int> ThrowingTask()
				{
					await Task.Yield();
					throw exception;
				}
				const int returnValue = 12345;

				// Act
				Func<Task<int>> act = () => _sut.CorrelateAsync(
					null,
					ThrowingTask,
					ctx =>
					{
						ctx.CorrelationContext.CorrelationId.Should().Be(GeneratedCorrelationId);
						ctx.Exception.Should().Be(exception);
						ctx.Result = returnValue;
					});

				// Assert
				act.Should().NotThrow();
				(await act()).Should().Be(returnValue);
			}

			[Fact]
			public void When_not_handling_exception_with_delegate_should_still_throw()
			{
				var exception = new Exception();
				const bool handlesException = false;

				// Act
				Func<Task> act = () => _sut.CorrelateAsync(
					() => throw exception,
					ctx => ctx.IsExceptionHandled = handlesException
				);

				// Assert
				act.Should().Throw<Exception>().Which.Should().Be(exception);
			}

			[Fact]
			public Task When_starting_correlationContext_when_another_context_is_active_should_start_new()
			{
				const string parentContextId = nameof(parentContextId);
				const string innerContextId = nameof(innerContextId);

				return _sut.CorrelateAsync(parentContextId,
					async () =>
					{
						CorrelationContext parentContext = _correlationContextAccessor.CorrelationContext;
						parentContext.Should().NotBeNull();
						parentContext.CorrelationId.Should().Be(parentContextId);

						await _sut.CorrelateAsync(innerContextId,
							() =>
							{
								CorrelationContext innerContext = _correlationContextAccessor.CorrelationContext;
								innerContext.Should().NotBeNull();
								innerContext.Should().NotBe(parentContext);
								innerContext.CorrelationId.Should().Be(innerContextId);

								return Task.CompletedTask;
							});

						_correlationContextAccessor.CorrelationContext.Should().NotBeNull();

						_correlationContextAccessor.CorrelationContext
							.CorrelationId
							.Should()
							.Be(parentContextId);
					});
			}

			[Fact]
			public Task When_starting_correlationContext_inside_running_context_with_same_id_should_reuse()
			{
				return _sut.CorrelateAsync(async () =>
				{
					CorrelationContext parentContext = _correlationContextAccessor.CorrelationContext;

					await _sut.CorrelateAsync(parentContext.CorrelationId,
						() =>
						{
							CorrelationContext innerContext = _correlationContextAccessor.CorrelationContext;
							innerContext.Should()
								.NotBe(parentContext)
								.And.BeEquivalentTo(parentContext);

							return Task.CompletedTask;
						});
				});
			}

			[Fact]
			public Task When_starting_correlationContext_inside_running_context_without_specifying_should_reuse()
			{
				return _sut.CorrelateAsync(async () =>
				{
					CorrelationContext parentContext = _correlationContextAccessor.CorrelationContext;

					await _sut.CorrelateAsync(() =>
					{
						CorrelationContext innerContext = _correlationContextAccessor.CorrelationContext;
						innerContext.Should()
							.NotBe(parentContext)
							.And.BeEquivalentTo(parentContext);

						return Task.CompletedTask;
					});
				});
			}

			[Fact]
			public Task When_starting_correlationContext_with_legacy_ctor_when_another_context_is_active_should_not_throw()
			{
				const string parentContextId = nameof(parentContextId);

#pragma warning disable 618 // justification, covering legacy implementation (pre v3.0)
				var sut = new CorrelationManager(
					new CorrelationContextFactory(_correlationContextAccessor),
					_correlationIdFactoryMock.Object,
					new NullLogger<CorrelationManager>()
				);
#pragma warning restore 618

				return sut.CorrelateAsync(parentContextId,
					async () =>
					{
						CorrelationContext parentContext = _correlationContextAccessor.CorrelationContext;
						parentContext.Should().NotBeNull();
						parentContext.CorrelationId.Should().Be(parentContextId);

						await sut.CorrelateAsync(() =>
						{
							CorrelationContext innerContext = _correlationContextAccessor.CorrelationContext;
							innerContext.Should().NotBeNull().And.NotBe(parentContext);
							innerContext.CorrelationId.Should().NotBe(parentContextId);

							return Task.CompletedTask;
						});
					});
			}

			[Fact]
			public async Task Given_task_returns_a_value_when_executed_should_return_value()
			{
				const int value = 12345;

				// Pre-assert
				_correlationContextAccessor.CorrelationContext.Should().BeNull();

				// Act
				int actual = await _sut.CorrelateAsync(() =>
				{
					// Inline assert
					_correlationContextAccessor.CorrelationContext.Should().NotBeNull();
					return Task.FromResult(value);
				});

				// Post-assert
				actual.Should().Be(value);
				_correlationContextAccessor.CorrelationContext.Should().BeNull();
			}

			[Fact]
			public async Task Should_create_log_scope()
			{
				using (TestCorrelator.CreateContext())
				{
					_logger.LogInformation("Start message without correlation id.");

					// Act
					await _sut.CorrelateAsync(() =>
					{
						_logger.LogInformation("Message with correlation id.");
						return Task.CompletedTask;
					});

					_logger.LogInformation("End message without correlation id.");

					// Assert
					List<LogEvent> logEvents = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
					logEvents.Should()
						.HaveCount(3)
						.And.ContainSingle(ev => ev.MessageTemplate.Text == "Message with correlation id." && ev.Properties.ContainsKey("CorrelationId"));
				}
			}

			[Fact]
			public void Given_provided_task_throws_but_exception_delegate_is_null_it_should_just_rethrow()
			{
				var exception = new Exception();
				Task ThrowingTask() => throw exception;

				// Act
				Func<Task> act = () => _sut.CorrelateAsync(null, ThrowingTask, null);

				// Assert
				act.Should().Throw<Exception>().Which.Should().Be(exception);
			}

			[Fact]
			public void Given_provided_task_with_returnValue_throws_but_exception_delegate_is_null_it_should_just_rethrow()
			{
				var exception = new Exception();
				Task<int> ThrowingTask() => throw exception;

				// Act
				Func<Task<int>> act = () => _sut.CorrelateAsync(null, ThrowingTask, null);

				// Assert
				act.Should().Throw<Exception>().Which.Should().Be(exception);
			}
		}

		public class Sync : CorrelationManagerTests
		{
			[Fact]
			public void Given_a_action_should_run_action_inside_correlated_context()
			{
				// Pre-assert
				_correlationContextAccessor.CorrelationContext.Should().BeNull();

				// Act
				_sut.Correlate(() =>
				{
					// Inline assert
					_correlationContextAccessor.CorrelationContext.Should().NotBeNull();
				});

				// Post-assert
				_correlationContextAccessor.CorrelationContext.Should().BeNull();
			}

			[Fact]
			public void When_running_correlated_action_without_correlation_id_should_use_generate_one()
			{
				// Act
				_sut.Correlate(() =>
				{
					// Inline assert
					_correlationContextAccessor.CorrelationContext.CorrelationId.Should().Be(GeneratedCorrelationId);
				});

				_correlationIdFactoryMock.Verify();
			}

			[Fact]
			public void When_running_correlated_action_with_correlation_id_should_use_it()
			{
				const string correlationId = "my-correlation-id";

				// Act
				_sut.Correlate(correlationId,
					() =>
					{
						// Inline assert
						_correlationContextAccessor.CorrelationContext.CorrelationId.Should().Be(correlationId);
					});

				_correlationIdFactoryMock.Verify(m => m.Create(), Times.Never);
			}

			[Fact]
			public void When_provided_action_throws_should_not_wrap_exception()
			{
				var exception = new Exception();

				void ThrowingAction() => throw exception;

				// Act
				Action act = () => _sut.Correlate(null, ThrowingAction);

				// Assert
				act.Should().Throw<Exception>().Which.Should().Be(exception);
			}

			[Fact]
			public void When_provided_action_throws_should_enrich_exception_with_correlationId()
			{
				var exception = new Exception();
				void ThrowingAction() => throw exception;

				// Act
				Action act = () => _sut.Correlate(null, ThrowingAction);

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
				Action act = () => _sut.Correlate(
					null,
					() => throw exception,
					ctx =>
					{
						ctx.CorrelationContext.CorrelationId.Should().Be(GeneratedCorrelationId);
						ctx.Exception.Should().Be(exception);
						ctx.IsExceptionHandled = handlesException;
					});

				// Assert
				act.Should().NotThrow();
			}

			[Fact]
			public void When_handling_exception_by_returning_new_value_should_not_throw()
			{
				var exception = new Exception();

				int ThrowingFunc() => throw exception;

				const int returnValue = 12345;

				// Act
				Func<int> act = () => _sut.Correlate(
					null,
					ThrowingFunc,
					ctx =>
					{
						ctx.CorrelationContext.CorrelationId.Should().Be(GeneratedCorrelationId);
						ctx.Exception.Should().Be(exception);
						ctx.Result = returnValue;
					});

				// Assert
				act.Should().NotThrow();
				act().Should().Be(returnValue);
			}

			[Fact]
			public void When_not_handling_exception_with_delegate_should_still_throw()
			{
				var exception = new Exception();
				const bool handlesException = false;

				// Act
				Action act = () => _sut.Correlate(
					() => throw exception,
					ctx => ctx.IsExceptionHandled = handlesException
				);

				// Assert
				act.Should().Throw<Exception>().Which.Should().Be(exception);
			}

			[Fact]
			public void When_starting_correlationContext_when_another_context_is_active_should_start_new()
			{
				const string parentContextId = nameof(parentContextId);
				const string innerContextId = nameof(innerContextId);

				_sut.Correlate(parentContextId,
					() =>
					{
						CorrelationContext parentContext = _correlationContextAccessor.CorrelationContext;
						parentContext.Should().NotBeNull();
						parentContext.CorrelationId.Should().Be(parentContextId);

						_sut.Correlate(innerContextId,
							() =>
							{
								CorrelationContext innerContext = _correlationContextAccessor.CorrelationContext;
								innerContext.Should().NotBeNull();
								innerContext.Should().NotBe(parentContext);
								innerContext.CorrelationId.Should().Be(innerContextId);
							});

						_correlationContextAccessor.CorrelationContext.Should().NotBeNull();

						_correlationContextAccessor.CorrelationContext
							.CorrelationId
							.Should()
							.Be(parentContextId);
					});
			}

			[Fact]
			public void When_starting_correlationContext_inside_running_context_with_same_id_should_reuse()
			{
				_sut.Correlate(() =>
				{
					CorrelationContext parentContext = _correlationContextAccessor.CorrelationContext;

					_sut.Correlate(parentContext.CorrelationId,
						() =>
						{
							CorrelationContext innerContext = _correlationContextAccessor.CorrelationContext;
							innerContext.Should()
								.NotBe(parentContext)
								.And.BeEquivalentTo(parentContext);
						});
				});
			}

			[Fact]
			public void When_starting_correlationContext_inside_running_context_without_specifying_should_reuse()
			{
				_sut.Correlate(() =>
				{
					CorrelationContext parentContext = _correlationContextAccessor.CorrelationContext;

					_sut.Correlate(() =>
					{
						CorrelationContext innerContext = _correlationContextAccessor.CorrelationContext;
						innerContext.Should()
							.NotBe(parentContext)
							.And.BeEquivalentTo(parentContext);
					});
				});
			}

			[Fact]
			public void Given_func_returns_a_value_when_executed_should_return_value()
			{
				const int value = 12345;

				// Pre-assert
				_correlationContextAccessor.CorrelationContext.Should().BeNull();

				// Act
				int actual = _sut.Correlate(() =>
				{
					// Inline assert
					_correlationContextAccessor.CorrelationContext.Should().NotBeNull();
					return value;
				});

				// Post-assert
				actual.Should().Be(value);
				_correlationContextAccessor.CorrelationContext.Should().BeNull();
			}

			[Fact]
			public void Should_create_log_scope()
			{
				using (TestCorrelator.CreateContext())
				{
					_logger.LogInformation("Start message without correlation id.");

					// Act
					_sut.Correlate(() =>
					{
						_logger.LogInformation("Message with correlation id.");
					});

					_logger.LogInformation("End message without correlation id.");

					// Assert
					List<LogEvent> logEvents = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
					logEvents.Should()
						.HaveCount(3)
						.And.ContainSingle(ev => ev.MessageTemplate.Text == "Message with correlation id." && ev.Properties.ContainsKey("CorrelationId"));
				}
			}

			[Fact]
			public void Given_provided_action_throws_but_exception_delegate_is_null_it_should_just_rethrow()
			{
				var exception = new Exception();
				// ReSharper disable once ConvertToLocalFunction
				Action throwingAction = () => throw exception;

				// Act
				Action act = () => _sut.Correlate(null, throwingAction, null);

				// Assert
				act.Should().Throw<Exception>().Which.Should().Be(exception);
			}

			[Fact]
			public void Given_provided_func_throws_but_exception_delegate_is_null_it_should_just_rethrow()
			{
				var exception = new Exception();
				// ReSharper disable once ConvertToLocalFunction
				Func<int> throwingFunc = () => throw exception;

				// Act
				Func<int> act = () => _sut.Correlate(null, throwingFunc, null);

				// Assert
				act.Should().Throw<Exception>().Which.Should().Be(exception);
			}
		}

		public class NullArgChecks
		{

			[Theory]
			[MemberData(nameof(NullArgumentTestCases))]
			public void Given_null_argument_when_executing_it_should_throw(params object[] args)
			{
				NullArgumentTest.Execute(args);
			}

			public static IEnumerable<object[]> NullArgumentTestCases()
			{
				var instance = new CorrelationManager(
					Mock.Of<ICorrelationContextFactory>(),
					Mock.Of<ICorrelationIdFactory>(),
					Mock.Of<ICorrelationContextAccessor>(),
					Mock.Of<ILogger<CorrelationManager>>()
				);
				// ReSharper disable ConvertToLocalFunction
				Func<Task> correlatedTask = () => Task.CompletedTask;
				Func<Task<int>> returningCorrelatedTask = () => Task.FromResult(1);
				Action correlatedAction = () => { };
				Func<int> returningCorrelatedAction = () => 1;
				// ReSharper restore ConvertToLocalFunction

				return new[]
					{
						// Instance members
						DelegateTestCase.Create(instance.CorrelateAsync, (string)null, correlatedTask, (OnException)null),
						DelegateTestCase.Create(instance.CorrelateAsync, (string)null, returningCorrelatedTask, (OnException<int>)null),
						DelegateTestCase.Create(instance.Correlate, (string)null, correlatedAction, (OnException)null),
						DelegateTestCase.Create(instance.Correlate, (string)null, returningCorrelatedAction, (OnException<int>)null),
						// Extensions
						DelegateTestCase.Create(AsyncCorrelationManagerExtensions.CorrelateAsync, instance, correlatedTask),
						DelegateTestCase.Create(AsyncCorrelationManagerExtensions.CorrelateAsync, instance, returningCorrelatedTask),
						DelegateTestCase.Create(CorrelationManagerExtensions.Correlate, instance, correlatedAction),
						DelegateTestCase.Create(CorrelationManagerExtensions.Correlate, instance, returningCorrelatedAction),

						DelegateTestCase.Create(AsyncCorrelationManagerExtensions.CorrelateAsync, instance, correlatedTask, (OnException)null),
						DelegateTestCase.Create(AsyncCorrelationManagerExtensions.CorrelateAsync, instance, returningCorrelatedTask, (OnException<int>)null),
						DelegateTestCase.Create(CorrelationManagerExtensions.Correlate, instance, correlatedAction, (OnException)null),
						DelegateTestCase.Create(CorrelationManagerExtensions.Correlate, instance, returningCorrelatedAction, (OnException<int>)null),
					}
					.SelectMany(tc => tc.GetNullArgumentTestCases());
			}
		}
	}
}
