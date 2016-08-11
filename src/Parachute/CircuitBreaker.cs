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
			var cb = new CircuitBreaker(action, config);

			return cb.Invoke;
		}

		private readonly CircuitState _state;
		private readonly List<DateTime> _errorStamps;
		private readonly Action _action;
		private readonly CircuitBreakerConfig _config;

		private CircuitBreaker(Action action, CircuitBreakerConfig config)
		{
			_action = action;
			_config = config;
			_state = new CircuitState(config.InitialState);
			config.ReadState = () => _state.Current;

			_errorStamps = new List<DateTime>();
		}

		private bool IsTripped()
		{
			var elapsed = _errorStamps.Any() ? DateTime.UtcNow.Subtract(_errorStamps.Last()) : TimeSpan.Zero;

			if (_state.Current == CircuitBreakerStates.Open && _config.HasTimeoutExpired(elapsed))
				_state.AttemptReset();

			return _state.Current == CircuitBreakerStates.Open;
		}

		public void Invoke()
		{
			if (IsTripped())
				throw new CircuitOpenException();

			try
			{
				_action();

				if (_state.Current == CircuitBreakerStates.Closed)
					_errorStamps.Clear();

				if (_state.Current == CircuitBreakerStates.PartiallyOpen)
					_state.Reset();
			}
			catch (Exception)
			{
				_errorStamps.Add(DateTime.UtcNow);

				var threasholdStamp = DateTime.UtcNow.Subtract(_config.ThreasholdWindow);
				var errorsInWindow = _errorStamps.Count(stamp => stamp > threasholdStamp);

				if (errorsInWindow > _config.Threashold || _state.Current == CircuitBreakerStates.PartiallyOpen)
					_state.Trip();

				throw;
			}
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
