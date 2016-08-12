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
			var state = new CircuitState(config.InitialState);
			config.ReadState = () => state.Current;

			var errorStamps = new List<DateTime>();

			return () =>
			{
				var now = config.GetTimestamp();
				var elapsed = errorStamps.Any() ? now.Subtract(errorStamps.Last()) : TimeSpan.Zero;

				if (state.Current == CircuitBreakerStates.Open && config.HasTimeoutExpired(elapsed))
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

					var threasholdStamp = now.Subtract(config.ThreasholdWindow);
					var errorsInWindow = errorStamps.Count(stamp => stamp > threasholdStamp);

					if (errorsInWindow > config.Threashold || state.Current == CircuitBreakerStates.PartiallyOpen)
						state.Trip();

					throw;
				}
			};
		}


		private class CircuitState
		{
			private CircuitBreakerStates _currentState;

			public CircuitState(CircuitBreakerStates initialState)
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
		internal Func<CircuitBreakerStates> ReadState { get; set; }
		public CircuitBreakerStates CurrentState => ReadState();
		public CircuitBreakerStates InitialState { get; set; }

		public int Threashold { get; set; }
		public Func<TimeSpan, bool> HasTimeoutExpired { get; set; }
		public TimeSpan ThreasholdWindow { get; set; }
		public Func<DateTime> GetTimestamp { get; set; }

		public CircuitBreakerConfig()
		{
			var timeout = TimeSpan.FromSeconds(5);
			HasTimeoutExpired = span => span > timeout;

			GetTimestamp = () => DateTime.UtcNow;
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
