using System;
using UIKit;
using Foundation;
using System.Collections.Generic;
using CoreGraphics;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Praeclarum.UI
{
	public partial class PForm : UITableViewController
	{
		/// <summary>
		/// Automatically show a done button?
		/// </summary>
		public bool AutoDoneButton { get; set; }

		/// <summary>
		/// Automatically show a cancel button?
		/// </summary>
		public bool AutoCancelButton { get; set; }

		partial void InitializeUI ()
		{
			AutoCancelButton = false;
			AutoDoneButton = true;
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

				var theme = DocumentAppDelegate.Shared.Theme;

				theme.Apply (TableView);
			}
			catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			try {
				var nc = NavigationController;
				if (nc != null) {
					DocumentAppDelegate.Shared.Theme.Apply (nc);
				}
				
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public override UIStatusBarStyle PreferredStatusBarStyle ()
		{
			return DocumentAppDelegate.Shared.Theme.StatusBarStyle;
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
					var c = TableView.CellAt (NSIndexPath.FromRowSection (row, si)) as PFormCell;
					if (c != null) {
						try {
							ds.FormatCell (c, section, item);
						} catch (Exception ex) {
							Debug.WriteLine (ex);
						}
					}
				}

			} catch (Exception ex) {
				Debug.WriteLine (ex);				
			}
		}

		public class PFormCell : UITableViewCell, IThemeAware
		{
			public PFormCell (PFormItemDisplay display, string reuseId)
				: base (
					(display == PFormItemDisplay.Title) ? UITableViewCellStyle.Default
					: ((display == PFormItemDisplay.TitleAndSubtitle) ? UITableViewCellStyle.Subtitle : UITableViewCellStyle.Value1),
					reuseId)
			{
				ApplyTheme (DocumentAppDelegate.Shared.Theme);
			}

			#region IThemeAware implementation

			public void ApplyTheme (Theme theme)
			{
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
						FormatCell (cell, section, item);
					} catch (Exception ex) {
						Debug.WriteLine (ex);
					}

					return cell;
				} catch (Exception ex) {
					Log.Error (ex);
					return new UITableViewCell ();
				}

			}

			static readonly Dictionary<string, UIImage> imageCache = 
				new Dictionary<string, UIImage> ();

			public void FormatCell (PFormCell cell, PFormSection section, object item)
			{
				var theme = DocumentAppDelegate.Shared.Theme;
				theme.Apply (cell);

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
				} else {
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


				} else {
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

//			UIFont headingFont = UIFont.FromName (WhiteTheme.TableHeadingFontName, 18);
//
//			public override float GetHeightForHeader (UITableView tableView, int section)
//			{
//				return string.IsNullOrWhiteSpace (controller.sections[section].Title) ? 11 : 44;
//			}
//
//			static readonly float LeftPadding = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ?
//				20 : 40;
//
//			public override UIView GetViewForHeader (UITableView tableView, int section)
//			{
//				var size = new SizeF (320, 44);
//
//				var label = new UILabel (new RectangleF (LeftPadding, 0, size.Width - LeftPadding, size.Height)) {
//					Opaque = false,
//					BackgroundColor = UIColor.Clear,
//					Text = controller.sections[section].Title,
//					TextColor = WhiteTheme.TableHeadingTextColor,
//					Font = headingFont,
//					AutoresizingMask = UIViewAutoresizing.FlexibleDimensions,
//				};
//
//				var h = new UIView (new RectangleF (PointF.Empty, size)) {
//					Opaque = false,
//					BackgroundColor = UIColor.Clear,
//				};
//				h.AddSubview (label);
//
//				return h;
//			}

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
							var source = tableView.Source as FormSource;
							if (source != null) {
								source.FormatCell (cell, section, item);
							}
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

