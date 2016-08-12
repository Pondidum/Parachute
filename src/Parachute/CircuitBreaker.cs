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
			var state = new CircuitState();

			var errorStamps = new List<DateTime>();

			return () =>
			{
				var now = config.GetTimestamp();
				var elapsed = errorStamps.Any() ? now.Subtract(errorStamps.Last()) : TimeSpan.Zero;

				if (state.Current == CircuitBreakerStates.Open && elapsed > config.ResetTimeout)
					state.AttemptReset();

				if (state.Current == CircuitBreakerStates.Open)
					throw new CircuitOpenException();

				try
				{
					action();

					if (state.Current == CircuitBreakerStates.Closed)
						errorStamps.Clear();

					if (state.Current == CircuitBreakerStates.PartiallyOpen)
						state.Reset();
				}
				catch (Exception)
				{
					errorStamps.Add(now);

					var threasholdStamp = now.Subtract(config.ExceptionTimeout);
					var errorsInWindow = errorStamps.Count(stamp => stamp > threasholdStamp);

					if (errorsInWindow >= config.ExceptionThreashold || state.Current == CircuitBreakerStates.PartiallyOpen)
						state.Trip();

					throw;
				}
			};
		}


		private class CircuitState
		{
			private CircuitBreakerStates _currentState;

			public CircuitState(CircuitBreakerStates initialState = CircuitBreakerStates.Closed)
			{
				_currentState = initialState;
			}

			public CircuitBreakerStates Current => _currentState;

			public void Trip()
			{
				if (_currentState == CircuitBreakerStates.Closed || _currentState == CircuitBreakerStates.PartiallyOpen)
					_currentState = CircuitBreakerStates.Open;
			}

			public void AttemptReset()
			{
				if (_currentState == CircuitBreakerStates.Open)
					_currentState = CircuitBreakerStates.PartiallyOpen;
			}

			public void Reset()
			{
				if (_currentState == CircuitBreakerStates.Open || _currentState == CircuitBreakerStates.PartiallyOpen)
					_currentState = CircuitBreakerStates.Closed;
			}
		}

	}

	public class CircuitBreakerConfig
	{
		/// <summary>The number of Exceptions needed to trip the breaker</summary>
		public int ExceptionThreashold { get; set; }

		/// <summary> The window in which the number of <see cref="ExceptionThreashold"/> must occur in</summary>
		public TimeSpan ExceptionTimeout { get; set; }

		/// <summary>The amount of time after tripping before attempting to reset the breaker</summary>
		public TimeSpan ResetTimeout { get; set; }
		public Func<DateTime> GetTimestamp { get; set; }

		public CircuitBreakerConfig()
		{
			ResetTimeout = TimeSpan.FromSeconds(5);

			GetTimestamp = () => DateTime.UtcNow;
			ExceptionTimeout = TimeSpan.FromSeconds(2);
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
