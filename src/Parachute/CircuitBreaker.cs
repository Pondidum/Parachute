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
}
