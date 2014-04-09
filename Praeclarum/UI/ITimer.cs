using System;

namespace Praeclarum
{
	public interface ITimer
	{
		event EventHandler Tick;
		bool Enabled { get; set; }
		TimeSpan Interval { get; set; }
	}
}

