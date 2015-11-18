using System;
using UIKit;
using Foundation;
using CoreGraphics;
using System.Collections.Generic;
using CoreAnimation;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Praeclarum.UI;
using Praeclarum;
using Praeclarum.IO;

namespace Praeclarum.UI
{
	public class TitleView : UILabel
	{
//		UITapGestureRecognizer titleTap;

		public event EventHandler Tapped = delegate {};

		public TitleView ()
		{
			var isPhone = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone;
			var titleX = isPhone ? (320 - 176) / 2 : (768 - 624) / 2;

			Frame = new CGRect (titleX + 1, 6, isPhone ? 176 : 624, 30);
			BackgroundColor = UIColor.Clear;
			Opaque = false;
			TextColor = DocumentAppDelegate.Shared.Theme.NavigationTextColor;
//			ShadowColor = WhiteTheme.BarTextShadowColor;
//			ShadowOffset = new SizeF (WhiteTheme.BarTextShadowOffset.Horizontal, WhiteTheme.BarTextShadowOffset.Vertical);
			Font = UIFont.FromName (WhiteTheme.TitleFontName, WhiteTheme.BarTitleFontSize);
			AdjustsFontSizeToFitWidth = true;
			TextAlignment = UITextAlignment.Center;
			UserInteractionEnabled = true;

			AddGestureRecognizer (new UITapGestureRecognizer (HandleTitleTap));
		}

		void HandleTitleTap (UITapGestureRecognizer pan)
		{
			Tapped (this, EventArgs.Empty);
		}
	}
}

