using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Parachute
{
	public class CircuitBreaker
	{
		[Pure]
		public static Action Create(Action action, CircuitBreakerConfig config)
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
					action();

					if (state.IsClosed)
						errorStamps.Clear();

					if (state.IsPartial)
						state.Reset();
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
