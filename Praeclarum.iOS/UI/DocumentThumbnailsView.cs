using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using Praeclarum.App;
using Praeclarum.IO;
using CoreGraphics;
using System.Collections.ObjectModel;
using Praeclarum.Graphics;
using System.Threading;

namespace Praeclarum.UI
{
	[Register ("DocumentThumbnailsView")]
	public class DocumentThumbnailsView : UICollectionView, IDocumentsView, IThemeAware
	{
		public static readonly NSString AddId = new NSString ("A");
		public static readonly NSString DirId = new NSString ("D");
		public static readonly NSString FileId = new NSString ("F");
		public static readonly NSString NotReadyId = new NSString ("NR");
		public static readonly NSString SortId = new NSString ("S");
		public static readonly NSString PatronId = new NSString ("P");

		public static float LabelHeight = 33;
		public static float Margin = 10;

		readonly bool ios8 = UIDevice.CurrentDevice.CheckSystemVersion (8, 0);
		readonly bool ios11 = UIDevice.CurrentDevice.CheckSystemVersion (11, 0);

		public List<DocumentsViewItem> Items {
			get;
			set;
		}
		public bool IsSyncing {
			get;
			set;
		}
		public bool ShowPatron { get; set; }

		public Praeclarum.Graphics.SizeF ThumbnailSize { get; private set; }

		public DocumentThumbnailsView (CGRect frame)
			: base (frame, new DocumentThumbnailsViewFlowLayout ())
		{
			Items = new List<DocumentsViewItem> ();
			SelectedDocuments = new ObservableCollection<string> ();
			SelectedDocuments.CollectionChanged += HandleSelectedDocumentsChanged;

			AlwaysBounceVertical = true;

			BackgroundColor = DocumentAppDelegate.Shared.Theme.DocumentsBackgroundColor;

			RegisterClassForCell (typeof(AddDocumentCell), AddId);
			RegisterClassForCell (typeof(DocumentThumbnailCell), FileId);
			RegisterClassForCell (typeof(DirectoryThumbnailCell), DirId);
			RegisterClassForCell (typeof(NotReadyThumbnailCell), NotReadyId);
			RegisterClassForCell (typeof(SortThumbnailCell), SortId);
			RegisterClassForCell (typeof(PatronCell), PatronId);

			if (ios8) {
				((UICollectionViewFlowLayout)this.CollectionViewLayout).EstimatedItemSize = new CGSize (88, 122);
			}

			Delegate = new DocumentThumbnailsViewDelegate ();
			DataSource = new DocumentThumbnailsViewDataSource ();

			Console.WriteLine ("UI {0}", UIDevice.CurrentDevice.UserInterfaceIdiom);

			var thumbWidth =
				UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ?
				320.0f/3.0f - Margin - Margin/3.0f:
				768.0f/5.0f - Margin - Margin/5.0f;

			thumbWidth = (int)(thumbWidth + 0.5f);

			var thumbHeight = thumbWidth / DocumentAppDelegate.Shared.App.ThumbnailAspectRatio;

			thumbHeight = (int)(thumbHeight + 0.5f);

			ThumbnailSize = new Praeclarum.Graphics.SizeF ((float)thumbWidth, (float)thumbHeight);

			if (ios11) {
				ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Always;
			}

//			Console.WriteLine ("THUMB SIZE = {0}", ThumbnailSize);
		}

		public void UpdateLayout ()
		{
			CollectionViewLayout.InvalidateLayout ();
		}

		void HandleSelectedDocumentsChanged (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			var cells = VisibleCells.OfType<BaseDocumentThumbnailCell> ().ToList ();

			foreach (var c in cells) {
				c.SetDocumentSelected (SelectedDocuments.Any (x => x == c.Document.File.Path), true);
			}
		}

		DocumentsSort sort;
		public DocumentsSort Sort { 
			get {
				return sort;
			}
			set {
				if (sort == value)
					return;
				sort = value;
				SortChanged (this, EventArgs.Empty);
			}
		}

		public event EventHandler SortChanged = delegate {};

		bool editing = false;
		public bool Editing { get { return editing; } set { SetEditing (value, false); } }



		public void SetEditing (bool editing, bool animated)
		{
			if (this.editing == editing)
				return;

			SelectedDocuments.Clear ();

			this.editing = editing;

			foreach (var c in VisibleCells.OfType <ThumbnailCell> ()) {
				c.Editing = editing;
				var d = c as DocumentThumbnailCell;
				if (d != null)
					d.SetDocumentSelected (false, true);
			}

			// TODO: Animate the thumbnails
		}

		bool selecting = false;
		public bool Selecting { get { return selecting; } set { SetSelecting (value, false); } }

		public ObservableCollection<string> SelectedDocuments { get; private set; }

		public void SetSelecting (bool selecting, bool animated)
		{
			if (this.selecting == selecting)
				return;

			SelectedDocuments.Clear ();

			this.selecting = selecting;

			foreach (var c in VisibleCells.OfType <ThumbnailCell> ()) {
				c.Selecting = selecting;
				var d = c as DocumentThumbnailCell;
				if (d != null)
					d.SetDocumentSelected (false, true);
			}
		}

		void IDocumentsView.DeleteItems (int[] docIndices, bool animated)
		{
			try {				
//				Console.WriteLine ("DeleteItems " + string.Join (", ", docIndices.Select (x => x.ToString ())));
				var indexPaths = docIndices.Select (x => NSIndexPath.FromRowSection (x+1, 1)).ToArray ();
				DeleteItems (indexPaths);
			} catch (Exception ex) {
				Debug.WriteLine (ex);
			}
		}

		public void UpdateItem (int docIndex)
		{
			var indexPath = NSIndexPath.FromRowSection (docIndex + 1, 1);

			var cell = CellForItem (indexPath) as DocumentThumbnailCell;

			if (cell != null) {
				cell.RefreshThumbnail ();
			}
			// TODO: Update cell file name
//			cell.TextLabel.Text = Docs [indexPath.Row].Name;
		}

		void IDocumentsView.InsertItems (int[] docIndices)
		{
//			Console.WriteLine ("InsertItems " + string.Join (", ", docIndices.Select (x => x.ToString ())));
//			var indexPaths = docIndices.Select (x => NSIndexPath.FromRowSection (x+1, 1)).ToArray ();
//			InsertItems (indexPaths);
			ReloadData ();
		}

		public void RefreshListTimes ()
		{
			var vs = IndexPathsForVisibleItems;
			foreach (var v in vs) {
				if (v.Row >= Items.Count)
					continue;
//				var cell = CellForItem (v);
				// TODO: Refresh the time of the cell
//				cell.DetailTextLabel.Text = Docs [v.Row].ModifiedAgo;
			}
		}

		public void SetOpenedDocument (int docIndex, bool animated)
		{
			var row = docIndex;
			if (row >= 0 && row < Items.Count) {

				var oldIndex = GetIndexPathsForSelectedItems ().FirstOrDefault ();

				if (oldIndex != null && oldIndex.Row == row)
					return;

				SelectItem (NSIndexPath.FromRowSection (row, 1), animated, UICollectionViewScrollPosition.CenteredVertically);
			}
		}

		public DocumentsViewItem GetItemAtPoint (Praeclarum.Graphics.PointF p)
		{
			var index = IndexPathForItemAtPoint (Praeclarum.Graphics.RectangleEx.ToPointF (p));
			if (index == null || index.Section == 0)
				return null;

			var i = index.Row - 1;

			if (0 <= i && i < Items.Count)
				return Items [i];

			return null;
		}

		public event Action<DocumentReference, object> RenameRequested = delegate {};

		public void HandleRenameRequested (object sender, EventArgs a)
		{
			try {
				var c = sender as BaseDocumentThumbnailCell;
				if (c == null)
					return;

				var view = c.CreateThumbnailView ();

				RenameRequested (c.Document, view);

			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public void ShowItem (int docIndex, bool animated)
		{
			var index = NSIndexPath.FromRowSection (docIndex + 1, 1);
			ScrollToItem (index, UICollectionViewScrollPosition.CenteredVertically, animated);
		}

		#region IThemeAware implementation

		public void ApplyTheme (Theme theme)
		{
			BackgroundColor = theme.DocumentsBackgroundColor;	
		}

		#endregion

		public void StopLoadingThumbnails ()
		{
			foreach (var c in VisibleCells.OfType<DocumentThumbnailCell> ()) {
				c.StopLoading ();
			}
		}
	}

	class DocumentThumbnailsViewFlowLayout : UICollectionViewFlowLayout
	{
		//public override UICollectionViewLayoutAttributes[] LayoutAttributesForElementsInRect (CGRect rect)
		//{
		//	var attributes = base.LayoutAttributesForElementsInRect(rect);
		//	var leftMargin = SectionInset.Left;

		//	nfloat maxY = -1.0f;
		//	foreach (var layoutAttribute in attributes) {
		//		if (layoutAttribute.IndexPath.Section == 0) continue;

		//		if (layoutAttribute.Frame.Y >= maxY) {
		//			leftMargin = SectionInset.Left;
		//		}

		//		var f = layoutAttribute.Frame;
		//		f.X = leftMargin;
		//		layoutAttribute.Frame = f;

		//		leftMargin += layoutAttribute.Frame.Width + MinimumInteritemSpacing;
		//		maxY = (nfloat)Math.Max(layoutAttribute.Frame.Bottom, maxY);
		//	}

		//	return attributes;
		//}
	}

	class DocumentThumbnailsViewDelegate : UICollectionViewDelegateFlowLayout
	{
		static readonly CGSize sortSize = new CGSize (SortThumbnailCell.Width, 33);

		public override CGSize GetSizeForItem (UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
		{
			try {
				if (indexPath.Section == 0)
					return sortSize;

				var controller = (DocumentThumbnailsView)collectionView;

				var s = controller.ThumbnailSize;

				var itemSize = new CGSize (s.Width, s.Height + DocumentThumbnailsView.LabelHeight);

				//				Console.WriteLine ("ITEM SIZE = {0}", itemSize);

				return itemSize;
			}
			catch (Exception ex) {
				Log.Error (ex);
				return new CGSize (44, 44);
			}
		}

		public override UIEdgeInsets GetInsetForSection (UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			try {
				nfloat h = 0.0f;
				var t = DocumentThumbnailsView.Margin / 2;
				var b = 0.0f;

				var controller = (DocumentThumbnailsView)collectionView;

				var frameW = collectionView.Frame.Width;

				if (section == 0) {
					h = (frameW - sortSize.Width) / 2 - 44; // - 44 because of insets
					t = 15;
					b = 11;
				}
				else {
					h = controller.IsSyncing ? ((frameW - controller.ThumbnailSize.Width) / 2 - 44) : DocumentThumbnailsView.Margin;
				}

				return new UIEdgeInsets (t, h, b, h);
			}
			catch (Exception ex) {
				Log.Error (ex);
				return new UIEdgeInsets ();
			}
		}

		public override nfloat GetMinimumInteritemSpacingForSection (UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			if (section == 0)
				return 0;

			return DocumentThumbnailsView.Margin / 2;
		}

		public override async void ItemSelected (UICollectionView collectionView, NSIndexPath indexPath)
		{
			if (indexPath.Section == 0)
				return;

			//			Console.WriteLine ("SELECT {0}", indexPath.Row);

			var controller = (DocumentThumbnailsView)collectionView;

			if (controller.IsSyncing)
				return;

			var row = indexPath.Row;

			if (controller.Editing || controller.Selecting) {

				if (row == 0) {
					// Add
				}
				else if (row > controller.Items.Count) {
					await DocumentAppDelegate.Shared.ShowPatronAsync ();
				}
				else {
					row--;

					var item = controller.Items[row];

					var d = item.Reference;

					if (controller.SelectedDocuments.Contains (d.File.Path)) {

						controller.SelectedDocuments.Remove (d.File.Path);

					}
					else {

						controller.SelectedDocuments.Add (d.File.Path);

					}
				}

			}
			else {

				try {
					if (row == 0) {
						// Add
						await DocumentAppDelegate.Shared.AddAndOpenNewDocument ();
					}
					else if (row > controller.Items.Count) {
						await DocumentAppDelegate.Shared.ShowPatronAsync ();
					}
					else {
						row--;



						var d = controller.Items[row].Reference;

						if (d.File.IsDirectory) {
							DocumentAppDelegate.Shared.OpenDirectory (row, animated: true);
						}
						else {
							await DocumentAppDelegate.Shared.OpenDocument (row, animated: true);
						}
					}

				}
				catch (Exception ex) {
					Debug.WriteLine (ex);
				}
			}
		}
	}

	class ThumbnailFrameView : UIView, IThemeAware
	{
		public ThumbnailFrameView ()
		{
			var appDel = DocumentAppDelegate.Shared;
			BackgroundColor = Praeclarum.Graphics.ColorEx.GetUIColor (appDel.App.GetThumbnailBackgroundColor (appDel.Theme));
		}

		#region IThemeAware implementation

		public virtual void ApplyTheme (Theme theme)
		{
			SetNeedsDisplay ();
		}

		#endregion

		public override void Draw (CGRect rect)
		{
			try {
				var c = UIGraphics.GetCurrentContext ();

				var appDel = DocumentAppDelegate.Shared;
				var backColor = Praeclarum.Graphics.ColorEx.GetUIColor (appDel.App.GetThumbnailBackgroundColor (appDel.Theme));
				backColor.SetFill ();
				c.FillRect (rect);

				var b = Bounds;
				c.SetLineWidth (1.0f);

				appDel.Theme.DocumentsFrameSideColor.SetStroke ();

				c.MoveTo (0, 0);
				c.AddLineToPoint (0, b.Height);
				c.StrokePath ();

				c.MoveTo (b.Width, 0);
				c.AddLineToPoint (b.Width, b.Height);
				c.StrokePath ();

				appDel.Theme.DocumentsFrameBottomColor.SetStroke ();

				c.MoveTo (0, b.Height);
				c.AddLineToPoint (b.Width, b.Height);
				c.StrokePath ();
			}
			catch (Exception ex) {
				Log.Error (ex);
			}
		}
	}

	[Preserve]
	class SortThumbnailCell : UICollectionViewCell, IThemeAware
	{
		public const float Width = 160.0f;

		UISegmentedControl segs;

		DocumentsSort sort = DocumentsSort.Date;
		public DocumentsSort Sort {
			get { return sort; }
			set {
				if (sort == value)
					return;
				sort = value;
				segs.SelectedSegment = sort == DocumentsSort.Name ? 1 : 0;
			}
		}
		public DocumentThumbnailsView Controller;

		[Preserve]
		public SortThumbnailCell (IntPtr handle)
			: base (handle)
		{
			Initialize ();
		}

		[Preserve]
		public SortThumbnailCell ()
		{
			Initialize ();
		}

		readonly bool ios7 = UIDevice.CurrentDevice.CheckSystemVersion (7, 0);
		readonly bool ios8 = UIDevice.CurrentDevice.CheckSystemVersion (8, 0);

		void Initialize ()
		{
			segs = new UISegmentedControl (new[] { "Date", "Name" }) {

			};

			ApplyTheme (DocumentAppDelegate.Shared.Theme);

			if (!ios7) {
				segs.TintColor = UIColor.FromWhiteAlpha (165 / 255.0f, 1);
				segs.SetTitleTextAttributes (new UITextAttributes {
					TextColor = UIColor.FromWhiteAlpha (220 / 255.0f, 1),
					TextShadowColor = UIColor.Gray,
					TextShadowOffset = new UIOffset (0, -1),
				}, UIControlState.Normal);
				segs.SetTitleTextAttributes (new UITextAttributes {
					TextColor = UIColor.White,
					TextShadowColor = UIColor.DarkGray,
					TextShadowOffset = new UIOffset (0, -1),
				}, UIControlState.Selected);
				segs.ControlStyle = UISegmentedControlStyle.Bar;
			}

			segs.SelectedSegment = sort == DocumentsSort.Name ? 1 : 0;
			segs.ValueChanged += HandleValueChanged;
		}

		#region IThemeAware implementation

		public void ApplyTheme (Theme theme)
		{
			BackgroundColor = theme.DocumentsBackgroundColor;
			if (ios7) {
				segs.TintColor = theme.DocumentsControlColor;
			}
		}

		#endregion

		bool filledContent = false;
		public void FillContentView ()
		{
			if (filledContent)
				return;
			if (ContentView != null) {
				ContentView.AddSubview (segs);

				ContentView.ConstrainLayout (() =>
					segs.Frame.GetMidX () == ContentView.Frame.GetMidX () &&
					segs.Frame.GetMidY () == ContentView.Frame.GetMidY () &&
					segs.Frame.Width == Width);

				filledContent = true;
			}
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			sort = segs.SelectedSegment == 0 ? DocumentsSort.Date : DocumentsSort.Name;
			if (Controller != null)
				Controller.Sort = sort;
		}
	}

	class PatronCell : UICollectionViewCell, IThemeAware
	{
		ButtonView button;

		public PatronCell ()
		{
			ContentView.Frame = new CGRect (0, 0, 100, 100);
			Initialize ();
		}

		public PatronCell (IntPtr handle)
			: base (handle)
		{
			Initialize ();
		}

		void Initialize ()
		{
			button = new ButtonView ();
			//			ContentView.BackgroundColor = UIColor.Red;
			button.Frame = ContentView.Bounds;
			button.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			//			button.SetTitle ("Support Calca Development", UIControlState.Normal);
			ContentView.AddSubview (button);
			ApplyTheme (DocumentAppDelegate.Shared.Theme);
		}

		class ButtonView : UIView, IThemeAware
		{
			public ButtonView ()
			{
				ApplyTheme (null);
			}

			public void ApplyTheme (Theme theme)
			{
				//				var appdel = DocumentAppDelegate.Shared;
				BackgroundColor = UIColor.Clear;// appdel.App.GetThumbnailBackgroundColor (appdel.Theme).GetUIColor ().ColorWithAlpha (0.5f);
			}

			public override void Draw (CGRect rect)
			{
				base.Draw (rect);
				var b = Bounds;
				var bb = b;
				bb.Inflate (-4.0f, -4.0f);
				var w = bb.Width;
				var h = 3 * w / 5;
				bb.Height = h;
				bb.Y = (b.Height - h) / 5;
				var rr = UIBezierPath.FromRoundedRect (bb, 10.0f);
				var color = TintColor;
				color.SetStroke ();
				rr.Stroke ();
				color.SetFill ();
				var str = new NSString ("Become a Patron");
				var tb = bb;
				tb.Inflate (-4.0f, -4.0f);
				var font = UIFont.SystemFontOfSize (16.0f);
				var sz = str.StringSize (font, tb.Size);
				tb.Y += (tb.Height - sz.Height) / 2;
				str.DrawString (tb, font, UILineBreakMode.WordWrap, UITextAlignment.Center);

				var dtb = bb;
				dtb.Y += bb.Height + 4;
				var appdel = DocumentAppDelegate.Shared;
				appdel.Theme.NavigationTextColor.SetFill ();
				str = new NSString ("Support the Development of " + appdel.App.Name);
				str.DrawString (dtb, UIFont.SystemFontOfSize (10.0f), UILineBreakMode.WordWrap, UITextAlignment.Center);
			}
		}

		public void ApplyTheme (Theme theme)
		{
			button.SetNeedsDisplay ();
		}
	}

	abstract class ThumbnailCell : UICollectionViewCell, IThemeAware
	{
		public static UIColor GetNotSelectableColor (Theme theme)
		{
			if (theme.IsDark) {
				return UIColor.FromWhiteAlpha (1.0f - 0.875f, 0.5961f); ;
			}
			return UIColor.FromWhiteAlpha (0.875f, 0.5961f);
		}

		public Praeclarum.Graphics.SizeF ThumbnailSize = new Praeclarum.Graphics.SizeF (1, 1);

		protected UILabel label;

		protected CGRect ThumbnailFrame {
			get {
				var b = Bounds;
				var w = ThumbnailSize.Width;
				var h = ThumbnailSize.Height;
				return new CGRect ((b.Width - w) / 2, 0, w, h);
			}
		}

		bool editing = false;
		public bool Editing {
			get { return editing; }
			set {
				if (editing == value)
					return;
				editing = value;
				if (editing)
					BeginEditingStyle ();
				else
					EndEditingStyle ();
			}
		}

		bool selecting = false;
		public bool Selecting {
			get { return selecting; }
			set {
				if (selecting == value)
					return;
				selecting = value;
				if (selecting)
					BeginEditingStyle ();
				else
					EndEditingStyle ();
			}
		}

		static readonly bool ios7 = UIDevice.CurrentDevice.CheckSystemVersion (7, 0);

		protected ThumbnailCell (IntPtr handle)
			: base (handle)
		{
			var theme = DocumentAppDelegate.Shared.Theme;
			ApplyTheme (theme);

			var b = Bounds;

			label = new UILabel (new CGRect (0, b.Bottom - DocumentThumbnailsView.LabelHeight + 1, b.Width, DocumentThumbnailsView.LabelHeight - 1)) {
				Text = "",
				TextColor = theme.TintColor,
				Font = ios7 ? UIFont.PreferredCaption2 : UIFont.SystemFontOfSize (10),
				Lines = 2,
				TextAlignment = UITextAlignment.Center,
				AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin,
				LineBreakMode = UILineBreakMode.MiddleTruncation,
				BackgroundColor = BackgroundColor,
				Opaque = true,
			};
		}

		#region IThemeAware implementation

		public virtual void ApplyTheme (Theme theme)
		{
			BackgroundColor = theme.DocumentsBackgroundColor;
			if (label != null) {
				label.BackgroundColor = theme.DocumentsBackgroundColor;
				label.TextColor = theme.TintColor;
			}
		}

		#endregion

		bool filledContent = false;
		public void FillContentView ()
		{
			if (filledContent)
				return;

			if (ContentView != null) {
				filledContent = true;

				ContentView.AddSubview (label);

				OnFillContentView ();
			}
		}

		protected abstract void OnFillContentView ();

		protected virtual void BeginEditingStyle ()
		{
		}

		protected virtual void EndEditingStyle ()
		{
		}
	}

	class AddDocumentCell : ThumbnailCell
	{
		readonly AddFrameView frameView;

		public void SetThumbnail ()
		{
			frameView.Frame = ThumbnailFrame;
		}

		public AddDocumentCell (IntPtr handle)
			: base (handle)
		{
			frameView = new AddFrameView ();

			label.Text = "New " + DocumentAppDelegate.Shared.App.DocumentBaseName;
			label.TextColor = DocumentAppDelegate.Shared.Theme.NavigationTextColor;

			SetThumbnail ();
		}

		public override void ApplyTheme (Theme theme)
		{
			base.ApplyTheme (theme);
			if (label != null) {
				label.TextColor = theme.NavigationTextColor;
			}
		}

		protected override void OnFillContentView ()
		{
			ContentView.AddSubviews (frameView);
		}

		protected override void BeginEditingStyle ()
		{
			frameView.Editing = true;
			frameView.SetNeedsDisplay ();
		}

		protected override void EndEditingStyle ()
		{
			frameView.Editing = false;
			frameView.SetNeedsDisplay ();
		}

		class AddFrameView : ThumbnailFrameView
		{
			public bool Editing = false;

			public override void ApplyTheme (Theme theme)
			{
				SetNeedsDisplay ();
			}

			public AddFrameView ()
			{
				var appdel = DocumentAppDelegate.Shared;
				BackgroundColor = Praeclarum.Graphics.ColorEx.GetUIColor (appdel.App.GetThumbnailBackgroundColor (appdel.Theme));
			}

			public override void Draw (CGRect rect)
			{
				try {
					base.Draw (rect);

					var appdel = DocumentAppDelegate.Shared;
					var theme = appdel.Theme;

					var c = UIGraphics.GetCurrentContext ();
					
					var backColor = Praeclarum.Graphics.ColorEx.GetUIColor (appdel.App.GetThumbnailBackgroundColor (theme));
					backColor.SetFill ();
					c.FillRect (rect);

					var b = Bounds;
					
					c.SetLineWidth (2.0f);
					
					var color = theme.TintColor;
					
					color.SetStroke ();
					
					var size = (nfloat)(Math.Min (b.Width, b.Height) * 0.5);
					var f = new CGRect ((b.Width - size) / 2, (b.Height - size) / 2, size, size);
					f.X = (int)f.X;
					f.Y = (int)f.Y;
					
					c.MoveTo (f.Left, f.GetMidY ());
					c.AddLineToPoint (f.Right, f.GetMidY ());
					c.StrokePath ();
					
					c.MoveTo (f.GetMidX (), f.Top);
					c.AddLineToPoint (f.GetMidX (), f.Bottom);
					c.StrokePath ();
					
					if (Editing) {
						GetNotSelectableColor (theme).SetFill ();
						c.FillRect (b);
					}
				} catch (Exception ex) {
					Log.Error (ex);
				}
			}
		}
	}

	abstract class BaseDocumentThumbnailCell : ThumbnailCell
	{
		readonly UITapGestureRecognizer labelTap;

		protected BaseDocumentThumbnailCell (IntPtr handle)
			: base (handle)
		{
			labelTap = new UITapGestureRecognizer (HandleLabelTap);
			label.AddGestureRecognizer (labelTap);
			label.UserInteractionEnabled = true;
		}

		public abstract DocumentReference Document { get; set; }

		public EventHandler RenameRequested = delegate {};

		void HandleLabelTap (UITapGestureRecognizer g)
		{
			if (g.State == UIGestureRecognizerState.Ended) {
				RenameRequested (this, EventArgs.Empty);
			}
		}

		public abstract UIView CreateThumbnailView ();

		SelectedView selectedView;

		bool selected = false;
		public bool DocumentSelected
		{
			get {
				return selected;
			}
		}

		public abstract void SetDocumentSelected (bool value, bool animated);

		protected void SetDocumentSelected (bool value, UIView frameView, bool animated)
		{
			if (value == selected)
				return;
			selected = value;

			if (selected) {
				if (selectedView == null) {
					selectedView = new SelectedView {
						Alpha = animated ? 0 : 1,
					};
					frameView.AddSubview (selectedView);
				}
				selectedView.Frame = frameView.Bounds;
				if (animated) {
					UIView.Animate (0.1, () => selectedView.Alpha = 1);
				} else {
					selectedView.Alpha = 1;
				}
			} else {
				if (selectedView != null) {
					if (animated) {
						UIView.Animate (0.1, () => selectedView.Alpha = 0);
					} else {
						selectedView.Alpha = 0;
					}
				}
			}
		}

		class SelectedView : UIView
		{
			UIColor SelectionColor = UIColor.Blue;

			const float borderThickness = 3.0f;

			public SelectedView ()
			{
				Opaque = false;
				SelectionColor = Praeclarum.Graphics.ColorEx.GetUIColor (DocumentAppDelegate.Shared.App.TintColor);
				BackgroundColor = Praeclarum.Graphics.ColorEx.GetUIColor (DocumentAppDelegate.Shared.App.TintColor).ColorWithAlpha (0.1f);
			}

			public override void Draw (CGRect dirtyRect)
			{
				try {
					var c = UIGraphics.GetCurrentContext ();
					
					var rect = Bounds;
					
					rect.Inflate (-borderThickness / 2, -borderThickness / 2);
					
					SelectionColor.SetStroke ();
					
					c.SetLineWidth (borderThickness);
					c.StrokeRect (rect);
				} catch (Exception ex) {
					Log.Error (ex);					
				}
			}
		}

	}

	class DocumentThumbnailCell : BaseDocumentThumbnailCell
	{
		UIImageView imageView;

		ThumbnailFrameView frameView;

		DocumentReference doc;

		public override DocumentReference Document {
			get {
				return doc;
			}
			set {
				if (doc == value)
					return;

				doc = value;
				if (doc == null)
					return;

				AccessibilityLabel = doc.Name;
				label.Text = doc.Name;

				SetThumbnail (null);

				RefreshThumbnail ();
			}
		}

		public override void ApplyTheme (Theme theme)
		{
			base.ApplyTheme (theme);
			RefreshThumbnail ();
		}

		CancellationTokenSource refreshCancelSource;

		public void StopLoading ()
		{
			refreshCancelSource?.Cancel ();
			refreshCancelSource = null;
		}

		public void RefreshThumbnail ()
		{
			refreshCancelSource?.Cancel ();
			refreshCancelSource = new CancellationTokenSource ();

			RefreshThumbnailAsync (refreshCancelSource.Token).ContinueWith (t => {
				if (t.IsFaulted) {
					Debug.WriteLine (t.Exception);
				}
			});
		}

		async Task RefreshThumbnailAsync (CancellationToken cancellationToken)
		{
			var appDel = DocumentAppDelegate.Shared;
			var Cache = appDel.ThumbnailCache;
			if (Cache == null)
				return;

			if (doc == null || doc.File == null)
				return;

			var path = doc.File.Path;

			var theme = DocumentAppDelegate.Shared.Theme;

			var scale = UIScreen.MainScreen.Scale;

			var thumbKey = appDel.GetThumbnailKey (doc.File, theme);

			var memImage = Cache.GetMemoryImage (thumbKey);

			if (memImage != null) {
				SetThumbnail (memImage);
			}

			if (cancellationToken.IsCancellationRequested)
				return;

			var thumbImage = await Cache.GetImageAsync (thumbKey, doc.ModifiedTime);

			if (cancellationToken.IsCancellationRequested)
				return;

			if (thumbImage == null) {
				thumbImage = await appDel.GenerateThumbnailAsync (doc, ThumbnailSize, theme, scale, cancellationToken);
				if (thumbImage != null) {
					if (doc != null && path == doc.File.Path) {
						SetThumbnail (thumbImage);
					}
					await Cache.SetGeneratedImageAsync (thumbKey, thumbImage, saveToDisk: true);
				}
			} else {
				if (doc != null && path == doc.File.Path) {
					SetThumbnail (thumbImage);
				}
			}
		}

		void SetThumbnail (UIImage thumbImage)
		{
			var b = Bounds;

			var tw = ThumbnailSize.Width;
			var th = ThumbnailSize.Height;

			var w = tw + 1;
			var h = th + 0.5f;

			var f = new CGRect ((b.Width - w)/2, 0, w, h);

			frameView.Frame = f;

			imageView.Frame = new CGRect (0.5f, 0, tw, th);
			imageView.Image = thumbImage;

			if (thumbImage == null) {
				var appdel = DocumentAppDelegate.Shared;
				imageView.BackgroundColor = Praeclarum.Graphics.ColorEx.GetUIColor (appdel.App.GetThumbnailBackgroundColor (appdel.Theme));
			}
		}

		public DocumentThumbnailCell (IntPtr handle)
			: base (handle)
		{
			frameView = new ThumbnailFrameView ();

			imageView = new UIImageView {
				ContentMode = UIViewContentMode.Center,
				Opaque = true,
				ClipsToBounds = true,
			};

			frameView.AddSubview (imageView);
		}

		protected override void OnFillContentView ()
		{
			ContentView.AddSubviews (frameView);
		}

		public override UIView CreateThumbnailView ()
		{
			var fr = new ThumbnailFrameView {
				Frame = frameView.Frame,
			};
			var im = new UIImageView {
				ContentMode = UIViewContentMode.Center,
				Opaque = true,
				ClipsToBounds = true,
				BackgroundColor = imageView.BackgroundColor,
				Image = imageView.Image,
				Frame = new CGRect (0.5f, 0, ThumbnailSize.Width, ThumbnailSize.Height),
			};
			fr.AddSubview (im);

			return fr;
		}

		public override void SetDocumentSelected (bool value, bool animated)
		{
			SetDocumentSelected (value, frameView, animated);
		}
	}

	class DirectoryThumbnailCell : BaseDocumentThumbnailCell
	{
		DocumentsViewItem item;
		DirectoryBackgroundView bg;

		readonly List<UIImageView> thumbnailViews = new List<UIImageView> ();

		const int NumCols = 3;
		const int MaxSubs = 9;

		public override UIColor BackgroundColor {
			get {
				return base.BackgroundColor;
			}
			set {
				base.BackgroundColor = UIColor.Clear;
			}
		}

		CancellationTokenSource refreshCancellationTokenSource;

		public DocumentsViewItem Item {
			get {
				return item;
			}
			set {
				var newDir = item != value;

				item = value;
				if (item == null)
					return;

				AccessibilityLabel = item.Reference.Name;
				label.Text = item.Reference.Name;

				if (newDir)
					SetThumbnails (null);

				refreshCancellationTokenSource?.Cancel ();
				refreshCancellationTokenSource = new CancellationTokenSource ();
				RefreshThumbnails (refreshCancellationTokenSource.Token).ContinueWith (t => {
					if (t.IsFaulted)
						Debug.WriteLine (t.Exception);
				});
			}
		}

		public override DocumentReference Document {
			get {
				return item != null ? item.Reference : null;
			}
			set {
				// Required because I can't figureout how to make only get virtual
			}
		}

		void SetThumbnails (UIImage[] thumbnailImages)
		{
			var thumbnailFrame = ThumbnailFrame;
			bg.Frame = thumbnailFrame;

			var gg = 6.0f;
			var g = 4.0f;
			var w = -g + g/NumCols - 2*gg/NumCols + thumbnailFrame.Width/NumCols;
			var h = w / DocumentAppDelegate.Shared.App.ThumbnailAspectRatio;
			var vg = (thumbnailFrame.Height - NumCols * h - (NumCols-1)*g) / 2;

			if (thumbnailImages == null || thumbnailImages.Length == 0) {
				foreach (var tv in thumbnailViews) {
					tv.Hidden = true;
				}
			} else {

				while (thumbnailViews.Count < MaxSubs) {
					var tv = new UIImageView {
						Opaque = true,
						ContentMode = UIViewContentMode.ScaleAspectFill,
					};
					thumbnailViews.Add (tv);
					bg.AddSubview (tv);
				}

				for (int i = 0; i < thumbnailImages.Length; i++) {
					var tv = thumbnailViews [i];
					var ti = thumbnailImages [i];

					var r = i / NumCols;
					var c = i % NumCols;

					tv.Frame = new CGRect (
						c * (g + w) + gg,
						r * (g + h) + vg,
						w,
						h);

					tv.Image = ti;
					tv.Hidden = false;
				}

				for (int i = thumbnailImages.Length; i < MaxSubs; i++) {
					thumbnailViews [i].Hidden = true;
				}
			}
		}

		async Task RefreshThumbnails (CancellationToken cancellationToken)
		{
			var appDel = DocumentAppDelegate.Shared;
			var Cache = appDel.ThumbnailCache;
			if (Cache == null)
				return;
			if (item == null)
				return;

			var startDir = Item.Reference.File.Path;
			UIImage[] thumbnails = null;

			var scale = UIScreen.MainScreen.Scale;

			try {
				var fileTasks = from dr in item.SubReferences.Take (9)
                               select GetThumbnail (new DocumentReference (dr.File, DocumentAppDelegate.Shared.App.CreateDocument, false), appDel.Theme, scale, cancellationToken);

				thumbnails = (await Task.WhenAll (fileTasks)).Where (x => x != null).ToArray ();

			} catch (Exception ex) {
				Debug.WriteLine (ex);				
			}

			// Make sure we only update if this cell is still bound to the same dir
			if (startDir == Item.Reference.File.Path) {
				SetThumbnails (thumbnails);
			}
		}

		async Task<UIImage> GetThumbnail (DocumentReference doc, Theme theme, nfloat scale, CancellationToken cancellationToken)
		{
			var appDel = DocumentAppDelegate.Shared;
			var Cache = appDel.ThumbnailCache;

			var thumbKey = appDel.GetThumbnailKey (doc.File, theme);

			var thumbImage = await Cache.GetImageAsync (thumbKey, doc.ModifiedTime);

			if (thumbImage == null) {
				thumbImage = await appDel.GenerateThumbnailAsync (doc, ThumbnailSize, theme, scale, cancellationToken);
				if (thumbImage != null) {
					await Cache.SetGeneratedImageAsync (thumbKey, thumbImage, saveToDisk: true);
				}
			}

			return thumbImage;
		}

		public DirectoryThumbnailCell (IntPtr handle)
			: base (handle)
		{
			bg = new DirectoryBackgroundView {
				BackgroundColor = BackgroundColor,
			};

			SetThumbnails (null);
		}

		protected override void OnFillContentView ()
		{
			ContentView.AddSubviews (bg);
		}

		public override UIView CreateThumbnailView ()
		{
			var b = new DirectoryBackgroundView {
				Frame = bg.Frame,
			};

			return b;
		}

		public override void ApplyTheme (Theme theme)
		{
			base.ApplyTheme (theme);
			RefreshThumbnails (CancellationToken.None).ContinueWith (t => {
				if (t.IsFaulted) {
					Console.WriteLine (t.Exception);
				}
			});
		}

		class DirectoryBackgroundView : UIView, IThemeAware
		{
			public DirectoryBackgroundView ()
			{
				Opaque = true;
				ContentMode = UIViewContentMode.Redraw;
			}

			#region IThemeAware implementation

			public void ApplyTheme (Theme theme)
			{
				SetNeedsDisplay ();
			}

			#endregion

			public override void Draw (CGRect rect)
			{
				base.Draw (rect);

				var theme = DocumentAppDelegate.Shared.Theme;

				try {
					theme.DocumentsFolderColor.SetFill ();
					UIBezierPath.FromRoundedRect (Bounds, 10).Fill ();
				} catch (Exception ex) {
					Log.Error (ex);					
				}
			}
		}

		public override void SetDocumentSelected (bool value, bool animated)
		{
			SetDocumentSelected (value, bg, animated);
		}
	}

	class NotReadyThumbnailCell : UICollectionViewCell, IThemeAware
	{
		UIActivityIndicatorView activity;
		UILabel label;

		static readonly bool ios7 = UIDevice.CurrentDevice.CheckSystemVersion (7, 0);

		public NotReadyThumbnailCell (IntPtr handle)
			: base (handle)
		{
			var theme = DocumentAppDelegate.Shared.Theme;
			BackgroundColor = theme.DocumentsBackgroundColor;

			var b = Bounds;

			var text = "Loading...";
			var font = ios7 ? UIFont.PreferredBody : UIFont.SystemFontOfSize (UIFont.SystemFontSize);

			var size = text.StringSize (font);

			activity = new UIActivityIndicatorView (theme.IsDark ? UIActivityIndicatorViewStyle.White : UIActivityIndicatorViewStyle.Gray) {
			};
			var af = activity.Frame;
			af.X = (b.Width - size.Width) / 2 - 11 - af.Width;
			af.Y = (b.Height - size.Height)/2;
			activity.Frame = af;

			label = new UILabel (b) {
				Text = text,
				Font = font,
				TextColor = theme.NavigationTextColor,
				AutoresizingMask = UIViewAutoresizing.FlexibleDimensions,
				Lines = 2,
				TextAlignment = UITextAlignment.Center,
			};

			if (ContentView != null) {
				ContentView.AddSubviews (label, activity);
			}

			activity.StartAnimating ();
		}

		#region IThemeAware implementation
		public void ApplyTheme (Theme theme)
		{
			BackgroundColor = theme.DocumentsBackgroundColor;
			if (label != null) {
				label.TextColor = theme.NavigationTextColor;
			}
		}
		#endregion


		bool filledContent = false;
		public void FillContentView ()
		{
			if (filledContent)
				return;
			if (ContentView != null) {
				ContentView.AddSubviews (label, activity);

				filledContent = true;
			}
		}
	}

	class DocumentThumbnailsViewDataSource : UICollectionViewDataSource
	{
		public override nint NumberOfSections (UICollectionView collectionView)
		{
			return 2;
		}

		public override nint GetItemsCount (UICollectionView collectionView, nint section)
		{
			var controller = (DocumentThumbnailsView)collectionView;

			if (section == 0)
				return 1;

			var count = controller.Items.Count;
//			Console.WriteLine ("Items.Count == {0}", count);

			if (controller.IsSyncing) {
				return 1;
			}
			else {
				var n = count + 1;
				if (controller.ShowPatron) {
					n++;
				}
				return n;
			}
		}

		public override UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var controller = collectionView as DocumentThumbnailsView;
			var isDir = false;
			var id = DocumentThumbnailsView.FileId;
			DocumentsViewItem item = null;
			DocumentReference d = null;

			try {
				
				if (indexPath.Section == 0) {
					return GetSortCell (collectionView, indexPath);
				}
				
				if (controller.IsSyncing) {
					return GetNotReadyCell (collectionView, indexPath);
				}
				
				var row = indexPath.Row;
				if (row == 0) {
					return GetAddCell (collectionView, indexPath);
				}

				if (row > controller.Items.Count) {
					return GetPatronCell (collectionView, indexPath);
				}
				
				row--;
				item = controller.Items [row];
				d = item.Reference;
				
				isDir = d.File.IsDirectory;
				
				id = isDir ? DocumentThumbnailsView.DirId : DocumentThumbnailsView.FileId;
			} catch (Exception ex) {
				Log.Error (ex);				
			}

			var c = (BaseDocumentThumbnailCell)collectionView.DequeueReusableCell (id, indexPath);
			c.FillContentView ();

			try {
				c.RenameRequested = controller.HandleRenameRequested;
				
				if (isDir) {
					var dirCell = ((DirectoryThumbnailCell)c);
					dirCell.ThumbnailSize = controller.ThumbnailSize;
					dirCell.Item = item;
					dirCell.Editing = controller.Editing;
					dirCell.Selecting = controller.Selecting;
					dirCell.SetDocumentSelected (controller.SelectedDocuments.Contains (d.File.Path), false);
				} else {
					var docCell = ((DocumentThumbnailCell)c);
					docCell.ThumbnailSize = controller.ThumbnailSize;
					docCell.Document = d;
					docCell.Editing = controller.Editing;
					docCell.Selecting = controller.Selecting;
					docCell.SetDocumentSelected (controller.SelectedDocuments.Contains (d.File.Path), false);
				}
			} catch (Exception ex) {
				Log.Error (ex);				
			}

			return c;
		}

		UICollectionViewCell GetPatronCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var controller = (DocumentThumbnailsView)collectionView;

			var c = (PatronCell)collectionView.DequeueReusableCell (DocumentThumbnailsView.PatronId, indexPath);

			return c;
		}

		UICollectionViewCell GetSortCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var controller = (DocumentThumbnailsView)collectionView;

			var c = (SortThumbnailCell)collectionView.DequeueReusableCell (DocumentThumbnailsView.SortId, indexPath);
			c.FillContentView ();
			c.Controller = controller;
			c.Sort = controller.Sort;
			return c;
		}

		UICollectionViewCell GetAddCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var controller = (DocumentThumbnailsView)collectionView;

			var c = (AddDocumentCell)collectionView.DequeueReusableCell (DocumentThumbnailsView.AddId, indexPath);
			c.FillContentView ();
			c.ThumbnailSize = controller.ThumbnailSize;
			c.Editing = controller.Editing;
			c.Selecting = controller.Selecting;
			c.SetThumbnail ();

			return c;
		}

		UICollectionViewCell GetNotReadyCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var c = (NotReadyThumbnailCell)collectionView.DequeueReusableCell (DocumentThumbnailsView.NotReadyId, indexPath);
			c.FillContentView ();
			return c;
		}
	}
}





