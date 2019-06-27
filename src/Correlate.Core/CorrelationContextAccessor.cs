using System.Collections.Generic;
using System.Threading;

namespace Correlate
{
	/// <summary>
	/// Provides access to the <see cref="CorrelationContext"/>.
	/// </summary>
	public class CorrelationContextAccessor : ICorrelationContextAccessor
	{
		private static readonly AsyncLocal<CorrelationContextHolder> CurrentContext = new AsyncLocal<CorrelationContextHolder>();

		/// <inheritdoc />
		public CorrelationContext CorrelationContext
		{
			get => CurrentContext.Value?.Context;
			set
			{
				CorrelationContextHolder holder = CurrentContext.Value;
				if (value == null)
				{
					if (holder != null)
					{
						// Clear current CorrelationContext trapped in the AsyncLocals, as its done.
						holder.Context = null;
					}

					return;
				}

				if (holder == null)
				{
					// Use an object indirection to hold the CorrelationContext in the AsyncLocal,
					// so it can be cleared in all ExecutionContexts when its cleared.
					CurrentContext.Value = new CorrelationContextHolder
					{
						Context = value
					};
				}
				else
				{
					holder.Context = value;
				}
			}
		}

		private class CorrelationContextHolder
		{
			private Stack<CorrelationContext> _contextStack;
			private CorrelationContext _currentContext;

			public CorrelationContext Context
			{
				get => _currentContext;
				set
				{
					if (value == null)
					{
						_currentContext = _contextStack?.Count > 0 ? _contextStack.Pop() : null;
						if (_currentContext == null)
						{
							_contextStack = null;
						}
					}
					else
					{
						if (_currentContext != null)
						{
							if (_contextStack == null)
							{
								_contextStack = new Stack<CorrelationContext>();
							}

							_contextStack.Push(_currentContext);
						}

						_currentContext = value;
					}
				}
			}
		}
	}
}
