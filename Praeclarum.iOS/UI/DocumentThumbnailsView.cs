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

namespace Praeclarum.UI
{
	[Register ("DocumentThumbnailsView")]
	public class DocumentThumbnailsView : UICollectionView, IDocumentsView
	{
		public static readonly NSString AddId = new NSString ("A");
		public static readonly NSString DirId = new NSString ("D");
		public static readonly NSString FileId = new NSString ("F");
		public static readonly NSString NotReadyId = new NSString ("NR");
		public static readonly NSString SortId = new NSString ("S");

		public static float LabelHeight = 33;
		public static float Margin = 10;
		static int NumHorizontalThumbnails = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ? 3 : 5;

		public List<DocumentsViewItem> Items {
			get;
			set;
		}
		public bool IsSyncing {
			get;
			set;
		}

		public Praeclarum.Graphics.SizeF ThumbnailSize { get; private set; }

		public static readonly UIColor DefaultBackgroundColor = UIColor.FromRGB (222, 222, 222);

		public DocumentThumbnailsView (CGRect frame)
			: base (frame, new UICollectionViewFlowLayout ())
		{
			Items = new List<DocumentsViewItem> ();
			SelectedDocuments = new ObservableCollection<IFile> ();
			SelectedDocuments.CollectionChanged += HandleSelectedDocumentsChanged;

			AlwaysBounceVertical = true;

			BackgroundColor = DefaultBackgroundColor;

			RegisterClassForCell (typeof(AddDocumentCell), AddId);
			RegisterClassForCell (typeof(DocumentThumbnailCell), FileId);
			RegisterClassForCell (typeof(DirectoryThumbnailCell), DirId);
			RegisterClassForCell (typeof(NotReadyThumbnailCell), NotReadyId);
			RegisterClassForCell (typeof(SortThumbnailCell), SortId);

			Console.WriteLine ("SORT REF");

			Delegate = new DocumentThumbnailsViewDelegate ();
			DataSource = new DocumentThumbnailsViewDataSource ();

			var thumbWidth = UIScreen.MainScreen.Bounds.Width/NumHorizontalThumbnails - Margin - Margin/NumHorizontalThumbnails;

			thumbWidth = (int)(thumbWidth + 0.5f);

			var thumbHeight = thumbWidth / DocumentAppDelegate.Shared.App.ThumbnailAspectRatio;

			thumbHeight = (int)(thumbHeight + 0.5f);

			ThumbnailSize = new Praeclarum.Graphics.SizeF ((float)thumbWidth, (float)thumbHeight);

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
				c.SetDocumentSelected (SelectedDocuments.Contains (c.Document.File), true);
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

		public ObservableCollection<IFile> SelectedDocuments { get; private set; }

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

		/// <summary>
		/// Gets or sets the content inset.
		/// http://stackoverflow.com/questions/19483511/uirefreshcontrol-with-uicollectionview-in-ios7
		/// </summary>
		public override UIEdgeInsets ContentInset {
			get {
				try {
					return base.ContentInset;
				} catch (Exception ex) {
					Log.Error (ex);
					return new UIEdgeInsets ();
				}
			}
			set {
				try {
					if (Tracking) {
						var diff = value.Top - base.ContentInset.Top;
						var translation = PanGestureRecognizer.TranslationInView (this);
						translation.Y -= diff * 3.0f / 2.0f;
						PanGestureRecognizer.SetTranslation (translation, this);
					}
					base.ContentInset = value;
				} catch (Exception ex) {
					Log.Error (ex);					
				}
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
			var c = sender as BaseDocumentThumbnailCell;
			if (c == null)
				return;

			var view = c.CreateThumbnailView ();

			RenameRequested (c.Document, view);
		}

		public void ShowItem (int docIndex, bool animated)
		{
			var index = NSIndexPath.FromRowSection (docIndex + 1, 1);
			ScrollToItem (index, UICollectionViewScrollPosition.CenteredVertically, animated);
		}
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
				//			Console.WriteLine ("item size = {0}", itemSize);
				return itemSize;
			} catch (Exception ex) {
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
					h = (frameW - sortSize.Width) / 2;
					t = 15;
					b = 11;
				} else {
					h = controller.IsSyncing ? (frameW - controller.ThumbnailSize.Width) / 2 : DocumentThumbnailsView.Margin;
				} 
				
				return new UIEdgeInsets (t, h, b, h);
			} catch (Exception ex) {
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

			var controller = (DocumentThumbnailsView)collectionView;

			if (controller.IsSyncing)
				return;

			var row = indexPath.Row;

			if (controller.Editing || controller.Selecting) {

				if (row == 0) {
					// Add
				} else {
					row--;

					var item = controller.Items [row];

					var d = item.Reference;

					if (controller.SelectedDocuments.Contains (d.File)) {

						controller.SelectedDocuments.Remove (d.File);

					} else {

						controller.SelectedDocuments.Add (d.File);

					}
				}

			} else {

				try {
					if (row == 0) {
						// Add
						await DocumentAppDelegate.Shared.AddAndOpenNewDocument ();
					} else {
						row--;
						var d = controller.Items [row].Reference;

						if (d.File.IsDirectory) {
							DocumentAppDelegate.Shared.OpenDirectory (row, animated: true);
						} else {
							await DocumentAppDelegate.Shared.OpenDocument (row, animated: true);
						}
					}

				} catch (Exception ex) {
					Debug.WriteLine (ex);
				}
			}
		}
	}

	class ThumbnailFrameView : UIView
	{
		public ThumbnailFrameView ()
		{
			BackgroundColor = Praeclarum.Graphics.ColorEx.GetUIColor (DocumentAppDelegate.Shared.App.ThumbnailBackgroundColor);
		}

		public override void Draw (CGRect rect)
		{
			try {
				var c = UIGraphics.GetCurrentContext ();
				
				var b = Bounds;
				c.SetLineWidth (1.0f);
				
				c.SetStrokeColor (202 / 255.0f, 202 / 255.0f, 202 / 255.0f, 1);
				//			UIColor.Red.SetStroke ();
				
				c.MoveTo (0, 0);
				c.AddLineToPoint (0, b.Height);
				c.StrokePath ();
				
				c.MoveTo (b.Width, 0);
				c.AddLineToPoint (b.Width, b.Height);
				c.StrokePath ();
				
				c.SetStrokeColor (176 / 255.0f, 176 / 255.0f, 176 / 255.0f, 1);
				//			UIColor.Green.SetStroke ();
				
				c.MoveTo (0, b.Height);
				c.AddLineToPoint (b.Width, b.Height);
				c.StrokePath ();
			} catch (Exception ex) {
				Log.Error (ex);				
			}
		}
	}

	[Preserve]
	class SortThumbnailCell : UICollectionViewCell
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

		void Initialize ()
		{
			BackgroundColor = UIColor.FromRGB (222, 222, 222);

			var b = Bounds;

			var ios7 = UIDevice.CurrentDevice.CheckSystemVersion (7, 0);

			segs = new UISegmentedControl (new [] { "Date", "Name" }) {

			};
			var darkColor = UIColor.FromWhiteAlpha (59 / 255.0f, 1);
			if (ios7) {
				segs.TintColor = darkColor;
			} else {
				segs.TintColor = UIColor.FromWhiteAlpha (165/255.0f, 1);
				segs.SetTitleTextAttributes (new UITextAttributes {
					TextColor = UIColor.FromWhiteAlpha (220/255.0f, 1),
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

			ContentView.AddSubview (segs);

			ContentView.ConstrainLayout (() =>
				segs.Frame.GetMidX () == ContentView.Frame.GetMidX () &&
				segs.Frame.GetMidY () == ContentView.Frame.GetMidY () &&
				segs.Frame.Width == Width);
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			sort = segs.SelectedSegment == 0 ? DocumentsSort.Date : DocumentsSort.Name;
			if (Controller != null)
				Controller.Sort = sort;
		}
	}

	abstract class ThumbnailCell : UICollectionViewCell
	{
		public static UIColor NotSelectableColor = UIColor.FromWhiteAlpha (0.875f, 0.5961f);

		public Praeclarum.Graphics.SizeF ThumbnailSize = new Praeclarum.Graphics.SizeF (1, 1);

		protected UILabel label;

		static readonly nfloat screenScale = UIScreen.MainScreen.Scale;

		protected CGRect ThumbnailFrame
		{
			get {
				var b = Bounds;

				var tw = ThumbnailSize.Width;
				var th = ThumbnailSize.Height;

				var w = tw + (screenScale > 1 ? 1 : 2);
				var h = th + (screenScale > 1 ? 0.5f : 1);

				return new CGRect ((b.Width - w)/2, 0, w, h);
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
			BackgroundColor = UIColor.FromRGB (222, 222, 222);

			var b = Bounds;


			label = new UILabel (new CGRect (0, b.Bottom - DocumentThumbnailsView.LabelHeight+1, b.Width, DocumentThumbnailsView.LabelHeight-1)) {
				Text = "",
				TextColor = Praeclarum.Graphics.ColorEx.GetUIColor (DocumentAppDelegate.Shared.App.TintColor),
				Font = ios7 ? UIFont.PreferredCaption2 : UIFont.SystemFontOfSize (10),
				Lines = 2,
				TextAlignment = UITextAlignment.Center,
				AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin,
				LineBreakMode = UILineBreakMode.MiddleTruncation,
				BackgroundColor = BackgroundColor,
				Opaque = true,
			};

			ContentView.AddSubview (label);
		}

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

			ContentView.AddSubviews (frameView);

			label.Text = "Create New";
			label.TextColor = UIColor.Black;

			SetThumbnail ();
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



			public AddFrameView ()
			{
				BackgroundColor = Praeclarum.Graphics.ColorEx.GetUIColor (DocumentAppDelegate.Shared.App.ThumbnailBackgroundColor);
			}

			public override void Draw (CGRect rect)
			{
				try {
					base.Draw (rect);
					
					var c = UIGraphics.GetCurrentContext ();
					
					var b = Bounds;
					
					c.SetLineWidth (2.0f);
					
					var color = Praeclarum.Graphics.ColorEx.GetUIColor (DocumentAppDelegate.Shared.App.TintColor);
					
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
						NotSelectableColor.SetFill ();
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

		public void RefreshThumbnail ()
		{
			RefreshThumbnailAsync ().ContinueWith (t => {
				if (t.IsFaulted) {
					Debug.WriteLine (t.Exception);
				}
			});
		}

		async Task RefreshThumbnailAsync ()
		{
			var appDel = DocumentAppDelegate.Shared;
			var Cache = appDel.ThumbnailCache;
			if (Cache == null)
				return;

			var path = doc.File.Path;

			var thumbKey = appDel.GetThumbnailKey (doc.File);

			var memImage = Cache.GetMemoryImage (thumbKey);

			if (memImage != null) {
				SetThumbnail (memImage);
			}

			var thumbImage = await Cache.GetImageAsync (thumbKey, doc.ModifiedTime);

			if (thumbImage == null) {
				thumbImage = await appDel.GenerateThumbnailAsync (doc, ThumbnailSize);
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
				imageView.BackgroundColor = Praeclarum.Graphics.ColorEx.GetUIColor (DocumentAppDelegate.Shared.App.ThumbnailBackgroundColor);
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

				RefreshThumbnails ().ContinueWith (t => {
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

		async Task RefreshThumbnails ()
		{
			var appDel = DocumentAppDelegate.Shared;
			var Cache = appDel.ThumbnailCache;
			if (Cache == null)
				return;

			var startDir = Item.Reference.File.Path;
			UIImage[] thumbnails = null;

			try {
				var fileTasks = from dr in item.SubReferences.Take (9)
				                select GetThumbnail (new DocumentReference (dr.File, DocumentAppDelegate.Shared.App.CreateDocument, false));

				thumbnails = await Task.WhenAll (fileTasks);

			} catch (Exception ex) {
				Debug.WriteLine (ex);				
			}

			// Make sure we only update if this cell is still bound to the same dir
			if (startDir == Item.Reference.File.Path) {
				SetThumbnails (thumbnails);
			}
		}

		async Task<UIImage> GetThumbnail (DocumentReference doc)
		{
			var appDel = DocumentAppDelegate.Shared;
			var Cache = appDel.ThumbnailCache;

			var thumbKey = appDel.GetThumbnailKey (doc.File);

			var thumbImage = await Cache.GetImageAsync (thumbKey, doc.ModifiedTime);

			if (thumbImage == null) {
				thumbImage = await appDel.GenerateThumbnailAsync (doc, ThumbnailSize);
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

			ContentView.AddSubviews (bg);

			SetThumbnails (null);
		}

		public override UIView CreateThumbnailView ()
		{
			var b = new DirectoryBackgroundView {
				Frame = bg.Frame,
			};

			return b;
		}

		class DirectoryBackgroundView : UIView
		{
			public UIColor GrayColor = UIColor.FromRGB (195, 195, 195);

			public DirectoryBackgroundView ()
			{
				Opaque = true;
				ContentMode = UIViewContentMode.Redraw;
			}

			public override void Draw (CGRect rect)
			{
				base.Draw (rect);

				try {
					GrayColor.SetFill ();
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

	class NotReadyThumbnailCell : UICollectionViewCell
	{
		UIActivityIndicatorView activity;
		UILabel label;

		static readonly bool ios7 = UIDevice.CurrentDevice.CheckSystemVersion (7, 0);

		public NotReadyThumbnailCell (IntPtr handle)
			: base (handle)
		{
			BackgroundColor = UIColor.FromRGB (222, 222, 222);

			var b = ContentView.Bounds;

			var text = "Syncing...";
			var font = ios7 ? UIFont.PreferredBody : UIFont.SystemFontOfSize (UIFont.SystemFontSize);

			var size = text.StringSize (font);

			activity = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.Gray) {
			};
			var af = activity.Frame;
			af.X = (b.Width - size.Width) / 2 - 11 - af.Width;
			af.Y = (b.Height - size.Height)/2;
			activity.Frame = af;

			label = new UILabel (ContentView.Bounds) {
				Text = text,
				Font = font,
				TextColor = UIColor.FromWhiteAlpha (137/255.0f, 1),
				AutoresizingMask = UIViewAutoresizing.FlexibleDimensions,
				Lines = 2,
				TextAlignment = UITextAlignment.Center,
			};

			ContentView.AddSubviews (label, activity);

			activity.StartAnimating ();
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
				return count + 1;
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
				
				row--;
				item = controller.Items [row];
				d = item.Reference;
				
				isDir = d.File.IsDirectory;
				
				id = isDir ? DocumentThumbnailsView.DirId : DocumentThumbnailsView.FileId;
			} catch (Exception ex) {
				Log.Error (ex);				
			}

			var c = (BaseDocumentThumbnailCell)collectionView.DequeueReusableCell (id, indexPath);

			try {
				c.RenameRequested = controller.HandleRenameRequested;
				
				if (isDir) {
					var dirCell = ((DirectoryThumbnailCell)c);
					dirCell.ThumbnailSize = controller.ThumbnailSize;
					dirCell.Item = item;
					dirCell.Editing = controller.Editing;
					dirCell.Selecting = controller.Selecting;
					dirCell.SetDocumentSelected (controller.SelectedDocuments.Contains (d.File), false);
				} else {
					var docCell = ((DocumentThumbnailCell)c);
					docCell.ThumbnailSize = controller.ThumbnailSize;
					docCell.Document = d;
					docCell.Editing = controller.Editing;
					docCell.Selecting = controller.Selecting;
					docCell.SetDocumentSelected (controller.SelectedDocuments.Contains (d.File), false);
				}
			} catch (Exception ex) {
				Log.Error (ex);				
			}

			return c;
		}

		UICollectionViewCell GetSortCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var controller = (DocumentThumbnailsView)collectionView;

			var c = (SortThumbnailCell)collectionView.DequeueReusableCell (DocumentThumbnailsView.SortId, indexPath);
			c.Controller = controller;
			c.Sort = controller.Sort;
			return c;
		}

		UICollectionViewCell GetAddCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var controller = (DocumentThumbnailsView)collectionView;

			var c = (AddDocumentCell)collectionView.DequeueReusableCell (DocumentThumbnailsView.AddId, indexPath);
			c.ThumbnailSize = controller.ThumbnailSize;
			c.Editing = controller.Editing;
			c.Selecting = controller.Selecting;
			c.SetThumbnail ();

			return c;
		}

		UICollectionViewCell GetNotReadyCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var c = (UICollectionViewCell)collectionView.DequeueReusableCell (DocumentThumbnailsView.NotReadyId, indexPath);
			return c;
		}
	}
}





