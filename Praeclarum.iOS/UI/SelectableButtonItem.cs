#nullable enable

using System;
using UIKit;

namespace Praeclarum.UI
{
	public class SelectableButtonItem
	{
		public UIBarButtonItem Item { get; private set; }

		readonly UIButton? button = null;

		private bool selected;

		public bool Selected {
			get {
				return selected;
			}
			set
			{
				if (selected == value) return;
				selected = value;
				if (button is { } b)
				{
					b.Selected = selected;
				}
				else
				{
					if (ios15)
					{
						Item.Selected = selected;
					}
				}
			}
		}

		readonly static bool ios15 = UIDevice.CurrentDevice.CheckSystemVersion (15, 0);

		public SelectableButtonItem (UIImage image, EventHandler handler)
		{
			if (ios15) {
				Item = new UIBarButtonItem (image, UIBarButtonItemStyle.Plain, handler);
				Item.Selected = selected;
			}
			else {
				button = UIButton.FromType (UIButtonType.RoundedRect);
				button.SetImage (image, UIControlState.Normal);
				button.Frame = new CoreGraphics.CGRect (0, 0, 44, 44);
				button.TouchUpInside += handler;
				button.Selected = selected;
				Item = new UIBarButtonItem (button);
			}
		}
	}
}

