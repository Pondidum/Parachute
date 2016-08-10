using System;
using System.Threading;

namespace Parachute
{
	public class Retry
	{
		public static Action Create(Action action, Func<RetryConfiguration> configure)
		{
			var config = configure();
			return () => Run(action, config);
		}

		public static Action Create(Action action, Action<RetryConfiguration> configure)
		{
			var config = new RetryConfiguration();
			configure(config);

			return () => Run(action, config);
		}

		public static Action Create(Action action, RetryConfiguration config)
		{
			return () => Run(action, config);
		}

		public static void Run(Action action, Func<RetryConfiguration> configure)
		{
			Run(action, configure());
		}

		public static void Run(Action action, Action<RetryConfiguration> configure)
		{
			var config = new RetryConfiguration();
			configure(config);

			Run(action, config);
		}

		public static void Run(Action action, RetryConfiguration config)
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

		public static Action<TContext> Create<TContext>(Action<TContext> action, Func<RetryConfiguration> configure)
		{
			var config = configure();
			return context => Run(context, action, config);
		}

		public static Action<TContext> Create<TContext>(Action<TContext> action, Action<RetryConfiguration> configure)
		{
			var config = new RetryConfiguration();
			configure(config);

			return context => Run(context, action, config);
		}

		public static Action<TContext> Create<TContext>(Action<TContext> action, RetryConfiguration config)
		{
			return context => Run(context, action, config);
		}

		public static void Run<TContext>(TContext context, Action<TContext> action, Func<RetryConfiguration> configure)
		{
			Run(context, action, configure());
		}

		public static void Run<TContext>(TContext context, Action<TContext> action, Action<RetryConfiguration> configure)
		{
			var config = new RetryConfiguration();
			configure(config);

			Run(context, action, config);
		}

		public static void Run<TContext>(TContext context, Action<TContext> action, RetryConfiguration config)
		{
			var attempt = 0;

			while (attempt < config.MaxRetries)
			{
				try
				{
					action(context);
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
