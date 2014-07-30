using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Praeclarum.UI
{
	public partial class PForm : UITableViewController
	{
		public bool AutoDoneButton { get; set; }

		partial void InitializeUI ()
		{
			AutoDoneButton = true;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			if (AutoDoneButton && (NavigationController == null || NavigationController.ViewControllers.Length == 1)) {
				NavigationItem.RightBarButtonItem = new UIBarButtonItem (
					UIBarButtonSystemItem.Done,
					HandleDone);
			}

			var theme = DocumentAppDelegate.Shared.Theme;

			theme.Apply (TableView);

			TableView.Source = new FormSource (this);
		}

		protected virtual async void HandleDone (object sender, EventArgs e)
		{
			await DismissAsync ();
		}

		public virtual async Task DismissAsync ()
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
					NavigationController.PopViewControllerAnimated (true);
				}
			} else {
				await DismissViewControllerAsync (true);
			}
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
			var si = sections.IndexOf (section);
			if (si < 0)
				return;

			TableView.ReloadSections (NSIndexSet.FromIndex (si), UITableViewRowAnimation.Automatic);
		}

		public virtual void FormatSection (PFormSection section)
		{
			var si = sections.IndexOf (section);
			if (si < 0)
				return;

			try {
				var ds = (FormSource)TableView.Source;

				for (int row = 0; row < section.Items.Count; row++) {
					var item = section.Items [row];
					var c = TableView.CellAt (NSIndexPath.FromRowSection (row, si));
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

		protected class FormSource : UITableViewSource
		{
			PForm controller;

			public FormSource (PForm controller)
			{
				this.controller = controller;
			}

			public override int NumberOfSections (UITableView tableView)
			{
				return controller.Sections.Count;
			}

			public override int RowsInSection (UITableView tableView, int sectionIndex)
			{
				return controller.Sections [sectionIndex].Items.Count;
			}

			public override string TitleForHeader (UITableView tableView, int sectionIndex)
			{
				return controller.Sections [sectionIndex].Title;
			}

			public override string TitleForFooter (UITableView tableView, int sectionIndex)
			{
				return controller.Sections [sectionIndex].Hint;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				var section = controller.Sections [indexPath.Section];
				var item = section.Items [indexPath.Row];

				var id = item.GetType ().Name;

				var cell = tableView.DequeueReusableCell (id);

				if (cell == null) {
					cell = new UITableViewCell (UITableViewCellStyle.Default, id);
				}

				try {
					FormatCell (cell, section, item);
				} catch (Exception ex) {
					Debug.WriteLine (ex);
				}

				return cell;
			}

			static readonly Dictionary<string, UIImage> imageCache = 
				new Dictionary<string, UIImage> ();

			readonly Theme theme = DocumentAppDelegate.Shared.Theme;

			public void FormatCell (UITableViewCell cell, PFormSection section, object item)
			{
				theme.Apply (cell);


				var itemTitle = section.GetItemTitle (item);
				cell.TextLabel.Text = itemTitle;
				cell.TextLabel.AccessibilityLabel = itemTitle;

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

				if (section.GetItemChecked (item)) {
					cell.Accessory = UITableViewCellAccessory.Checkmark;

					theme.ApplyChecked (cell);


				} else {
					cell.Accessory = section.GetItemNavigates (item) ?
					             UITableViewCellAccessory.DisclosureIndicator :
					             UITableViewCellAccessory.None;
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
							var cell = tableView.CellAt (indexPath);
							((FormSource)tableView.Source).FormatCell (cell, section, item);
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

