using System;
using MonoTouch.UIKit;

namespace Praeclarum.UI
{
	public class SelectableButtonItem
	{
		public UIBarButtonItem Item { get; private set; }

		readonly UIButton button;

		public bool Selected {
			get {
				return button.Selected;
			}
			set {
				button.Selected = value;
			}
		}

		readonly static bool ios7 = UIDevice.CurrentDevice.CheckSystemVersion (7, 0);

		public SelectableButtonItem (UIImage image, EventHandler handler)
		{
			if (ios7) {
				button = UIButton.FromType (UIButtonType.RoundedRect);
				button.SetImage (image, UIControlState.Normal);
				button.Frame = new System.Drawing.RectangleF (0, 0, 44, 44);
				button.TouchUpInside += handler;
				Item = new UIBarButtonItem (button);
			} else {
				Item = new UIBarButtonItem (image, UIBarButtonItemStyle.Plain, handler);
				button = UIButton.FromType (UIButtonType.Custom);
			}
		}
	}
}

