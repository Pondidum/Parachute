using System;
using Shouldly;
using Xunit;

namespace Parachute.Tests
{
	public class FallbackTests
	{
		[Fact]
		public void When_no_actions_are_supplied()
		{
			Should.NotThrow(() => Fallback.Run());
		}

		[Fact]
		public void When_the_first_works()
		{
			var first = false;
			var second = false;
			var third = false;

			Fallback.Run(
				() => { first = true; },
				() => { second = true; },
				() => { third = true; }
			);

			first.ShouldBe(true);
			second.ShouldBe(false);
			third.ShouldBe(false);
		}

		[Fact]
		public void When_the_second_works()
		{
			var first = false;
			var second = false;
			var third = false;

			Fallback.Run(
				() => { throw new NotSupportedException(); },
				() => { second = true; },
				() => { third = true; }
			);

			first.ShouldBe(false);
			second.ShouldBe(true);
			third.ShouldBe(false);
		}

		[Fact]
		public void When_none_work()
		{
			var first = false;
			var second = false;
			var third = false;

			Should.Throw<NotSupportedException>(() =>
				Fallback.Run(
					() => { throw new NotSupportedException(); },
					() => { throw new NotSupportedException(); },
					() => { throw new NotSupportedException(); }
				)
			);

			first.ShouldBe(false);
			second.ShouldBe(false);
			third.ShouldBe(false);
		}

		[Fact]
		public void When_creating_a_promise_version()
		{
			var first = false;

			var promise = Fallback.Create(
				() => { first = true; }
			);

			first.ShouldBe(false);

			promise();

			first.ShouldBe(true);
		}
	}
}
