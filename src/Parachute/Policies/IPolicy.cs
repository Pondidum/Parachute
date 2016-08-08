using System;

namespace Parachute.Policies
{
	public interface IPolicy
	{
		TimeSpan GetDelay(int attempt);
	}
}