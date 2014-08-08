using System;
using MonoTouch.UIKit;

namespace Praeclarum.UI
{
	public class SelectableButtonItem
	{
		public UIBarButtonItem Item { get; private set; }

		UIButton button;

		public bool Selected {
			get {
				return button.Selected;
			}
			set {
				button.Selected = value;
			}
		}

		public SelectableButtonItem (UIImage image, EventHandler handler)
		{
			button = UIButton.FromType (UIButtonType.RoundedRect);
			button.SetImage (image, UIControlState.Normal);
			button.Frame = new System.Drawing.RectangleF (0, 0, 44, 44);
			button.TouchUpInside += handler;

			Item = new UIBarButtonItem (button);

		}
	}
}

