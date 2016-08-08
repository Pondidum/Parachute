using System;
using Shouldly;
using Xunit;
using Parachute;

namespace Parachute.Tests
{
	public class RetryTests
	{
		private readonly Action<RetryConfigurationExpression> _config;

		public RetryTests()
		{
			_config = config =>
			{
				config.Delay = TimeSpan.FromSeconds(5);
				config.MaxRetries = 5;
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
				c.Delay = TimeSpan.FromSeconds(1);
				c.MaxRetries = 5;
			});
		}
	}
}
