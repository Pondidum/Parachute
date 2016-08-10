using System;
using System.Diagnostics.Contracts;

namespace Parachute
{
	public class CircuitBreaker
	{
		[Pure]
		public static Action Create(Action action, CircuitBreakerConfig config)
		{
			return () =>
			{
			};
		}
	}

	public class CircuitBreakerConfig
	{
		public CircuitBreakerStates InitialState { get; set; }
		public CircuitBreakerStates CurrentState { get; internal set; }
		public int Threashold { get; set; }
		public TimeSpan TimeoutCheckInterval { get; set; }
		public Func<TimeSpan, bool> HasTimeoutExpired { get; set; }
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
