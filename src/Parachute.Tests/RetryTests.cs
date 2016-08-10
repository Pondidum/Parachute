using System;
using Shouldly;
using Xunit;

namespace Parachute.Tests
{
	public class RetryTests
	{
		private readonly RetryConfiguration _config;

		public RetryTests()
		{
			_config = new RetryConfiguration
			{
				MaxRetries = 5
			};
		}

		[Fact]
		public void When_everything_goes_smoothly()
		{
			var ran = false;
			Action action = () => { ran = true; };

			Retry.Run(action, _config);

			ran.ShouldBe(true);
		}

		[Fact]
		public void When_all_hell_breaks_loose()
		{
			var attempts = 0;
			Action action = () =>
			{
				attempts++;
				throw new InvalidOperationException();
			};

			Should.Throw<InvalidOperationException>(() => Retry.Run(action, _config));
			attempts.ShouldBe(5);
		}

		[Fact]
		public void When_it_succeeds_after_one_error()
		{
			var attempts = 0;
			Action action = () =>
			{
				attempts++;

				if (attempts <= 1)
					throw new InvalidOperationException();
			};

			Retry.Run(action, _config);
			attempts.ShouldBe(2);
		}

		[Fact]
		public void It_can_be_configured_in_multiple_ways()
		{
			Action action = () => { };
			var config = new RetryConfiguration();

			Retry.Run(action, config);
			Retry.Run(action, () => config);
			Retry.Run(action, c =>
			{
				c.MaxRetries = _config.MaxRetries;
			});
		}

		[Fact]
		public void When_creating_a_promise_config_is_created_instantly()
		{
			var actionRan = false;

			Action action = () => actionRan = true;

			var promise = Retry.Create(action, _config);

			actionRan.ShouldBe(false);
		}

		[Fact]
		public void When_creating_a_promise_config_is_created_instantly_for_action_config()
		{
			var configRan = false;
			var actionRan = false;

			Action<RetryConfiguration> configure = config => configRan = true;
			Action action = () => actionRan = true;

			var promise = Retry.Create(action, configure);

			configRan.ShouldBe(true);
			actionRan.ShouldBe(false);
		}

		[Fact]
		public void When_creating_a_promise_config_is_created_instantly_for_func_config()
		{
			var configRan = false;
			var actionRan = false;

			Func<RetryConfiguration> configure = () =>
			{
				configRan = true;
				return _config;
			};
			Action action = () => actionRan = true;

			var promise = Retry.Create(action, configure);

			configRan.ShouldBe(true);
			actionRan.ShouldBe(false);
		}

		[Fact]
		public void For_contextual_when_everything_goes_smoothly()
		{
			var context = new Context();
			var ran = false;
			Action<Context> action = cx => { ran = true; };

			Retry.Run(context, action, _config);

			ran.ShouldBe(true);
		}

		[Fact]
		public void For_contextual_when_all_hell_breaks_loose()
		{
			var context = new Context();
			var attempts = 0;
			Action<Context> action = cx =>
			{
				attempts++;
				throw new InvalidOperationException();
			};

			Should.Throw<InvalidOperationException>(() => Retry.Run(context, action, _config));
			attempts.ShouldBe(5);
		}

		[Fact]
		public void For_contextual_when_it_succeeds_after_one_error()
		{
			var context = new Context();
			var attempts = 0;
			Action<Context> action = cx =>
			{
				attempts++;

				if (attempts <= 1)
					throw new InvalidOperationException();
			};

			Retry.Run(context, action, _config);
			attempts.ShouldBe(2);
		}

		[Fact]
		public void For_contextual_it_can_be_configured_in_multiple_ways()
		{
			var context = new Context();
			Action<Context> action = cx => { };
			var config = new RetryConfiguration();

			Retry.Run(context, action, config);
			Retry.Run(context, action, () => config);
			Retry.Run(context, action, c =>
			{
				c.MaxRetries = _config.MaxRetries;
			});
		}

		[Fact]
		public void For_contextual_when_creating_a_promise_config_is_created_instantly()
		{
			var actionRan = false;

			Action<Context> action = cx => actionRan = true;

			var promise = Retry.Create(action, _config);

			actionRan.ShouldBe(false);
		}

		[Fact]
		public void For_contextual_when_creating_a_promise_config_is_created_instantly_for_action_config()
		{
			var configRan = false;
			var actionRan = false;

			Action<RetryConfiguration> configure = config => configRan = true;
			Action<Context> action = cx => actionRan = true;

			var promise = Retry.Create(action, configure);

			configRan.ShouldBe(true);
			actionRan.ShouldBe(false);
		}

		[Fact]
		public void For_contextual_when_creating_a_promise_config_is_created_instantly_for_func_config()
		{
			var configRan = false;
			var actionRan = false;

			Func<RetryConfiguration> configure = () =>
			{
				configRan = true;
				return _config;
			};
			Action<Context> action = cx => actionRan = true;

			var promise = Retry.Create(action, configure);

			configRan.ShouldBe(true);
			actionRan.ShouldBe(false);
		}

		[Fact]
		public void For_contextual_when_creating_a_promise_version()
		{
			var result = "";
			var promise = Retry.Create<Context>(cx => result = cx.Name, _config);

			result.ShouldBe("");

			promise(new Context { Name = "A" });
			result.ShouldBe("A");

			promise(new Context { Name = "B" });
			result.ShouldBe("B");
		}

		private class Context
		{
			public string Name { get; set; }
		}
	}
}
