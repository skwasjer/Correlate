using System.Collections;
using Correlate.Testing;
using Correlate.Testing.TestCases;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;
using Serilog.Sinks.TestCorrelator;

namespace Correlate;

public class CorrelationManagerTests : IDisposable
{
    private const string GeneratedCorrelationId = "generated-correlation-id";
    private readonly CorrelationContextAccessor _correlationContextAccessor;
    private readonly ICorrelationIdFactory _correlationIdFactoryMock;
    private readonly ILogger<CorrelationManager> _logger;
    private readonly IOptions<CorrelationManagerOptions> _options;
    private readonly SerilogLoggerProvider _logProvider;
    private readonly CorrelationManager _sut;

    protected CorrelationManagerTests(CorrelationManagerOptions options)
    {
        _correlationContextAccessor = new CorrelationContextAccessor();

        _correlationIdFactoryMock = Substitute.For<ICorrelationIdFactory>();
        _correlationIdFactoryMock
            .Create()
            .Returns(GeneratedCorrelationId);

        Logger serilogLogger = new LoggerConfiguration()
            .WriteTo.TestCorrelator()
            .CreateLogger();

        _logProvider = new SerilogLoggerProvider(serilogLogger);
        _logger = new TestLogger<CorrelationManager>(_logProvider.CreateLogger(nameof(CorrelationManager)));
        _options = Options.Create(options);

        _sut = new CorrelationManager(
            new CorrelationContextFactory(_correlationContextAccessor),
            _correlationIdFactoryMock,
            _correlationContextAccessor,
            _logger,
            _options
        );
    }

    public void Dispose()
    {
        _logProvider.Dispose();
        GC.SuppressFinalize(this);
    }

    public class Async : CorrelationManagerTests
    {
        public Async() : base(new()
        {
            LoggingScopeKey = "ActivityId"
        })
        {
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
        public async Task Given_provided_task_throws_but_exception_delegate_is_null_it_should_just_rethrow()
        {
            var exception = new Exception();

            Task ThrowingTask()
            {
                throw exception;
            }

            // Act
            Func<Task> act = () => _sut.CorrelateAsync(null, ThrowingTask, null);

            // Assert
            (await act.Should().ThrowAsync<Exception>()).Which.Should().Be(exception);
        }

        [Fact]
        public async Task Given_provided_task_with_returnValue_throws_but_exception_delegate_is_null_it_should_just_rethrow()
        {
            var exception = new Exception();

            Task<int> ThrowingTask()
            {
                throw exception;
            }

            // Act
            Func<Task<int>> act = () => _sut.CorrelateAsync(null, ThrowingTask, null);

            // Assert
            (await act.Should().ThrowAsync<Exception>()).Which.Should().Be(exception);
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
                var logEvents = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
                logEvents.Should()
                    .HaveCount(3)
                    .And.ContainSingle(ev => ev.MessageTemplate.Text == "Message with correlation id." && ev.Properties.ContainsKey("ActivityId"));
            }
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
            (await act.Should().NotThrowAsync()).Which.Should().Be(returnValue);
        }

        [Fact]
        public async Task When_handling_exception_with_delegate_should_not_throw()
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
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task When_not_handling_exception_with_delegate_should_still_throw()
        {
            var exception = new Exception();
            const bool handlesException = false;

            // Act
            Func<Task> act = () => _sut.CorrelateAsync(
                () => throw exception,
                ctx => ctx.IsExceptionHandled = handlesException
            );

            // Assert
            (await act.Should().ThrowAsync<Exception>()).Which.Should().Be(exception);
        }

        [Fact]
        public async Task When_provided_task_throws_should_enrich_exception_with_correlationId()
        {
            var exception = new Exception();

            Task ThrowingTask()
            {
                throw exception;
            }

            // Act
            Func<Task> act = () => _sut.CorrelateAsync(null, ThrowingTask);

            // Assert
            IDictionary exceptionData = (await act.Should().ThrowAsync<Exception>()).Which.Data;
            exceptionData
                .Cast<DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                .Should()
                .ContainKey("ActivityId")
                .WhoseValue.Should()
                .Be(GeneratedCorrelationId);
        }

        [Fact]
        public async Task When_provided_task_throws_should_not_wrap_exception()
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
            (await act.Should().ThrowAsync<Exception>()).Which.Should().Be(exception);
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
                    _correlationContextAccessor.CorrelationContext?.CorrelationId.Should().Be(correlationId);
                    return Task.CompletedTask;
                });

            _correlationIdFactoryMock.DidNotReceive().Create();
        }

        [Fact]
        public async Task When_running_correlated_task_without_correlation_id_should_use_generate_one()
        {
            // Act
            await _sut.CorrelateAsync(() =>
            {
                // Inline assert
                _correlationContextAccessor.CorrelationContext?.CorrelationId.Should().Be(GeneratedCorrelationId);
                return Task.CompletedTask;
            });

            _correlationIdFactoryMock.Received(1).Create();
        }

        [Fact]
        public Task When_starting_correlationContext_inside_running_context_with_same_id_should_reuse()
        {
            return _sut.CorrelateAsync(async () =>
            {
                CorrelationContext? parentContext = _correlationContextAccessor.CorrelationContext;
                parentContext.Should().NotBeNull();

                await _sut.CorrelateAsync(parentContext?.CorrelationId,
                    () =>
                    {
                        CorrelationContext? innerContext = _correlationContextAccessor.CorrelationContext;
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
                CorrelationContext? parentContext = _correlationContextAccessor.CorrelationContext;

                await _sut.CorrelateAsync(() =>
                {
                    CorrelationContext? innerContext = _correlationContextAccessor.CorrelationContext;
                    innerContext.Should()
                        .NotBe(parentContext)
                        .And.BeEquivalentTo(parentContext);

                    return Task.CompletedTask;
                });
            });
        }

        [Fact]
        public Task When_starting_correlationContext_when_another_context_is_active_should_start_new()
        {
            const string parentContextId = nameof(parentContextId);
            const string innerContextId = nameof(innerContextId);

            return _sut.CorrelateAsync(parentContextId,
                async () =>
                {
                    CorrelationContext? parentContext = _correlationContextAccessor.CorrelationContext;
                    parentContext.Should().NotBeNull();
                    parentContext?.CorrelationId.Should().Be(parentContextId);

                    await _sut.CorrelateAsync(innerContextId,
                        () =>
                        {
                            CorrelationContext? innerContext = _correlationContextAccessor.CorrelationContext;
                            innerContext.Should().NotBeNull();
                            innerContext.Should().NotBe(parentContext);
                            innerContext?.CorrelationId.Should().Be(innerContextId);

                            return Task.CompletedTask;
                        });

                    _correlationContextAccessor.CorrelationContext.Should().NotBeNull();

                    _correlationContextAccessor.CorrelationContext?
                        .CorrelationId
                        .Should()
                        .Be(parentContextId);
                });
        }
    }

    public class Sync : CorrelationManagerTests
    {
        public Sync() : base(new())
        {
        }

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
        public void Given_provided_action_throws_but_exception_delegate_is_null_it_should_just_rethrow()
        {
            var exception = new Exception();

            void ThrowingAction()
            {
                throw exception;
            }

            // Act
            Action act = () => _sut.Correlate(null, ThrowingAction, null);

            // Assert
            act.Should().Throw<Exception>().Which.Should().Be(exception);
        }

        [Fact]
        public void Given_provided_func_throws_but_exception_delegate_is_null_it_should_just_rethrow()
        {
            var exception = new Exception();

            int ThrowingFunc()
            {
                throw exception;
            }

            // Act
            Func<int> act = () => _sut.Correlate(null, ThrowingFunc, null);

            // Assert
            act.Should().Throw<Exception>().Which.Should().Be(exception);
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
                var logEvents = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
                logEvents.Should()
                    .HaveCount(3)
                    .And.ContainSingle(ev => ev.MessageTemplate.Text == "Message with correlation id." && ev.Properties.ContainsKey("CorrelationId"));
            }
        }

        [Fact]
        public void When_handling_exception_by_returning_new_value_should_not_throw()
        {
            var exception = new Exception();

            int ThrowingFunc()
            {
                throw exception;
            }

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
        public void When_provided_action_throws_should_enrich_exception_with_correlationId()
        {
            var exception = new Exception();

            void ThrowingAction()
            {
                throw exception;
            }

            // Act
            Action act = () => _sut.Correlate(null, ThrowingAction);

            // Assert
            IDictionary exceptionData = act.Should().Throw<Exception>().Which.Data;
            exceptionData
                .Cast<DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                .Should()
                .ContainKey(CorrelateConstants.CorrelationIdKey)
                .WhoseValue.Should()
                .Be(GeneratedCorrelationId);
        }

        [Fact]
        public void When_provided_action_throws_should_not_wrap_exception()
        {
            var exception = new Exception();

            void ThrowingAction()
            {
                throw exception;
            }

            // Act
            Action act = () => _sut.Correlate(null, ThrowingAction);

            // Assert
            act.Should().Throw<Exception>().Which.Should().Be(exception);
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
                    _correlationContextAccessor.CorrelationContext?.CorrelationId.Should().Be(correlationId);
                });

            _correlationIdFactoryMock.DidNotReceive().Create();
        }

        [Fact]
        public void When_running_correlated_action_without_correlation_id_should_use_generate_one()
        {
            // Act
            _sut.Correlate(() =>
            {
                // Inline assert
                _correlationContextAccessor.CorrelationContext?.CorrelationId.Should().Be(GeneratedCorrelationId);
            });

            _correlationIdFactoryMock.Received(1).Create();
        }

        [Fact]
        public void When_starting_correlationContext_inside_running_context_with_same_id_should_reuse()
        {
            _sut.Correlate(() =>
            {
                CorrelationContext? parentContext = _correlationContextAccessor.CorrelationContext;
                parentContext.Should().NotBeNull();

                _sut.Correlate(parentContext?.CorrelationId,
                    () =>
                    {
                        CorrelationContext? innerContext = _correlationContextAccessor.CorrelationContext;
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
                CorrelationContext? parentContext = _correlationContextAccessor.CorrelationContext;

                _sut.Correlate(() =>
                {
                    CorrelationContext? innerContext = _correlationContextAccessor.CorrelationContext;
                    innerContext.Should()
                        .NotBe(parentContext)
                        .And.BeEquivalentTo(parentContext);
                });
            });
        }

        [Fact]
        public void When_starting_correlationContext_when_another_context_is_active_should_start_new()
        {
            const string parentContextId = nameof(parentContextId);
            const string innerContextId = nameof(innerContextId);

            _sut.Correlate(parentContextId,
                () =>
                {
                    CorrelationContext? parentContext = _correlationContextAccessor.CorrelationContext;
                    parentContext.Should().NotBeNull();
                    parentContext?.CorrelationId.Should().Be(parentContextId);

                    _sut.Correlate(innerContextId,
                        () =>
                        {
                            CorrelationContext? innerContext = _correlationContextAccessor.CorrelationContext;
                            innerContext.Should().NotBeNull();
                            innerContext.Should().NotBe(parentContext);
                            innerContext?.CorrelationId.Should().Be(innerContextId);
                        });

                    _correlationContextAccessor.CorrelationContext.Should().NotBeNull();

                    _correlationContextAccessor.CorrelationContext?
                        .CorrelationId
                        .Should()
                        .Be(parentContextId);
                });
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
                Substitute.For<ICorrelationContextFactory>(),
                Substitute.For<ICorrelationIdFactory>(),
                Substitute.For<ICorrelationContextAccessor>(),
                Substitute.For<ILogger<CorrelationManager>>(),
                Substitute.For<IOptions<CorrelationManagerOptions>>()
            );

            static Task CorrelatedTask()
            {
                return Task.CompletedTask;
            }

            static Task<int> ReturningCorrelatedTask()
            {
                return Task.FromResult(1);
            }

            static void CorrelatedAction() { }

            static int ReturningCorrelatedFunc()
            {
                return 1;
            }

            return new[]
                {
                    // Instance members
                    DelegateTestCase.Create(instance.CorrelateAsync, (string?)null, (Func<Task>)CorrelatedTask, (OnException?)null),
                    DelegateTestCase.Create(instance.CorrelateAsync, (string?)null, (Func<Task<int>>)ReturningCorrelatedTask, (OnException<int>?)null),
                    DelegateTestCase.Create(instance.Correlate, (string?)null, (Action)CorrelatedAction, (OnException?)null),
                    DelegateTestCase.Create(instance.Correlate, (string?)null, (Func<int>)ReturningCorrelatedFunc, (OnException<int>?)null),
                    // Extensions
                    DelegateTestCase.Create(AsyncCorrelationManagerExtensions.CorrelateAsync, instance, (Func<Task>)CorrelatedTask),
                    DelegateTestCase.Create(AsyncCorrelationManagerExtensions.CorrelateAsync, instance, (Func<Task<int>>)ReturningCorrelatedTask),
                    DelegateTestCase.Create(CorrelationManagerExtensions.Correlate, instance, (Action)CorrelatedAction),
                    DelegateTestCase.Create(CorrelationManagerExtensions.Correlate, instance, (Func<int>)ReturningCorrelatedFunc),
                    DelegateTestCase.Create(AsyncCorrelationManagerExtensions.CorrelateAsync, instance, (Func<Task>)CorrelatedTask, (OnException?)null),
                    DelegateTestCase.Create(AsyncCorrelationManagerExtensions.CorrelateAsync, instance, (Func<Task<int>>)ReturningCorrelatedTask, (OnException<int>?)null),
                    DelegateTestCase.Create(CorrelationManagerExtensions.Correlate, instance, (Action)CorrelatedAction, (OnException?)null),
                    DelegateTestCase.Create(CorrelationManagerExtensions.Correlate, instance, (Func<int>)ReturningCorrelatedFunc, (OnException<int>?)null)
                }
                .SelectMany(tc => tc.GetNullArgumentTestCases());
        }
    }
}
