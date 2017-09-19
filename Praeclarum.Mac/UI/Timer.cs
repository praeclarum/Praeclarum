using System;
using Foundation;

namespace Praeclarum.UI
{
	public class Timer : ITimer
	{
		public event EventHandler Tick;

		NSTimer timer;

		public bool Enabled {
			get { return timer != null; }
			set {
				if (value) {
					if (timer != null)
						return;
					try {
						timer = NSTimer.CreateRepeatingScheduledTimer (
							Interval, NSTimerTick);
					} catch (Exception) {						
					}
				} else {
					if (timer == null)
						return;
					try {
						timer.Invalidate ();
					} catch (Exception) {
					}
					finally {
						timer = null;
					}
				}
			}
		}

		TimeSpan interval;

		public TimeSpan Interval {
			get {
				return interval;
			}
			set {
				if (interval == value)
					return;
				interval = value;
				var en = Enabled;
				Enabled = false;
				Enabled = en;
			}
		}

		public Timer ()
		{
			interval = TimeSpan.FromSeconds (1);
		}

		void NSTimerTick (NSTimer t)
		{
			var ev = Tick;
			if (ev != null) {
				ev (this, EventArgs.Empty);
			}
		}
	}
}

