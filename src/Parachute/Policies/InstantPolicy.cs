using System;

namespace Parachute.Policies
{
	public class InstantPolicy : IPolicy
	{
		public TimeSpan Delay { get; set; }

		public InstantPolicy()
		{
			Delay = TimeSpan.Zero;
		}

		public TimeSpan GetDelay(int attempt)
		{
			return Delay;
		}
	}
}