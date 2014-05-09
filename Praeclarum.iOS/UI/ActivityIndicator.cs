using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace Praeclarum.UI
{
	public class ActivityIndicator : UIView
	{
		readonly UILabel titleLabel;
		readonly UIActivityIndicatorView activity;

		public string Title {
			get { return titleLabel.Text; }
			set { titleLabel.Text = value; }
		}

		public ActivityIndicator ()
		{
			Opaque = false;

			var bounds = new RectangleF (0, 0, 150, 44);
			Frame = bounds;

			BackgroundColor = UIColor.FromWhiteAlpha (0.96f, 0.85f);
			Layer.CornerRadius = 12;

			const float margin = 12;

			activity = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.Gray) {
				Frame = new RectangleF (margin, margin, 21, 21),
				HidesWhenStopped = false,
			};

			titleLabel = new UILabel (new RectangleF (activity.Frame.Right+margin, 0, bounds.Width - activity.Frame.Right - 2*margin, 44)) {
				TextAlignment = UITextAlignment.Center,
				TextColor = UIColor.FromWhiteAlpha (0.33f, 1),
				BackgroundColor = UIColor.Clear,
			};

			AddSubviews (titleLabel, activity);
		}

		public void Show (bool animated = true)
		{
			Hidden = false;
			activity.StartAnimating ();
			if (animated)
				UIView.Animate (1, () => Alpha = 1);
			else
				Alpha = 1;
		}

		public void Hide (bool animated = true)
		{
			activity.StopAnimating ();
			if (animated)
				UIView.Animate (0.25, () => Alpha = 0, () => Hidden = true);
			else {
				Alpha = 0;
				Hidden = true;
			}
		}
	}
}
