using System;

namespace Parachute
{
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
}