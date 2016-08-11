﻿using System;
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

				if (config.CurrentState == CircuitBreakerStates.Closed)
				{
					try
					{
						action();
					}
					catch (Exception)
					{
						errorStamps.Add(DateTime.UtcNow);

						var threasholdStamp = DateTime.UtcNow.Subtract(config.ThreasholdWindow);
						var errorsInWindow = errorStamps.Count(stamp => stamp > threasholdStamp);

						if (errorsInWindow > config.Threashold)
							state.Trip();

						throw;
					}
				}
				else if (config.CurrentState == CircuitBreakerStates.Open)
				{
					var elapsed = errorStamps.Any() ? DateTime.UtcNow.Subtract(errorStamps.Last()) : TimeSpan.Zero;

					if (config.HasTimeoutExpired(elapsed))
					{
						try
						{
							action();
							state.AttemptReset();
						}
						catch (Exception)
						{
							errorStamps.Add(DateTime.UtcNow);
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
						state.Reset();
					}
					catch (Exception)
					{
						errorStamps.Add(DateTime.UtcNow);
						state.Trip();
						throw;
					}
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
