using System;

namespace Parachute.Policies
{
	public class ExponentialBackoffPolicy : IPolicy
	{
		public TimeSpan GetDelay(int attempt)
		{
			return TimeSpan.FromSeconds(Math.Pow(attempt, 2));
		}
	}
}