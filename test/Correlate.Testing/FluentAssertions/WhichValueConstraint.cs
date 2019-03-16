using FluentAssertions;

namespace Correlate.Testing.FluentAssertions
{
	public class WhichValueConstraint<TParentConstraint, TValue> : AndConstraint<TParentConstraint>
	{
		public WhichValueConstraint(
			TParentConstraint parentConstraint,
			TValue value)
			: base(parentConstraint)
		{
			WhichValue = value;
		}

		/// <summary>Gets the value of the object referred to by the key.</summary>
		public TValue WhichValue { get; private set; }
	}
}