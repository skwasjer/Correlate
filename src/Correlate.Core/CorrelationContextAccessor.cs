namespace Correlate;

/// <summary>
/// Provides access to the <see cref="CorrelationContext" />.
/// </summary>
public class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<CorrelationContextHolder> CurrentContext = new();

    /// <inheritdoc />
    public CorrelationContext? CorrelationContext
    {
        get => CurrentContext.Value?.Context;
        set
        {
            CorrelationContextHolder? holder = CurrentContext.Value;
            switch (value)
            {
                case null when holder is null:
                    return;

                case null:
                {
                    // Restore parent context as current context (if any), and clear current context.
                    if (holder.ParentContext is not null)
                    {
                        CurrentContext.Value = holder.ParentContext;
                    }

                    holder.Context = null;
                    holder.ParentContext = null;
                    break;
                }

                default:
                    // Use an object indirection to hold the CorrelationContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    CurrentContext.Value = new CorrelationContextHolder { Context = value, ParentContext = holder };
                    break;
            }
        }
    }

    private class CorrelationContextHolder
    {
        public CorrelationContext? Context { get; set; }
        public CorrelationContextHolder? ParentContext { get; set; }
    }
}
