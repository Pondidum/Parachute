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

		[Fact]
		public void For_contextual_when_no_actions_are_supplied()
		{
			var context = new Context { Name = "first" };
			Should.NotThrow(() => Fallback.Run(context));
		}

		[Fact]
		public void For_contextual_when_the_first_works()
		{
			var context = new Context { Name = "first" };
			var first = "";
			var second = "";
			var third = "";

			Fallback.Run(
				context,
				cx => { first = cx.Name; },
				cx => { second = cx.Name; },
				cx => { third = cx.Name; }
			);

			first.ShouldBe(context.Name);
			second.ShouldBe("");
			third.ShouldBe("");
		}

		[Fact]
		public void For_contextual_when_the_second_works()
		{
			var context = new Context { Name = "first" };
			var first = "";
			var second = "";
			var third = "";

			Fallback.Run(
				context,
				cx => { throw new NotSupportedException(); },
				cx => { second = cx.Name; },
				cx => { third = cx.Name; }
			);

			first.ShouldBe("");
			second.ShouldBe(context.Name);
			third.ShouldBe("");
		}

		[Fact]
		public void For_contextual_when_none_work()
		{
			var context = new Context { Name = "first" };
			var first = "";
			var second = "";
			var third = "";

			Should.Throw<NotSupportedException>(() =>
				Fallback.Run(
					context,
					cx => { throw new NotSupportedException(); },
					cx => { throw new NotSupportedException(); },
					cx => { throw new NotSupportedException(); }
				)
			);

			first.ShouldBe("");
			second.ShouldBe("");
			third.ShouldBe("");
		}

		[Fact]
		public void For_contextual_when_creating_a_promise_version()
		{
			var first = "";

			var promise = Fallback.Create<Context>(
				cx => { first = cx.Name; }
			);

			first.ShouldBe("");

			promise(new Context { Name = "A"});
			first.ShouldBe("A");

			promise(new Context { Name = "B" });
			first.ShouldBe("B");
		}

		private class Context
		{
			public string Name { get; set; }
		}
	}
}
