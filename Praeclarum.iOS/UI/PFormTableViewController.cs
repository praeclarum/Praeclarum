using System;
using UIKit;
using Foundation;
using System.Collections.Generic;
using CoreGraphics;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Praeclarum.UI
{
	public partial class PForm : UITableViewController, IThemeAware
	{
		/// <summary>
		/// Automatically show a done button?
		/// </summary>
		public bool AutoDoneButton { get; set; }

		/// <summary>
		/// Automatically show a cancel button?
		/// </summary>
		public bool AutoCancelButton { get; set; }

		readonly bool useBlur = !UIAccessibility.IsReduceTransparencyEnabled && UIDevice.CurrentDevice.CheckSystemVersion (11, 0);
		UIBlurEffect blurEffect;

		partial void InitializeUI ()
		{
			AutoCancelButton = false;
			AutoDoneButton = true;

			if (useBlur)
				blurEffect = UIBlurEffect.FromStyle (Theme.Current.IsDark ? UIBlurEffectStyle.Dark : UIBlurEffectStyle.Light);
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			try {

				TableView.Source = new FormSource (this);

				if (AutoCancelButton && (NavigationController == null || NavigationController.ViewControllers.Length == 1)) {
					NavigationItem.LeftBarButtonItem = new UIBarButtonItem (
						UIBarButtonSystemItem.Cancel,
						HandleCancel);
				}

				if (AutoDoneButton && (NavigationController == null || NavigationController.ViewControllers.Length == 1)) {
					NavigationItem.RightBarButtonItem = new UIBarButtonItem (
						UIBarButtonSystemItem.Done,
						HandleDone);
				}

				if (useBlur) {
					TableView.BackgroundColor = UIColor.Clear;
					TableView.BackgroundView = new UIVisualEffectView (effect: blurEffect);
					TableView.SeparatorEffect = UIVibrancyEffect.FromBlurEffect (blurEffect);
				}
				else {
					Theme.Current.Apply (this.TableView);
				}
			}
			catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			try {
				Theme.Current.Apply (NavigationController);
				if (useBlur) {
					var popover = NavigationController?.PopoverPresentationController;
					if (popover != null)
						popover.BackgroundColor = UIColor.Clear;
				}

				TableView.ReloadData ();
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public override UIStatusBarStyle PreferredStatusBarStyle ()
		{
			return Theme.Current.StatusBarStyle;
		}

		protected virtual async void HandleCancel (object sender, EventArgs e)
		{
			await DismissAsync (false);
		}

		protected virtual async void HandleDone (object sender, EventArgs e)
		{
			await DismissAsync (true);
		}

		public Task DismissAsync ()
		{
			return DismissAsync (true);
		}

		public virtual async Task DismissAsync (bool done)
		{
			foreach (var s in sections) {
				s.Dismiss ();
			}

			TableView.Source = null;
			sections.CollectionChanged -= HandleSectionsChanged;
			NavigationItem.RightBarButtonItem = null;

			if (NavigationController != null) {
				if (NavigationController.ViewControllers.Length == 1) {
					await NavigationController.DismissViewControllerAsync (true);
				} else {
					NavigationController.PopViewController (true);
				}
			} else {
				await DismissViewControllerAsync (true);
			}

			OnDismissed (done);
		}

		public event Action<bool> Dismissed = delegate{};

		protected virtual void OnDismissed (bool done)
		{
			Dismissed (done);
		}
//
//		protected virtual bool IsSingleTextEntry {
//			get {
//				var sections = ((FormBridge)Bridge).Form.Sections;
//				return sections.Count == 1 && sections[0].Items.Count == 1 && sections[0].Items[0] is TextItem;
//			}
//		}

		protected virtual FormSource CreateSource ()
		{
			return new FormSource (this);
		}

		public virtual void ReloadSection (PFormSection section)
		{
			if (!IsViewLoaded)
				return;

			var si = sections.IndexOf (section);
			if (si < 0)
				return;

			TableView.ReloadData ();//.ReloadSections (NSIndexSet.FromIndex (si), UITableViewRowAnimation.Automatic);
		}

		public virtual void ReloadAll (PFormSection section)
		{
			if (!IsViewLoaded)
				return;
			TableView.ReloadData ();
		}

		public virtual void FormatSection (PFormSection section)
		{
			if (!IsViewLoaded)
				return;

			var si = sections.IndexOf (section);
			if (si < 0)
				return;

			try {
				var ds = (FormSource)TableView.Source;

				for (int row = 0; row < section.Items.Count; row++) {
					var item = section.Items [row];
					if (TableView.CellAt (NSIndexPath.FromRowSection (row, si)) is PFormCell c) {
						try {
							c.Format (section, item);
						}
						catch (Exception ex) {
							Debug.WriteLine (ex);
						}
					}
				}

			} catch (Exception ex) {
				Debug.WriteLine (ex);				
			}
		}

		public void ApplyTheme (Theme theme)
		{
			if (useBlur && IsViewLoaded) {
				blurEffect = UIBlurEffect.FromStyle (theme.IsDark ? UIBlurEffectStyle.Dark : UIBlurEffectStyle.Light);
				TableView.BackgroundView = new UIVisualEffectView (effect: blurEffect);
				TableView.SeparatorEffect = UIVibrancyEffect.FromBlurEffect (blurEffect);
			}
		}

		public class PFormCell : UITableViewCell, IThemeAware
		{
			static readonly Dictionary<string, UIImage> imageCache =
				new Dictionary<string, UIImage> ();

			readonly bool useBlur = !UIAccessibility.IsReduceTransparencyEnabled && UIDevice.CurrentDevice.CheckSystemVersion (11, 0);

			public PFormCell (PFormItemDisplay display, string reuseId)
				: base (
					(display == PFormItemDisplay.Title) ? UITableViewCellStyle.Default
					: ((display == PFormItemDisplay.TitleAndSubtitle) ? UITableViewCellStyle.Subtitle : UITableViewCellStyle.Value1),
					reuseId)
			{
				ApplyTheme (Theme.Current);
			}

			public void Format (PFormSection section, object item)
			{
				var cell = this;
				var theme = Theme.Current;
				theme.Apply (cell);
				if (useBlur)
					BackgroundColor = theme.TableCellBackgroundColor.ColorWithAlpha (0.5f);

				var itemTitle = section.GetItemTitle (item);
				cell.TextLabel.Text = itemTitle;
				cell.TextLabel.AccessibilityLabel = itemTitle;
				cell.TextLabel.BackgroundColor = UIColor.Clear;

				var display = section.GetItemDisplay (item);
				if (display != PFormItemDisplay.Title) {
					cell.DetailTextLabel.Text = section.GetItemDetails (item) ?? "";
					if (display == PFormItemDisplay.TitleAndValue) {
						cell.DetailTextLabel.TextColor = UIApplication.SharedApplication.KeyWindow.TintColor;
					}
				}

				var imageUrl = section.GetItemImage (item);
				if (string.IsNullOrEmpty (imageUrl)) {
					cell.ImageView.Image = null;
				}
				else {
					UIImage image;
					if (!imageCache.TryGetValue (imageUrl, out image)) {
						image = UIImage.FromBundle (imageUrl);
						imageCache.Add (imageUrl, image);
					}
					cell.ImageView.Image = image;
				}

				if (section.GetItemChecked (item)) {
					cell.Accessory = UITableViewCellAccessory.Checkmark;

					theme.ApplyChecked (cell);


				}
				else {
					cell.Accessory = section.GetItemNavigates (item) ?
								 UITableViewCellAccessory.DisclosureIndicator :
								 UITableViewCellAccessory.None;
				}

				if (section.GetItemEnabled (item)) {
					var cmd = item as Command;
					if (cmd != null) {
						theme.ApplyCommand (cell);
					}
					cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
				}
				else {
					cell.TextLabel.TextColor = UIColor.LightGray;
					cell.SelectionStyle = UITableViewCellSelectionStyle.None;
				}
			}

			#region IThemeAware implementation

			public void ApplyTheme (Theme theme)
			{
				if (useBlur)
					BackgroundColor = theme.TableCellBackgroundColor.ColorWithAlpha (0.5f);
				else
					BackgroundColor = theme.TableCellBackgroundColor;
				TextLabel.TextColor = theme.TableCellTextColor;
			}

			#endregion
		}

		protected class FormSource : UITableViewSource
		{
			PForm controller;

			public FormSource (PForm controller)
			{
				this.controller = controller;
			}

			public override nint NumberOfSections (UITableView tableView)
			{
				try {
					return controller.Sections.Count;					
				} catch (Exception ex) {
					Log.Error (ex);
					return 0;
				}
			}

			public override nint RowsInSection (UITableView tableView, nint sectionIndex)
			{
				try {
					return controller.Sections [(int)sectionIndex].Items.Count;					
				} catch (Exception ex) {
					Log.Error (ex);
					return 0;
				}
			}

			public override string TitleForHeader (UITableView tableView, nint sectionIndex)
			{
				try {
					return controller.Sections [(int)sectionIndex].Title;					
				} catch (Exception ex) {
					Log.Error (ex);
					return "";
				}
			}

			public override string TitleForFooter (UITableView tableView, nint sectionIndex)
			{
				try {
					return controller.Sections [(int)sectionIndex].Hint;					
				} catch (Exception ex) {
					Log.Error (ex);
					return "";
				}
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				try {
					
					var section = controller.Sections [indexPath.Section];
					var item = section.Items [indexPath.Row];

					var itemDisplay = section.GetItemDisplay (item);
					var id = item.GetType ().Name + itemDisplay;

					var cell = tableView.DequeueReusableCell (id) as PFormCell;

					if (cell == null) {
						cell = new PFormCell (itemDisplay, id);
					}

					try {
						cell.Format (section, item);
					} catch (Exception ex) {
						Debug.WriteLine (ex);
					}

					return cell;
				} catch (Exception ex) {
					Log.Error (ex);
					return new UITableViewCell ();
				}

			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				try {
					var section = controller.Sections [indexPath.Section];
					var item = section.Items [indexPath.Row];

					if (section.GetItemEnabled (item)) {
						var sel = section.SelectItem (item);
						if (!sel) {
							tableView.DeselectRow (indexPath, true);
							var cell = tableView.CellAt (indexPath) as PFormCell;
							cell.Format (section, item);
						}
					}
					else {
						tableView.DeselectRow (indexPath, true);
					}

				} catch (Exception ex) {
					Debug.WriteLine (ex);
				}
			}
		}
	}
}

