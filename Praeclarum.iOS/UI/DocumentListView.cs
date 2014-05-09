using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Praeclarum.IO;
using Praeclarum.UI;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using Praeclarum.App;
using System.Collections.ObjectModel;

namespace Praeclarum.UI
{
	[Register ("DocumentListView")]
	public class DocumentListView : UITableView, IDocumentsView
	{
		public bool IsSyncing { get; set; }
		public List<DocumentsViewItem> Items { get; set; }

		public DocumentsSort Sort { get; set; }
		public event EventHandler SortChanged =  delegate {};

		public ObservableCollection<string> SelectedDocuments { get; private set; }

		public DocumentListView (RectangleF frame)
			: base (frame)
		{
			Items = new List<DocumentsViewItem> ();
			SelectedDocuments = new ObservableCollection<string> ();
			Source = new DocumentListViewSource (this);
		}

		public void DeleteItems (int[] docIndices, bool animated)
		{
			try {
				var indexPaths = docIndices.Select (x => NSIndexPath.FromRowSection (x, 0)).ToArray ();
				DeleteRows (indexPaths, animated ? UITableViewRowAnimation.Automatic : UITableViewRowAnimation.None);
			} catch (Exception ex) {
				Debug.WriteLine (ex);
			}
		}

		public void UpdateItem (int docIndex)
		{
			var indexPath = NSIndexPath.FromRowSection (docIndex, 0);
			var cell = CellAt (indexPath);
			cell.TextLabel.Text = Items [indexPath.Row].Reference.Name;
			cell.SetNeedsLayout ();
		}

		public void InsertItems (int[] docIndices)
		{
			var indexPaths = docIndices.Select (x => NSIndexPath.FromRowSection (x, 0)).ToArray ();
			InsertRows (indexPaths, UITableViewRowAnimation.Automatic);
		}

		public void RefreshListTimes ()
		{
			var vs = IndexPathsForVisibleRows;
			foreach (var v in vs) {
				if (v.Row >= Items.Count)
					continue;
				var cell = CellAt (v);
				cell.DetailTextLabel.Text = Items [v.Row].Reference.ModifiedAgo;
			}
		}

		public void SetOpenedDocument (int docIndex, bool animated)
		{
			var row = docIndex;
			if (row >= 0 && row < Items.Count) {

				var oldIndex = IndexPathForSelectedRow;

				if (oldIndex != null && oldIndex.Row == row)
					return;

				SelectRow (NSIndexPath.FromRowSection (row, 0), animated, UITableViewScrollPosition.Middle);
			}
		}

		public void SetSelecting (bool selecting, bool animated)
		{
			SelectedDocuments.Clear ();
		}

		void IDocumentsView.ReloadData ()
		{
			ReloadData ();
			if (!DocumentAppDelegate.IsPhone) {
				SetOpenedDocument (DocumentAppDelegate.Shared.OpenedDocIndex, false);
			}
		}

		public DocumentsViewItem GetItemAtPoint (Praeclarum.Graphics.PointF p)
		{
			var index = IndexPathForRowAtPoint (Praeclarum.Graphics.RectangleEx.ToPointF (p));
			if (index == null)
				return null;

			var i = index.Row;

			return Items[i];
		}

		public event Action<DocumentReference, object> RenameRequested = delegate {};

		public void ShowItem (int docIndex, bool animated)
		{
			var index = NSIndexPath.FromRowSection (docIndex, 0);
			ScrollToRow (index, UITableViewScrollPosition.Middle, animated);
		}
	}

	class DocumentListViewSource : UITableViewSource
	{
		readonly bool isPhone = DocumentAppDelegate.IsPhone;
		readonly DocumentListView controller;

		public DocumentListViewSource (DocumentListView controller)
		{
			this.controller = controller;
		}

		public override int NumberOfSections (UITableView tableView)
		{
			return 1;
		}

		static IFileSystem FileSystem { get { return FileSystemManager.Shared.ActiveFileSystem; } }

//			public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
//			{
//				return controller.Docs.Count > 0;
//			}
//
//			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
//			{
//				Console.WriteLine ("COMMIT");
//			}
//
//			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
//			{
//				return UITableViewCellEditingStyle.Delete;
//			}

		public override int RowsInSection (UITableView tableview, int section)
		{
			var c = controller.Items.Count;
			if (controller.IsSyncing)
				c = 1;
			return c;
		}

		static readonly UIImage directoryImage = UIImage.FromBundle ("Folder");

		readonly Theme theme = DocumentAppDelegate.Shared.Theme;

		UITableViewCell GetNotReadyCell (UITableView tableView)
		{
			var c = tableView.DequeueReusableCell ("NR");

			if (c == null) {
				c = new UITableViewCell (UITableViewCellStyle.Default, "NR");
				theme.Apply (c);
				c.TextLabel.TextColor = UIColor.Gray;
				c.TextLabel.TextAlignment = UITextAlignment.Center;
				c.SelectionStyle = UITableViewCellSelectionStyle.None;
				c.Accessory = UITableViewCellAccessory.None;

				var activity = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.Gray) {
					Frame = new RectangleF (100, 12, 21, 21),
					Tag = 42,
				};
				c.ContentView.AddSubview (activity);
			}

			var a = (UIActivityIndicatorView)c.ViewWithTag (42);
			a.StartAnimating ();

			c.TextLabel.Text = FileSystem.SyncStatus;

			return c;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var row = indexPath.Row;
			if (controller.IsSyncing)
				return GetNotReadyCell (tableView);

			var item = controller.Items[row];
			var d = item.Reference;

			var isDir = d.File.IsDirectory;

			var id = isDir ? "D" : "F";

			var c = tableView.DequeueReusableCell (id);

			if (c == null) {
				c = new UITableViewCell (UITableViewCellStyle.Value1, id);
				theme.Apply (c);

				c.Accessory = (isPhone||isDir) ? UITableViewCellAccessory.DisclosureIndicator : UITableViewCellAccessory.None;
				c.Tag = row;

			}

			if (isDir) {
				c.ImageView.Image = directoryImage;
			}

			c.TextLabel.Text = d.Name;
			c.AccessibilityLabel = d.Name;

			if (d.File.IsDownloaded) {
				c.DetailTextLabel.Text = d.File.IsDirectory ? "" : d.ModifiedAgo;
				c.SelectionStyle = UITableViewCellSelectionStyle.Gray;
			} else {
				c.DetailTextLabel.Text = string.Format ("downloading {0:0}%...", d.File.DownloadProgress*100);
				c.SelectionStyle = UITableViewCellSelectionStyle.None;
			}

			return c;
		}

		public override async void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			try {
				var row = indexPath.Row;
				if (row >= controller.Items.Count)
					return;

				var d = controller.Items[row].Reference;

				if (d.File.IsDirectory) {



					DocumentAppDelegate.Shared.OpenDirectory (indexPath.Row, animated: true);

				} else {
					await DocumentAppDelegate.Shared.Open (indexPath.Row, animated: true);
				}

			} catch (Exception ex) {
				Debug.WriteLine (ex);
				
			}
		}
	}
}
