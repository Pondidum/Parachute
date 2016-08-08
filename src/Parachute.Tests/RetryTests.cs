using System;
using Shouldly;
using Xunit;

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
	}
}
