using System;
using UIKit;
using CoreGraphics;
using System.Collections.Generic;

namespace Praeclarum.UI
{
	public partial class Theme
	{
		static protected readonly bool ios7 = UIDevice.CurrentDevice.CheckSystemVersion (7, 0);
		static protected readonly bool ios13 = UIDevice.CurrentDevice.CheckSystemVersion (13, 0);

		readonly Lazy<UIImage> CloneButtonImage = new Lazy<UIImage> (
			() => UIImage.FromBundle ("Clone.png"));

		readonly Lazy<UIImage> MoveButtonImage = new Lazy<UIImage> (
			() => UIImage.FromBundle ("Move.png"));

#if NET6_0_OR_GREATER
		public static UIColor SystemDarkTextColor => UIColor.DarkText;
		public static UIColor SystemLightTextColor => UIColor.LightText;
		public static UIColor SystemLabelColor => UIColor.Label;
		public static UIColor SystemGroupTableViewBackgroundColor => UIColor.GroupTableViewBackground;
		public static UIColor SystemGroupedBackgroundColor => UIColor.SystemGroupedBackground;
#else
		public static UIColor SystemDarkTextColor => UIColor.DarkText;
		public static UIColor SystemLightTextColor => UIColor.LightText;
		public static UIColor SystemLabelColor => UIColor.Label;
		public static UIColor SystemGroupTableViewBackgroundColor => UIColor.GroupTableViewBackgroundColor;
		public static UIColor SystemGroupedBackgroundColor => UIColor.SystemGroupedBackground;
#endif

		public static Theme Current;

		static Theme ()
		{
			Current = new LightTheme ();
		}

		public Theme ()
			: this (dark: false)
		{
		}

		public Theme (bool dark)
		{
			IsDark = dark;
			TintColor = UIColor.Red;
			var appdel = DocumentAppDelegate.Shared;

			if (dark) {
				if (appdel != null && appdel.App != null)
				{
					TintColor = Praeclarum.Graphics.ColorEx.GetUIColor(appdel.App.VibrantTintColor);
				}
				StatusBarStyle = UIStatusBarStyle.LightContent;
				PopoverBackgroundColor = UIColor.Black;
				NavigationBackgroundColor = null;
				NavigationTextColor = UIColor.White;
				DocumentsBackgroundColor = UIColor.FromRGB (42, 42, 42);
				DocumentsFolderColor = UIColor.FromRGB(92, 92, 92);
				DocumentsFrameSideColor = UIColor.Black;
				DocumentsFrameBottomColor = UIColor.Black;
				DocumentsControlColor = UIColor.Gray;
				NavigationBarStyle = UIBarStyle.Black;
				TableHeadingTextColor = UIColor.Gray;
				GroupedTableBackgroundColor = UIColor.FromRGB(42, 42, 42);
				PlainTableBackgroundColor = UIColor.FromRGB (32, 32, 32);
				TableCellBackgroundColor = UIColor.FromRGB(64, 64, 64);
				TableCellTextColor = UIColor.White;
				TableCellDetailTextColor = UIColor.LightGray;
				TableSeparatorColor = UIColor.FromWhiteAlpha (0.25f, 1.0f);
				DocumentBackgroundColor = UIColor.FromRGB(0, 0, 0);
				DocumentTextColor = SystemLightTextColor;
				KeyboardAppearance = UIKeyboardAppearance.Dark;
			} else {
				if (appdel != null && appdel.App != null)
				{
					TintColor = Praeclarum.Graphics.ColorEx.GetUIColor(appdel.App.TintColor);
				}
				StatusBarStyle = UIStatusBarStyle.Default;
				PopoverBackgroundColor = null;
				NavigationBarStyle = UIBarStyle.Default;
				TableHeadingTextColor = UIColor.Gray;
				NavigationBackgroundColor = null;
				NavigationTextColor = UIColor.Black;
				DocumentsBackgroundColor = UIColor.FromRGB (222, 222, 222);
				DocumentsControlColor = UIColor.FromWhiteAlpha (59 / 255.0f, 1);
				DocumentsFolderColor = UIColor.FromRGB (195, 195, 195);
				DocumentsFrameSideColor = UIColor.FromRGB ((nfloat)202 / 255.0f, (nfloat)202 / 255.0f, (nfloat)202 / 255.0f);
				DocumentsFrameBottomColor = UIColor.FromRGB ((nfloat)176 / 255.0f, (nfloat)176 / 255.0f, (nfloat)176 / 255.0f);
				GroupedTableBackgroundColor = SystemGroupTableViewBackgroundColor;
				PlainTableBackgroundColor = UIColor.White;
				TableCellBackgroundColor = UIColor.White;
				TableCellTextColor = SystemDarkTextColor;
				TableCellDetailTextColor = UIColor.LightGray;
				TableSeparatorColor = UIColor.FromWhiteAlpha (0.85f, 1.0f);
				DocumentBackgroundColor = UIColor.White;
				DocumentTextColor = SystemDarkTextColor;
				KeyboardAppearance = UIKeyboardAppearance.Default;
			}
		}

		public virtual void Apply ()
		{
			if (ios7) {
				UINavigationBar.Appearance.TintColor = TintColor;
				UIBarButtonItem.Appearance.TintColor = TintColor;

				UIPopoverBackgroundView.Appearance.BackgroundColor = NavigationBackgroundColor;

				UINavigationBar.Appearance.BarTintColor = NavigationBackgroundColor;
				UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes {
					ForegroundColor = NavigationTextColor,
				};
				UIToolbar.Appearance.BarTintColor = NavigationBackgroundColor;

				if (!ios13) {
					UITableViewHeaderFooterView.Appearance.BackgroundColor = GroupedTableBackgroundColor;
				}

				foreach (var w in UIApplication.SharedApplication.Windows) {
					ApplyToVC (w.RootViewController, new HashSet<IntPtr> ());
				}
			}
		}

		public virtual void Apply (UINavigationController nc)
		{
			if (nc == null)
				return;
			Apply (nc.NavigationBar);
			Apply (nc.Toolbar);

			var pop = nc.PopoverPresentationController;
			if (pop != null && IsDark) {
				pop.BackgroundColor = UIColor.Black;
			}
		}

		public virtual void Apply (UINavigationBar navigationBar)
		{
			if (navigationBar == null)
				return;
			navigationBar.BarStyle = NavigationBarStyle;
			if (ios7) {
				navigationBar.BarTintColor = NavigationBackgroundColor;
				navigationBar.TitleTextAttributes = new UIStringAttributes {
					ForegroundColor = NavigationTextColor,
				};
			}
		}

		public virtual void Apply (UIToolbar toolbar)
		{
			if (toolbar == null)
				return;
			toolbar.BarStyle = NavigationBarStyle;
			if (ios7) {
				toolbar.BarTintColor = NavigationBackgroundColor;
			}
		}

		public virtual void Apply (UITextField textField)
		{
			if (textField == null)
				return;
			textField.KeyboardAppearance = KeyboardAppearance;
			textField.BackgroundColor = TableCellBackgroundColor;
			textField.TextColor = TableCellTextColor;
		}

		void ApplyToVC (UIViewController vc, HashSet<IntPtr> visited)
		{
			if (vc == null)
				return;

			if (visited.Contains (vc.Handle))
				return;

			visited.Add (vc.Handle);

			try {
				var nc = vc as UINavigationController;
				if (nc != null) {
					Apply (nc);
				}

				if (ios7) {
					vc.SetNeedsStatusBarAppearanceUpdate ();
				}

				var ta = vc as IThemeAware;
				if (ta != null) {
					ta.ApplyTheme (this);
				}

			} catch (Exception ex) {
				Log.Error (ex);
			}

			try {
				ApplyToVC (vc.PresentedViewController, visited);
			} catch (Exception ex) {
				Log.Error (ex);
			}

			try {
				foreach (var c in (vc.ChildViewControllers??new UIViewController[0])) {
					ApplyToVC (c, visited);
				}
			} catch (Exception ex) {
				Log.Error (ex);
			}

			try {
				ApplyToV (vc.View, new HashSet<IntPtr> ());
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		void ApplyToV (UIView view, HashSet<IntPtr> visited)
		{
			if (view == null)
				return;
			if (visited.Contains (view.Handle))
				return;
			visited.Add (view.Handle);

			try {

				var tv = view as UITableView;
				if (tv != null) {
					Apply (tv);
				}

				var ta = view as IThemeAware;
				if (ta != null) {
					ta.ApplyTheme (this);
				}

			} catch (Exception ex) {
				Log.Error (ex);
			}

			try {
				foreach (var c in (view.Subviews??new UIView[0])) {
					ApplyToV (c, visited);
				}
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public UIStatusBarStyle StatusBarStyle { get; protected set; }
		public UIBarStyle NavigationBarStyle { get; protected set; }
		public bool IsDark { get; protected set; }
		public UIColor TableHeadingTextColor  { get; protected set; }
		public UIColor PopoverBackgroundColor { get; protected set; }
		public UIColor NavigationBackgroundColor  { get; protected set; }
		public UIColor NavigationTextColor  { get; protected set; }
		public UIColor DocumentsBackgroundColor  { get; protected set; }
		public UIColor DocumentsControlColor  { get; protected set; }
		public UIColor DocumentsFolderColor  { get; protected set; }
		public UIColor DocumentsFrameSideColor  { get; protected set; }
		public UIColor DocumentsFrameBottomColor  { get; protected set; }
		public UIColor GroupedTableBackgroundColor  { get; protected set; }
		public UIColor PlainTableBackgroundColor { get; protected set; }
		public UIColor TableCellBackgroundColor  { get; protected set; }
		public UIColor TableCellTextColor  { get; protected set; }
		public UIColor TableCellDetailTextColor  { get; protected set; }
		public UIColor TableSeparatorColor  { get; protected set; }
		public UIColor DocumentBackgroundColor { get; protected set; }
		public UIColor DocumentTextColor { get; protected set; }
		public UIKeyboardAppearance KeyboardAppearance { get; protected set; }
		public UIColor TintColor { get; protected set; }

		public virtual void Apply (UITableView tableView)
		{
			var c = tableView.Style == UITableViewStyle.Grouped ? GroupedTableBackgroundColor : PlainTableBackgroundColor;
			if (ios13) {
				c = SystemGroupedBackgroundColor;
			}
			if (tableView.BackgroundView == null) {
				tableView.BackgroundView = new UIView { BackgroundColor = c };
			} else {
				tableView.BackgroundView.BackgroundColor = c;
			}
			tableView.BackgroundColor = c;
			tableView.SeparatorColor = TableSeparatorColor;
		}

		public virtual void Apply (UITableViewCell cell)
		{
			cell.BackgroundColor = TableCellBackgroundColor;
			cell.TextLabel.TextColor = TableCellTextColor;
			var d = cell.DetailTextLabel;
			if (d != null) {
				d.TextColor = TableCellDetailTextColor;
			}
		}

		public virtual void ApplyCommand (UITableViewCell cell)
		{
			var isnav = cell.Accessory == UITableViewCellAccessory.DisclosureIndicator;
			cell.TextLabel.TextColor = ios7 ? 
				(isnav ? TableCellTextColor : TintColor) : 
				TableCellTextColor;
		}

		readonly UIColor checkColor = UIColor.FromRGB (50, 79, 133);

		public virtual void ApplyChecked (UITableViewCell cell)
		{
			cell.TextLabel.TextColor = ios7 ? 
				TintColor : checkColor;
		}

		public virtual UIBarButtonItem CreateAddButton (EventHandler handler)
		{
			return new UIBarButtonItem (UIBarButtonSystemItem.Add, handler);
		}

		public virtual UIBarButtonItem CreateEditButton (EventHandler handler)
		{
			return new UIBarButtonItem (UIBarButtonSystemItem.Edit, handler);
		}

		public virtual UIBarButtonItem CreateActionButton (EventHandler handler)
		{
			return new UIBarButtonItem (UIBarButtonSystemItem.Action, handler);
		}

		public virtual UIBarButtonItem CreateDeleteButton (EventHandler handler)
		{
			return new UIBarButtonItem (UIBarButtonSystemItem.Trash, handler);
		}

		public virtual UIBarButtonItem CreateCancelButton (EventHandler handler)
		{
			return new UIBarButtonItem (UIBarButtonSystemItem.Cancel, handler);
		}

		public virtual UIBarButtonItem CreateDuplicateButton (EventHandler handler)
		{
			return CreateBarButton (CloneButtonImage.Value, handler, UIBarButtonSystemItem.Add);
		}

		public virtual UIBarButtonItem CreateMoveButton (EventHandler handler)
		{
			return CreateBarButton (MoveButtonImage.Value, handler);
		}

		public virtual UIBarButtonItem CreateBarButton (UIImage image, EventHandler handler, UIBarButtonSystemItem fallback = UIBarButtonSystemItem.Add)
		{
			if (image == null)
				return new UIBarButtonItem (fallback, handler);

			return new UIBarButtonItem (image, UIBarButtonItemStyle.Plain, handler);
		}
	}

	public class LightTheme : Theme
	{
	}

	public class DarkTheme : Theme
	{
		public DarkTheme ()
			: base (dark: true)
		{			
		}
	}

	public class WhiteTheme : Theme
	{
		public static UIColor PaperColor = UIColor.White;

		public static UIColor ValueTextColor = UIColor.FromRGB (56, 84, 135);

		public static UIColor BarTintColor = UIColor.Green;// UIColor.FromWhiteAlpha (0.95f, 1);
		public static UIColor BarTextColor {
			get {
				return IsModern ? UIColor.Black : UIColor.FromWhiteAlpha (0.5f, 1);
			}
		}
		public static UIColor BarTextShadowColor  {
			get {
				return IsModern ? UIColor.Clear : UIColor.White;
			}
		}
		public static UIOffset BarTextShadowOffset = new UIOffset (0, 1);
		public static string BarButtonFontName = "HelveticaNeue";
		public static float BarButtonFontSize = 14;
		public static UIColor BarButtonTextShadowColor = UIColor.Clear;
		public static UIOffset BarButtonTextShadowOffset = new UIOffset (0, 0);

		public static float BarTitleFontSize {
			get {
				return IsModern ? 18 : 20;
			}
		}

		public static string TitleFontName {
			get {
				return IsModern ? UIFont.PreferredHeadline.Name :  "HelveticaNeue-Light";
			}
		}

		static string tableFontName = null;
		static string tableHeavyFontName = null;
		static string userTableFontName = null;

		static WhiteTheme ()
		{
			BackButtonImage = new Lazy<UIImage> (
				() => UIImage.FromBundle ("BackButton.png").CreateResizableImage (new UIEdgeInsets (0, 16, 0, 8)));
			ButtonImage = new Lazy<UIImage> (
				() => UIImage.FromBundle ("BarButton.png").CreateResizableImage (new UIEdgeInsets (8, 8, 8, 8)));
			HudButtonImage = new Lazy<UIImage> (
				() => UIImage.FromBundle ("HudButton.png").CreateResizableImage (new UIEdgeInsets (8, 8, 8, 8)));
			ActionButtonImage = new Lazy<UIImage> (
				() => UIImage.FromBundle ("Action.png"));
			AddButtonImage = new Lazy<UIImage> (
				() => UIImage.FromBundle ("Add.png"));
			DeleteButtonImage = new Lazy<UIImage> (
				() => UIImage.FromBundle ("Delete.png"));
			CloneButtonImage = new Lazy<UIImage> (
				() => UIImage.FromBundle ("Clone.png"));
		}

		public static string TableFontName {
			get {
				if (tableFontName == null) {
					tableFontName = "HelveticaNeue-Light";
				}
				return tableFontName;
			}
		}
		public static string TableHeavyFontName {
			get {
				if (tableHeavyFontName == null) {
					tableHeavyFontName = "HelveticaNeue";
				}
				return tableHeavyFontName;
			}
		}
		public static string TableHeadingFontName {
			get {
				if (tableFontName == null) {
					tableFontName = "HelveticaNeue-Light";
				}
				return tableFontName;
			}
		}
		public static string UserTableFontName {
			get {
				if (userTableFontName == null) {
					if (UIDevice.CurrentDevice.CheckSystemVersion (6, 0)) {
						userTableFontName = "Noteworthy-Light";
					}
					else {
						userTableFontName = "HelveticaNeue-Light";
					}
				}
				return userTableFontName;
			}
		}

		public override void Apply ()
		{
			if (IsModern)
				return;
			var options = "";
			Apply (UINavigationBar.Appearance, options);
			Apply (UIToolbar.Appearance, options);
			Apply (UIBarButtonItem.Appearance, options);
			Apply (UISearchBar.Appearance, options);
		}

		public override void Apply (UITableView tableView)
		{
			base.Apply (tableView);

			tableView.BackgroundView = new UIView {
				BackgroundColor = GroupedTableBackgroundColor,
			};
		}

		readonly Lazy<UIFont> cellFont = new Lazy<UIFont> (() => UIFont.FromName (TableFontName, 18));
		readonly Lazy<UIFont> cellDetailFont = new Lazy<UIFont> (() => UIFont.FromName (TableFontName, 14));

		public override void Apply (UITableViewCell cell)
		{
			cell.TextLabel.TextColor = SystemDarkTextColor;
			cell.TextLabel.Font = cellFont.Value;
			cell.DetailTextLabel.TextColor = UIColor.Gray;
			cell.DetailTextLabel.Font = cellDetailFont.Value;
			cell.SelectionStyle = UITableViewCellSelectionStyle.Gray;
		}

		readonly Lazy<UIFont> cellCheckedFont = new Lazy<UIFont> (() => UIFont.FromName (TableFontName, 18));

		public override void ApplyChecked (UITableViewCell cell)
		{
			cell.TextLabel.TextColor = ValueTextColor;
			cell.TextLabel.Font = cellCheckedFont.Value;
		}

		public override void ApplyCommand (UITableViewCell cell)
		{
			cell.TextLabel.TextColor = TintColor;
		}

		public static void Apply (UINavigationBar.UINavigationBarAppearance appearance, string options = null)
		{
			if (IsModern)
				return;

			appearance.TintColor = BarTintColor;

			appearance.SetTitleVerticalPositionAdjustment (-1, UIBarMetrics.Default);
			appearance.SetTitleVerticalPositionAdjustment (-4, UIBarMetrics.LandscapePhone);

			appearance.SetTitleTextAttributes (new UITextAttributes {
				TextColor = BarTextColor,
				TextShadowColor = BarTextShadowColor,
				TextShadowOffset = BarTextShadowOffset,
				Font = UIFont.FromName (TitleFontName, BarTitleFontSize),
			});
		}

		public static void Apply (UINavigationBar appearance, string options = null)
		{
		}

		public static void Apply (UIToolbar.UIToolbarAppearance appearance, string options = null)
		{
			if (IsModern)
				return;
			appearance.BackgroundColor = BarTintColor;
		}

		public static bool IsModern = true;

		public override void Apply (UIToolbar appearance)
		{
			if (IsModern)
				return;
			appearance.TintColor = BarTintColor;
		}

		public static Lazy<UIImage> BackButtonImage { get; private set; }
		public static Lazy<UIImage> ButtonImage { get; private set; }
		public static Lazy<UIImage> HudButtonImage { get; private set; }
		public static Lazy<UIImage> AddButtonImage { get; private set; }
		public static Lazy<UIImage> ActionButtonImage { get; private set; }
		public static Lazy<UIImage> DeleteButtonImage { get; private set; }
		public static Lazy<UIImage> CloneButtonImage { get; private set; }

		public static void Apply (UIBarButtonItem.UIBarButtonItemAppearance appearance, string options = null)
		{
			if (IsModern)
				return;

			var font = UIFont.FromName (BarButtonFontName, BarButtonFontSize);

			appearance.SetBackgroundImage (
				ButtonImage.Value,
				UIControlState.Normal,
				UIBarMetrics.Default);

			appearance.SetBackButtonBackgroundImage (
				BackButtonImage.Value,
				UIControlState.Normal,
				UIBarMetrics.Default);

			appearance.SetTitlePositionAdjustment (new UIOffset (0, 1), UIBarMetrics.Default);
#if NET6_0_OR_GREATER
#else
			appearance.SetTitleTextAttributes (new UITextAttributes {
				TextColor = BarTextColor,
				TextShadowColor = BarButtonTextShadowColor,
				TextShadowOffset = BarButtonTextShadowOffset,
				Font = font,
			}, UIControlState.Normal);

			appearance.SetTitleTextAttributes (new UITextAttributes {
				TextColor = UIColor.FromWhiteAlpha (0.9f, 1),
				TextShadowColor = BarButtonTextShadowColor,
				TextShadowOffset = BarButtonTextShadowOffset,
				Font = font,
			}, UIControlState.Disabled);

			appearance.SetTitleTextAttributes (new UITextAttributes {
				TextColor = UIColor.White,
				TextShadowColor = BarButtonTextShadowColor,
				TextShadowOffset = BarButtonTextShadowOffset,
				Font = font,
			}, UIControlState.Highlighted);
#endif
		}

		public override UIBarButtonItem CreateAddButton (EventHandler handler)
		{
			if (IsModern)
				return new UIBarButtonItem (UIBarButtonSystemItem.Add, handler);
			return CreateBarButton (AddButtonImage.Value, handler, UIBarButtonSystemItem.Add);
		}

		public override UIBarButtonItem CreateEditButton (EventHandler handler)
		{
//			if (IsModern)
				return new UIBarButtonItem (UIBarButtonSystemItem.Edit, handler);
//			return CreateBarButton (AddButtonImage.Value, handler);
		}

		public override UIBarButtonItem CreateActionButton (EventHandler handler)
		{
			if (IsModern)
				return new UIBarButtonItem (UIBarButtonSystemItem.Action, handler);

			return CreateBarButton (ActionButtonImage.Value, handler, UIBarButtonSystemItem.Action);
		}

		public override UIBarButtonItem CreateDeleteButton (EventHandler handler)
		{
			if (IsModern)
				return new UIBarButtonItem (UIBarButtonSystemItem.Trash, handler);

			return CreateBarButton (DeleteButtonImage.Value, handler, UIBarButtonSystemItem.Trash);
		}

		public override UIBarButtonItem CreateCancelButton (EventHandler handler)
		{
			return new UIBarButtonItem (UIBarButtonSystemItem.Cancel, handler);
		}

		public override UIBarButtonItem CreateDuplicateButton (EventHandler handler)
		{
			return CreateBarButton (CloneButtonImage.Value, handler, UIBarButtonSystemItem.Add);
		}

		public override UIBarButtonItem CreateBarButton (UIImage image, EventHandler handler, UIBarButtonSystemItem fallback = UIBarButtonSystemItem.Add)
		{
			if (image == null)
				return new UIBarButtonItem (fallback, handler);

			if (IsModern)
				return new UIBarButtonItem (image, UIBarButtonItemStyle.Plain, handler);

			var button = UIButton.FromType (UIButtonType.Custom);
			var sz = image.Size;
			sz.Width = Math.Max (44, 44);
			sz.Height = 44;
			button.Frame = new CGRect (CGPoint.Empty, sz);
			button.SetImage (image, UIControlState.Normal);
			button.ShowsTouchWhenHighlighted = true;
			button.TouchUpInside += handler;
			return new UIBarButtonItem (button);
		}

		public static void Apply (UISearchBar.UISearchBarAppearance appearance, string options = null)
		{
			if (IsModern)
				return;

			appearance.TintColor = BarTintColor;
		}
	}
}

