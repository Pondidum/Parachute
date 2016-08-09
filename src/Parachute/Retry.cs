using System;
using System.Threading;

namespace Parachute
{
	public class Retry
	{
		public static Action Create(Action action, Func<RetryConfigurationExpression> configure)
		{
			var config = configure();
			return () => Run(action, config);
		}

		public static Action Create(Action action, Action<RetryConfigurationExpression> configure)
		{
			var config = new RetryConfigurationExpression();
			configure(config);

			return () => Run(action, config);
		}

		public static Action Create(Action action, RetryConfigurationExpression config)
		{
			return () => Run(action, config);
		}

		public static void Run(Action action, Func<RetryConfigurationExpression> configure)
		{
			Run(action, configure());
		}

		public static void Run(Action action, Action<RetryConfigurationExpression> configure)
		{
			var config = new RetryConfigurationExpression();
			configure(config);

			Run(action, config);
		}

		public static void Run(Action action, RetryConfigurationExpression config)
		{
			var attempt = 0;

			while (attempt < config.MaxRetries)
			{
				try
				{
					action();
					return;
				}
				catch (Exception)
				{
					attempt++;

					if (attempt >= config.MaxRetries)
						throw;

					Thread.Sleep(config.Policy.GetDelay(attempt));
				}
			}
		}
	}
}
