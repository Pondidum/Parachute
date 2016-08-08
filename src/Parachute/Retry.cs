using System;

namespace Parachute
{
	public class Retry
	{
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
			var retries = 0;

			while (retries < config.MaxRetries)
			{
				try
				{
					action();
					return;
				}
				catch (Exception)
				{
					retries++;

					if (retries >= config.MaxRetries)
						throw;
				}
			}
		}
	}
}
