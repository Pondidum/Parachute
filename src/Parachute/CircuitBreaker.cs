using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Parachute
{
	public class CircuitBreaker
	{
		public static Action Create(Action action, Action<CircuitBreakerConfig> configure)
		{
			var config = new CircuitBreakerConfig();
			configure(config);

			return Create(action, config);
		}

		public static Action Create(Action action, Func<CircuitBreakerConfig> configure)
		{
			return Create(action, configure());
		}

		public static Action Create(Action action, CircuitBreakerConfig config)
		{
			Func<bool> wrapped = () =>
			{
				action();
				return true;
			};

			var promise = Create(wrapped, config);

			return () =>
			{
				promise();
			};
		}

		public static Func<T> Create<T>(Func<T> action, Action<CircuitBreakerConfig> configure)
		{
			var config = new CircuitBreakerConfig();
			configure(config);

			return Create(action, config);
		}

		public static Func<T> Create<T>(Func<T> action, Func<CircuitBreakerConfig> configure)
		{
			return Create(action, configure());
		}

		[Pure]
		public static Func<T> Create<T>(Func<T> action, CircuitBreakerConfig config)
		{
			var state = new CircuitBreakerState();
			var errorStamps = new List<DateTime>();

			return () =>
			{
				var now = config.GetTimestamp();
				var elapsed = errorStamps.Any() ? now.Subtract(errorStamps.Last()) : TimeSpan.Zero;

				if (state.IsOpen && elapsed > config.ResetTimeout)
					state.AttemptReset();

				if (state.IsOpen)
					throw new CircuitOpenException();

				try
				{
					var result = action();

					if (state.IsClosed)
						errorStamps.Clear();

					if (state.IsPartial)
						state.Reset();

					return result;
				}
				catch (Exception ex)
				{
					if (config.IgnoreExceptions.Contains(ex.GetType()) == false)
					{
						errorStamps.Add(now);

						var threasholdStamp = now.Subtract(config.ExceptionTimeout);
						var errorsInWindow = errorStamps.Count(stamp => stamp > threasholdStamp);

						if (errorsInWindow >= config.ExceptionThreashold || state.IsPartial)
							state.Trip();
					}

					throw;
				}
			};
		}
	}
}
