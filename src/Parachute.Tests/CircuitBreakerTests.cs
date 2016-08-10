using System;
using Shouldly;
using Xunit;

namespace Parachute.Tests
{
	public class CircuitBreakerTests
	{
		private readonly Action _passAction;
		private readonly Action _failAction;
		private readonly CircuitBreakerConfig _config;

		public CircuitBreakerTests()
		{
			_passAction = () => { };
			_failAction = () => { throw new NotSupportedException(); };

			_config = new CircuitBreakerConfig
			{
				InitialState = CircuitBreakerStates.Closed,
			};
		}

		[Fact]
		public void When_in_closed_and_action_succeeds()
		{
			CircuitBreaker.Create(_passAction, _config)();

			_config.CurrentState.ShouldBe(CircuitBreakerStates.Closed);
		}

		[Fact]
		public void When_in_closed_and_action_fails_less_than_threashold()
		{
			_config.Threashold = 5;
			var promise = CircuitBreaker.Create(_passAction, _config);

			promise();

			_config.CurrentState.ShouldBe(CircuitBreakerStates.Closed);
		}

		[Fact]
		public void When_in_closed_and_aciton_fails_more_than_threashold()
		{
			_config.Threashold = 0;
			var promise = CircuitBreaker.Create(_failAction, _config);

			promise();

			_config.CurrentState.ShouldBe(CircuitBreakerStates.Open);
		}

		[Fact]
		public void When_in_open_and_timeout_has_not_elapsed()
		{
			_config.TimeoutCheckInterval = TimeSpan.Zero;
			_config.HasTimeoutExpired = elapsed => false;
			var promise = CircuitBreaker.Create(_passAction, _config);

			Should.Throw<CircuitOpenException>(() => promise());

			_config.CurrentState.ShouldBe(CircuitBreakerStates.Open);
		}

		[Fact]
		public void When_in_open_and_timeout_elapses()
		{
			_config.TimeoutCheckInterval = TimeSpan.Zero;
			_config.HasTimeoutExpired = elapsed => true;
			var promise = CircuitBreaker.Create(_passAction, _config);

			promise();

			_config.CurrentState.ShouldBe(CircuitBreakerStates.PartiallyOpen);
		}

		[Fact]
		public void When_in_partially_open_and_action_succeeds()
		{
			_config.InitialState = CircuitBreakerStates.PartiallyOpen;
			var promise = CircuitBreaker.Create(_passAction, _config);

			promise();

			_config.CurrentState.ShouldBe(CircuitBreakerStates.Closed);
		}

		[Fact]
		public void When_in_partially_open_and_action_fails()
		{
			_config.InitialState = CircuitBreakerStates.PartiallyOpen;
			var promise = CircuitBreaker.Create(_failAction, _config);

			Should.Throw<NotSupportedException>(() => promise());

			_config.CurrentState.ShouldBe(CircuitBreakerStates.Open);
		}
	}
}
