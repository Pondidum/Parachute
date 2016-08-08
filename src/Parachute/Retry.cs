using System;

namespace Parachute
{
	public class Retry
	{
		public static void Run(Action action, Action<RetryConfigurationExpression> configure)
		{
			var config = new RetryConfigurationExpression();
			configure(config);

			var retries = 0;

			while (retries < config.MaxRetries)
			{
				try
				{
					action();
					return;
				}
				catch (Exception ex)
				{
					retries++;

					if (retries >= config.MaxRetries)
						throw;
				}
			}
		}
	}
}
