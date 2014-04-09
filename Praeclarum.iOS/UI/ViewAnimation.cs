using System;
using MonoTouch.UIKit;

namespace Praeclarum.UI
{
	public static class ViewAnimation
	{
		public static void Run (Action action, double duration, bool animated)
		{
			if (animated) {
				UIView.Animate (duration, () => action());
			} else {
				action ();
			}
		}
	}
}

