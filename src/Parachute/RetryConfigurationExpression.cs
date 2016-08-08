using Parachute.Policies;

namespace Parachute
{
	public class RetryConfigurationExpression
	{
		public int MaxRetries { get; set; }
		public IPolicy Policy { get; set; }

		public RetryConfigurationExpression()
		{
			MaxRetries = 5;
			Policy = new InstantPolicy();
		}
	}
}
