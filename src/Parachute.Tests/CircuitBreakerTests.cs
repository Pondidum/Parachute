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
		private string _result;

		public CircuitBreakerTests()
		{
			_result = "";

			_passAction = () =>
			{
				_result = "PASS";
			};

			_failAction = () =>
			{
				_result = "FAIL";
				throw new NotSupportedException();
			};

			_config = new CircuitBreakerConfig
			{
				InitialState = CircuitBreakerStates.Closed,
			};
		}

		[Fact]
		public void When_in_closed_and_action_succeeds()
		{
			var promise = CircuitBreaker.Create(_passAction, _config);
			promise();

			_result.ShouldBe("PASS");
			_config.CurrentState.ShouldBe(CircuitBreakerStates.Closed);
		}

		[Fact]
		public void When_in_closed_and_action_fails_less_than_threashold()
		{
			_config.Threashold = 5;
			var promise = CircuitBreaker.Create(_failAction, _config);

			Should.Throw<NotSupportedException>(() => promise());

			_result.ShouldBe("FAIL");
			_config.CurrentState.ShouldBe(CircuitBreakerStates.Closed);
		}

		[Fact]
		public void When_in_closed_and_action_fails_more_than_threashold()
		{
			_config.Threashold = 0;
			var promise = CircuitBreaker.Create(_failAction, _config);

			Should.Throw<NotSupportedException>(() => promise());

			_result.ShouldBe("FAIL");
			_config.CurrentState.ShouldBe(CircuitBreakerStates.Open);
		}

		[Fact]
		public void When_in_open_and_timeout_has_not_elapsed()
		{
			//_config.HasTimeoutExpired = elapsed => false;
			_config.InitialState = CircuitBreakerStates.Open;

			var promise = CircuitBreaker.Create(_passAction, _config);

			Should.Throw<CircuitOpenException>(() => promise());

			_result.ShouldBe("");
			_config.CurrentState.ShouldBe(CircuitBreakerStates.Open);
		}

		[Fact]
		public void When_in_partially_open_and_action_succeeds()
		{
			_config.InitialState = CircuitBreakerStates.PartiallyOpen;
			var promise = CircuitBreaker.Create(_passAction, _config);

			promise();

			_result.ShouldBe("PASS");
			_config.CurrentState.ShouldBe(CircuitBreakerStates.Closed);
		}

		[Fact]
		public void When_in_partially_open_and_action_fails()
		{
			_config.InitialState = CircuitBreakerStates.PartiallyOpen;
			var promise = CircuitBreaker.Create(_failAction, _config);

			Should.Throw<NotSupportedException>(() => promise());

			_result.ShouldBe("FAIL");
			_config.CurrentState.ShouldBe(CircuitBreakerStates.Open);
		}

	}

	public class CircuitBreakerAcceptanceTests
	{
		private static readonly DateTime InitialStamp = new DateTime(2016, 08, 12, 15, 00, 00);

		private bool _succeeds;
		private DateTime _timestamp;
		private string _result;
		private Action _promise;

		public CircuitBreakerAcceptanceTests()
		{
			_succeeds = true;
			_timestamp = InitialStamp;

			Action controllableAction = () =>
			{
				_result = _succeeds ? "PASS" : "FAIL";
				if (_succeeds == false) throw new ArgumentException();
			};

			_promise = CircuitBreaker.Create(controllableAction, new CircuitBreakerConfig
			{
				GetTimestamp = () => _timestamp,
				Threashold = 1,
				ThreasholdWindow = TimeSpan.FromSeconds(5)
			});
		}

		private void Next(int offset) => _timestamp = InitialStamp.AddSeconds(offset);

		private void SuccessfulCall(int offset)
		{
			Next(offset);

			_succeeds = true;
			_result = "";

			_promise();

			_result.ShouldBe("PASS");
		}

		private void SuccessfulCallCircuitException(int offset)
		{
			Next(offset);

			_succeeds = true;
			_result = "";

			Should.Throw<CircuitOpenException>(() => _promise());
			_result.ShouldBe("");
		}

		private void FailingCallException(int offset)
		{
			Next(offset);

			_succeeds = false;
			_result = "";

			Should.Throw<ArgumentException>(() => _promise());
			_result.ShouldBe("FAIL");
		}

		private void FailingCallCircuitException(int offset)
		{
			Next(offset);

			_succeeds = false;
			_result = "";

			Should.Throw<CircuitOpenException>(() => _promise());
			_result.ShouldBe("");
		}

		[Fact]
		public void Invoke()
		{
			SuccessfulCall(0);
			FailingCallException(1);
			FailingCallCircuitException(2);

			FailingCallException(7);
			SuccessfulCallCircuitException(8);
		}
	}
}
