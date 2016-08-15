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

		private readonly List<Type> _filter;

		public CircuitBreakerAcceptanceTests()
		{
			_filter = new List<Type>();
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
				ExceptionTimeout = TimeSpan.FromSeconds(5),
				IgnoreExceptions = _filter
			});
		}

		[Fact]
		public void Invoke()
		{
			At(0, ShouldPass);
			At(1, ShouldFail);
			At(2, ShouldFailButCircuitBroken);
			At(7, ShouldFail);
			At(8, ShouldPassButCircuitBroken);
		}

		[Fact]
		public void When_a_call_succeeds()
		{
			At(0, ShouldPass);
		}

		[Fact]
		public void When_the_cb_trips_timesout_and_then_succeeds()
		{
			At(0, ShouldFail);
			At(1, ShouldFailButCircuitBroken);
			At(5, ShouldPassButCircuitBroken);
			At(6, ShouldPass);
		}

		[Fact]
		public void When_the_cb_trips_and_doesnt_timeout_before_the_next_error()
		{
			At(0, ShouldFail);
			At(3, ShouldPassButCircuitBroken);
			At(4, ShouldFailButCircuitBroken);
			At(5, ShouldPassButCircuitBroken);
			At(6, ShouldFail);
			At(7, ShouldPassButCircuitBroken);
		}

		[Fact]
		public void When_exceptions_are_filtered_out()
		{
			_filter.Add(typeof(ArgumentException));

			At(0, ShouldFail);
			At(1, ShouldFail);
		}

		private void At(int offset, Action action)
		{
			_timestamp = InitialStamp.AddSeconds(offset);
			action();
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
