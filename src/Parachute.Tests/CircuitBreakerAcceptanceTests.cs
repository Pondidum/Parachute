using System;
using System.Collections.Generic;
using System.Linq;
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

		[Fact]
		public void Invoke()
		{
			Execute(new Dictionary<int, Action>
			{
				[0] = ShouldPass,
				[1] = ShouldFail,
				[2] = ShouldFailButCircuitBroken,
				[7] = ShouldFail,
				[8] = ShouldPassButCircuitBroken
			});
		}

		[Fact]
		public void When_a_call_succeeds()
		{
			Execute(new Dictionary<int, Action>
			{
				[0] = ShouldPass
			});
		}

		[Fact]
		public void When_the_cb_trips_timesout_and_then_succeeds()
		{
			Execute(new Dictionary<int, Action>
			{
				[0] = ShouldFail,
				[1] = ShouldFailButCircuitBroken,
				[5] = ShouldPassButCircuitBroken,
				[6] = ShouldPass,
			});
		}

		[Fact]
		public void When_the_cb_trips_and_doesnt_timeout_before_the_next_error()
		{
			Execute(new Dictionary<int, Action>
			{
				[0] = ShouldFail,
				[3] = ShouldPassButCircuitBroken,
				[4] = ShouldFailButCircuitBroken,
				[5] = ShouldPassButCircuitBroken,
				[6] = ShouldFail,
				[7] = ShouldPassButCircuitBroken,
			});
		}

		private void Execute(Dictionary<int, Action> timeline)
		{
			foreach (var pair in timeline.OrderBy(p => p.Key))
			{
				_timestamp = InitialStamp.AddSeconds(pair.Key);
				pair.Value();
			}
		}

		private void ShouldPass()
		{
			_result = "";
			_succeeds = true;
			_promise();
			_result.ShouldBe("PASS");
		}

		private void ShouldFail()
		{
			_result = "";
			_succeeds = false;
			Should.Throw<ArgumentException>(() => _promise());
			_result.ShouldBe("FAIL");
		}

		private void ShouldPassButCircuitBroken()
		{
			_result = "";
			_succeeds = true;
			Should.Throw<CircuitOpenException>(() => _promise());
			_result.ShouldBe("");
		}

		private void ShouldFailButCircuitBroken()
		{
			_result = "";
			_succeeds = false;
			Should.Throw<CircuitOpenException>(() => _promise());
			_result.ShouldBe("");
		}
	}
}
