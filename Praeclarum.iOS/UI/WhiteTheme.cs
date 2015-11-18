using System;
using UIKit;
using CoreGraphics;
using System.Collections.Generic;

namespace Praeclarum.UI
{
	public interface IThemeAware
	{
		void ApplyTheme (Theme theme);
	}

	public abstract class Theme
	{
		static protected readonly bool ios7 = UIDevice.CurrentDevice.CheckSystemVersion (7, 0);

		readonly Lazy<UIImage> CloneButtonImage = new Lazy<UIImage> (
			() => UIImage.FromBundle ("Clone.png"));

		readonly Lazy<UIImage> MoveButtonImage = new Lazy<UIImage> (
			() => UIImage.FromBundle ("Move.png"));

		public virtual void Apply ()
		{
			if (ios7) {
				UINavigationBar.Appearance.TintColor = GetTintColor ();
				UIBarButtonItem.Appearance.TintColor = GetTintColor ();

				UINavigationBar.Appearance.BarTintColor = NavigationBackgroundColor;
				UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes {
					ForegroundColor = NavigationTextColor,
				};
				UIToolbar.Appearance.BarTintColor = NavigationBackgroundColor;

				foreach (var w in UIApplication.SharedApplication.Windows) {
					ApplyToVC (w.RootViewController, new HashSet<IntPtr> ());
				}
			}
		}

		public virtual void Apply (UINavigationController nc)
		{
			nc.NavigationBar.BarStyle = NavigationBarStyle;
			nc.NavigationBar.BarTintColor = NavigationBackgroundColor;
			nc.NavigationBar.TitleTextAttributes = new UIStringAttributes {
				ForegroundColor = NavigationTextColor,
			};
			nc.Toolbar.BarStyle = NavigationBarStyle;
			nc.Toolbar.BarTintColor = NavigationBackgroundColor;
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

				vc.SetNeedsStatusBarAppearanceUpdate ();

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

		public virtual UIStatusBarStyle StatusBarStyle
		{
			get {
				return UIStatusBarStyle.Default;
			}
		}

		public virtual UIBarStyle NavigationBarStyle
		{
			get {
				return UIBarStyle.Default;
			}
		}

		public virtual bool IsDark { get { return false; } }

		public virtual UIColor NavigationBackgroundColor {
			get {
				return null;
			}
		}
		public virtual UIColor NavigationTextColor {
			get {
				return UIColor.Black;
			}
		}
		public virtual UIColor DocumentsBackgroundColor {
			get {
				return UIColor.FromRGB (222, 222, 222);
			}
		}
		public virtual UIColor DocumentsControlColor {
			get {
				return UIColor.FromWhiteAlpha (59 / 255.0f, 1);
			}
		}
		public virtual UIColor DocumentsFolderColor {
			get {
				return UIColor.FromRGB (195, 195, 195);
			}
		}
		public virtual UIColor DocumentsFrameSideColor {
			get {
				return UIColor.FromRGB ((nfloat)202 / 255.0f, (nfloat)202 / 255.0f, (nfloat)202 / 255.0f);
			}
		}
		public virtual UIColor DocumentsFrameBottomColor {
			get {
				return UIColor.FromRGB ((nfloat)176 / 255.0f, (nfloat)176 / 255.0f, (nfloat)176 / 255.0f);
			}
		}
		public virtual UIColor GroupedTableBackgroundColor {
			get {
				return UIColor.GroupTableViewBackgroundColor;
			}
		}
		public virtual UIColor TableCellBackgroundColor {
			get {
				return UIColor.White;
			}
		}
		public virtual UIColor TableCellTextColor {
			get {
				return UIColor.DarkTextColor;
			}
		}
		public virtual UIColor TableSeparatorColor {
			get {
				return UIColor.FromWhiteAlpha (0.85f, 1.0f);
			}
		}

		public virtual void Apply (UITableView tableView)
		{
			var c = GroupedTableBackgroundColor;
			if (tableView.BackgroundView == null) {
				tableView.BackgroundView = new UIView { BackgroundColor = c };
			} else {
				tableView.BackgroundView.BackgroundColor = c;
			}
			tableView.BackgroundColor = c;
			tableView.SeparatorColor = TableSeparatorColor;
		}

		public virtual void Apply (UIToolbar toolbar)
		{
		}

		public virtual void Apply (UITableViewCell cell)
		{
			cell.BackgroundColor = TableCellBackgroundColor;
			cell.TextLabel.TextColor = TableCellTextColor;
		}

		public virtual void ApplyCommand (UITableViewCell cell)
		{
			cell.TextLabel.TextColor = ios7 ? 
				GetTintColor () : 
			                           UIColor.DarkTextColor;
		}

		readonly UIColor checkColor = UIColor.FromRGB (50, 79, 133);

		static UIColor GetTintColor ()
		{
			return DocumentAppDelegate.Shared.TintColor;
		}

		public virtual void ApplyChecked (UITableViewCell cell)
		{
			cell.TextLabel.TextColor = ios7 ? 
				GetTintColor () : 
			                           checkColor;
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
		public override bool IsDark {
			get {
				return true;
			}
		}
		public override UIStatusBarStyle StatusBarStyle {
			get {
				return UIStatusBarStyle.LightContent;
			}
		}
		public override UIColor NavigationBackgroundColor {
			get {
				return UIColor.Black;
			}
		}
		public override UIColor NavigationTextColor {
			get {
				return UIColor.LightGray;
			}
		}
		public override UIColor DocumentsBackgroundColor {
			get {
				return UIColor.FromRGB (22, 22, 22);
			}
		}
		public override UIColor DocumentsFolderColor {
			get {
				return UIColor.Black;
			}
		}
		public override UIColor DocumentsFrameSideColor {
			get {
				return UIColor.Black;
			}
		}
		public override UIColor DocumentsFrameBottomColor {
			get {
				return UIColor.Black;
			}
		}
		public override UIColor DocumentsControlColor {
			get {
				return UIColor.Gray;
			}
		}
		public override UIBarStyle NavigationBarStyle {
			get {
				return UIBarStyle.Black;
			}
		}
		public override UIColor GroupedTableBackgroundColor {
			get {
				return DocumentsBackgroundColor;
			}
		}
		public override UIColor TableCellBackgroundColor {
			get {
				return UIColor.FromWhiteAlpha (0.15f, 1.0f);
			}
		}
		public override UIColor TableCellTextColor {
			get {
				return UIColor.White;
			}
		}
		public override UIColor TableSeparatorColor {
			get {
				return UIColor.FromWhiteAlpha (0.25f, 1.0f);
			}
		}
	}

	public class WhiteTheme : Theme
	{
		public static UIColor TintColor = UIColor.Black;

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

		public static UIColor GroupedTableBackgroundColor = UIColor.FromWhiteAlpha (0.92f, 1);

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

		public static UIColor TableHeadingTextColor = UIColor.FromWhiteAlpha (0.0f, 1);
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
			cell.TextLabel.TextColor = UIColor.DarkTextColor;
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

