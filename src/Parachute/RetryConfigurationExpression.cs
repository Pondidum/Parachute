using System;

namespace Parachute
{
	public class RetryConfigurationExpression
	{
		public TimeSpan Delay { get; set; }
		public int MaxRetries { get; set; }
	}
}
