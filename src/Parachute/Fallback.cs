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
	}
}
