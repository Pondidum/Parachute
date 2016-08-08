using System;
using Parachute.Policies;
using Shouldly;
using Xunit;

namespace Parachute.Tests.Policies
{
	public class ExponentialBackoffPolicyTests
	{
		[Theory]
		[InlineData(0, 0)]
		[InlineData(1, 1)]
		[InlineData(2, 4)]
		[InlineData(3, 9)]
		[InlineData(4, 16)]
		public void When_getting_the_exponential_value(int input, int expected)
		{
			var policy = new ExponentialBackoffPolicy();
			
			policy.GetDelay(input).ShouldBe(TimeSpan.FromSeconds(expected));
		}
	}
}
