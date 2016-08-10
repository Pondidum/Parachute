using Parachute.Policies;

namespace Parachute
{
	public class RetryConfiguration
	{
		public int MaxRetries { get; set; }
		public IPolicy Policy { get; set; }

		public RetryConfiguration()
		{
			MaxRetries = 5;
			Policy = new InstantPolicy();
		}
	}
}
