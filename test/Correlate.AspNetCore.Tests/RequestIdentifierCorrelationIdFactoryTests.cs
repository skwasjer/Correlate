using FluentAssertions;
using Xunit;

namespace Correlate
{
	public class RequestIdentifierCorrelationIdFactoryTests
	{
		private readonly RequestIdentifierCorrelationIdFactory _sut;

		public RequestIdentifierCorrelationIdFactoryTests()
		{
			_sut = new RequestIdentifierCorrelationIdFactory();
		}

		[Fact]
		public void When_creating_should_return_value()
		{
			// Act
			string actual = _sut.Create();
				
			// Assert
			actual.Should().NotBeNullOrWhiteSpace();
		}

		[Fact]
		public void When_creating_twice_should_not_return_same_value()
		{
			// Act
			string actual = _sut.Create();
			string other = _sut.Create();

			// Assert
			actual.Should().NotBe(other);
		}
	}
}
