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
			config.CurrentState = config.InitialState;
			DateTime? lastError = null;
			var errorStamps = new List<DateTime>();

			return () =>
			{

				if (config.CurrentState == CircuitBreakerStates.Closed)
				{
					try
					{
						action();
					}
					catch (Exception)
					{
						errorStamps.Add(DateTime.UtcNow);
						lastError = DateTime.UtcNow;

						var threasholdStamp = DateTime.UtcNow.Subtract(config.ThreasholdWindow);
						var errorsInWindow = errorStamps.Count(stamp => stamp > threasholdStamp);

						if (errorsInWindow > config.Threashold)
							config.CurrentState = CircuitBreakerStates.Open;

						throw;
					}
				}
				else if (config.CurrentState == CircuitBreakerStates.Open)
				{
					var elapsed = lastError.HasValue ? DateTime.UtcNow.Subtract(lastError.Value) : TimeSpan.Zero;

					if (config.HasTimeoutExpired(elapsed))
					{
						try
						{
							action();
							config.CurrentState = CircuitBreakerStates.PartiallyOpen;
						}
						catch (Exception)
						{
							lastError = DateTime.UtcNow;
							throw;
						}
					}
					else
					{
						throw new CircuitOpenException();
					}
				}
				else if (config.CurrentState == CircuitBreakerStates.PartiallyOpen)
				{
					try
					{
						action();
						config.CurrentState = CircuitBreakerStates.Closed;
					}
					catch (Exception)
					{
						lastError = DateTime.UtcNow;
						config.CurrentState = CircuitBreakerStates.Open;
						throw;
					}
				}
			};
		}
	}

	public class CircuitBreakerConfig
	{
		public CircuitBreakerStates InitialState { get; set; }
		public CircuitBreakerStates CurrentState { get; internal set; }
		public int Threashold { get; set; }
		public Func<TimeSpan, bool> HasTimeoutExpired { get; set; }
		public TimeSpan ThreasholdWindow { get; set; }

		public CircuitBreakerConfig()
		{
			var timeout = TimeSpan.FromSeconds(5);
			HasTimeoutExpired = span => span > timeout;

			ThreasholdWindow = TimeSpan.FromSeconds(2);
		}
	}

	public enum CircuitBreakerStates
	{
		Closed,
		PartiallyOpen,
		Open
	}

	public class CircuitOpenException : Exception
	{
	}
}
