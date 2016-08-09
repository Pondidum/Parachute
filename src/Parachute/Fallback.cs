using System;
using System.Linq;

namespace Parachute
{
	public class Fallback
	{
		public static void Run(params Action[] actions)
		{
			foreach (var action in actions)
			{
				try
				{
					action();
					return;
				}
				catch (Exception)
				{
					if (action == actions.Last())
						throw;
				}
			}
		}

		public static Action Create(params Action[] actions)
		{
			return () => Run(actions);
		}

		public static void Run<TContext>(TContext context, params Action<TContext>[] actions)
		{
			foreach (var action in actions)
			{
				try
				{
					action(context);
					return;
				}
				catch (Exception)
				{
					if (action == actions.Last())
						throw;
				}
			}
		}

		public static Action<TContext> Create<TContext>(params Action<TContext>[] actions)
		{
			return context => Run(context, actions);
		}
	}
}
