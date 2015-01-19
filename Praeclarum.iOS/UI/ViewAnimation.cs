using System;
using UIKit;

namespace Praeclarum.UI
{
	public static class ViewAnimation
	{
		public static void Run (Action action, double duration, bool animated)
		{
			if (animated) {
				UIView.Animate (duration, () => {
					try {
						action();
					} catch (Exception ex) {
						Log.Error (ex);						
					}
				});
			} else {
				action ();
			}
		}
	}
}

