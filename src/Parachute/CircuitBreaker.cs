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
				catch (Exception)
				{
					errorStamps.Add(now);

					var threasholdStamp = now.Subtract(config.ExceptionTimeout);
					var errorsInWindow = errorStamps.Count(stamp => stamp > threasholdStamp);

					if (errorsInWindow >= config.ExceptionThreashold || state.IsPartial)
						state.Trip();

					throw;
				}
			};
		}

		private class CircuitState
		{
			private CircuitBreakerStates _currentState;

			public CircuitState()
			{
				_currentState = CircuitBreakerStates.Closed;
			}

			public bool IsOpen => _currentState == CircuitBreakerStates.Open;
			public bool IsClosed => _currentState == CircuitBreakerStates.Closed;
			public bool IsPartial => _currentState == CircuitBreakerStates.PartiallyOpen;

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

			private enum CircuitBreakerStates
			{
				Closed,
				PartiallyOpen,
				Open
			}
		}

	}
}
