using System;
using Shouldly;
using Xunit;

namespace Parachute.Tests
{
	public class RetryTests
	{
		private readonly RetryConfigurationExpression _config;

		public RetryTests()
		{
			_config = new RetryConfigurationExpression
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
			var config = new RetryConfigurationExpression();

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

			Action<RetryConfigurationExpression> configure = config => configRan = true;
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

			Func<RetryConfigurationExpression> configure = () =>
			{
				configRan = true;
				return _config;
			};
			Action action = () => actionRan = true;

			var promise = Retry.Create(action, configure);

			configRan.ShouldBe(true);
			actionRan.ShouldBe(false);
		}
	}
}
