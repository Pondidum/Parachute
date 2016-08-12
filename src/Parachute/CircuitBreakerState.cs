namespace Parachute
{
	internal class CircuitBreakerState
	{
		private CircuitBreakerStates _currentState;

		public CircuitBreakerState()
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
