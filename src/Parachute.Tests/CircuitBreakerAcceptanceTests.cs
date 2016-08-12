using System;
using Shouldly;
using Xunit;

namespace Parachute.Tests
{
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
				ExceptionThreashold = 1,
				ExceptionTimeout = TimeSpan.FromSeconds(5)
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
