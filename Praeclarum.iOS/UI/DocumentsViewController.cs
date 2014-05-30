using System;
using System.Threading.Tasks;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Praeclarum.IO;
using System.IO;
using System.Globalization;
using System.Linq;
using Praeclarum.App;
using System.Collections.Generic;
using System.Diagnostics;
using DropBoxSync.iOS;

namespace Praeclarum.UI
{
	public enum DocumentsViewMode
	{
		List,
		Thumbnails,
	}

	[Register ("DocumentsViewController")]
	public class DocumentsViewController : UIViewController
	{
		IDocumentsView docsView;

		DocumentsViewMode viewMode;
		public DocumentsViewMode ViewMode {
			get {
				return viewMode;
			}
			set {
				viewMode = value;
			}
		}

		public DocumentsViewController (string directory, DocumentsViewMode viewMode)
		{
			Docs = new List<DocumentReference> ();

			Directory = directory;

			this.viewMode = viewMode;

			var appDel = DocumentAppDelegate.Shared;
			var appName = appDel.App.Name;

			thereforeBtn = new UIBarButtonItem (appName, UIBarButtonItemStyle.Plain, HandleLamda);
			fileSystemsBtn = new UIBarButtonItem ("Loading Storage...", UIBarButtonItemStyle.Plain, HandleStorage);

			var theme = appDel.Theme;

			addBtn = theme.CreateAddButton (HandleAdd);
			actionBtn = theme.CreateActionButton (HandleAction);
			deleteBtn = theme.CreateDeleteButton (HandleDelete);
			dupBtn = theme.CreateDuplicateButton (HandleDuplicate);
			cancelSelBtn = theme.CreateCancelButton (HandleCancelSelection);

			NavigationItem.BackBarButtonItem = new UIBarButtonItem (
				"Back",
				UIBarButtonItemStyle.Bordered,
				null);			

			SetTitle ();
		}

		#region Document Management

		public string Directory { get; private set; }

		public bool IsRoot
		{
			get {
				return string.IsNullOrEmpty (Directory) || Directory == "/";
			}
		}

		public string DirectoryName
		{
			get {
				return Path.GetFileName (Directory);
			}
		}


		public List<DocumentReference> Docs { get; private set; }
		List<DocumentsViewItem> items = new List<DocumentsViewItem> ();

		public void RemoveDocument (string path, bool animated)
		{
			var docIndex = Docs.FindIndex (x => x.File.Path == path);

			if (docIndex < 0 || docIndex >= Docs.Count)
				return;

			Docs.RemoveAt (docIndex);
			items.RemoveAt (docIndex);
			docsView.DeleteItems (new[] { docIndex }, animated);
		}

		public void UpdateDocument (int docIndex)
		{
			if (docIndex < 0 || docIndex >= Docs.Count)
				return;

			docsView.UpdateItem (docIndex);
		}

		public void InsertDocument (int docIndex, DocumentReference dr)
		{
			if (docIndex < 0)
				return;

			// Watch out in case we already got notified of this insertion
			var existing = docIndex < Docs.Count ? Docs [docIndex] : null;
			if (existing != null && existing.File.Path == dr.File.Path)
				return;

			//
			// Insert it nicely
			//
			Docs.Insert (docIndex, dr);
			items.Insert (docIndex, new DocumentsViewItem (dr));

			try {
				docsView.InsertItems (new[] { docIndex });
			} catch (Exception ex) {
				Debug.WriteLine (ex);				
			}
		}

		void SetTitle ()
		{
			var FileSystem = FileSystemManager.Shared.ActiveFileSystem;

			Title = Editing ? "" : 
			        (selecting ? "Select a " + DocumentAppDelegate.Shared.App.DocumentBaseName : 
			        (IsRoot ? DocumentAppDelegate.Shared.App.DocumentBaseNamePluralized : DirectoryName));

			if (fileSystemsBtn != null) {
				var desc = FileSystem.Description;
				if (desc.Length > 30 || (!ios7 && desc.Length > 16))
					desc = FileSystem.ShortDescription;
				fileSystemsBtn.Title = desc;
			}
		}

		public async Task LoadDocs ()
		{
			try {
				await LoadDocsUnsafe ();
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}

		async Task LoadDocsUnsafe ()
		{
//			Console.WriteLine ("LOAD");

			var fs = FileSystemManager.Shared.ActiveFileSystem;

			SetTitle ();

			DocumentConstructor dctor = DocumentAppDelegate.Shared.App.CreateDocument;

			//
			// Get the items
			//
			var files = await fs.ListFiles (Directory);
			var newItems = (from f in files 
				select new DocumentsViewItem (
					new DocumentReference (f, dctor, isNew: false))).ToList ();
			foreach (var item in newItems) {
				if (item.Reference.File.IsDirectory) {
					item.SubReferences = (from f in await fs.ListFiles (item.Reference.File.Path)
						where !f.IsDirectory
						select new DocumentReference (f, dctor, isNew: false)).ToList ();
				}
			}

			//
			// Sort them
			//
			if (docsView.Sort == DocumentsSort.Date) {

				newItems.Sort ((a, b) => b.ModifiedTime.CompareTo (a.ModifiedTime));

				foreach (var item in newItems) {
					if (item.SubReferences != null) {
						item.SubReferences.Sort ((a, b) => b.ModifiedTime.CompareTo (a.ModifiedTime));
					}
				}

			} else {
				newItems.Sort ((a, b) => string.Compare (a.Reference.Name, b.Reference.Name, StringComparison.OrdinalIgnoreCase));

				foreach (var item in newItems) {
					if (item.SubReferences != null) {
						item.SubReferences.Sort ((a, b) => string.Compare (a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
					}
				}
			}

			items = newItems;
			Docs = newItems.Select (x => x.Reference).ToList ();		

			ReloadData ();
		}

		static async Task<Tuple<IFile, DateTime>> GetModifiedTimeAsync (IFile file)
		{
			var mt = DateTime.MinValue;

			if (file.IsDirectory) {

				var files = await FileSystemManager.Shared.ActiveFileSystem.ListFiles (file.Path);
				var q = from f in files
				        where !f.IsDirectory
				        select f.ModifiedTime;
				try {
					mt = q.Max ();
				} catch {
				}

			} else {
				mt = file.ModifiedTime;
			}

			return Tuple.Create (file, mt);
		}

		public void SetOpenedDocument (int docIndex, bool animated)
		{
			docsView.SetOpenedDocument (docIndex, animated);
		}

		#endregion

		UIRefreshControl refresh;
		UIBarButtonItem thereforeBtn, fileSystemsBtn;
		readonly UIBarButtonItem addBtn;
		readonly UIBarButtonItem actionBtn;
		readonly UIBarButtonItem deleteBtn;
		readonly UIBarButtonItem dupBtn;
		readonly UIBarButtonItem cancelSelBtn;

		static readonly TimeSpan SyncTimeout = TimeSpan.FromSeconds (20);
		static readonly TimeSpan RefreshListTimesInterval = TimeSpan.FromSeconds (30);

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			//
			// Create the refresh control
			//
			refresh = new UIRefreshControl {
				TintColor = UIColor.FromWhiteAlpha (59/255.0f, 1),
			};
			refresh.ValueChanged += async (sim, e) => {
				try {
					var fs = FileSystemManager.Shared.ActiveFileSystem;
					await fs.Sync (SyncTimeout);
					if (fs.IsSyncing)
						ShowSyncError ();
					else
						await LoadDocs ();
				} catch (Exception ex) {
					Debug.WriteLine (ex);					
				}
				finally {
					refresh.EndRefreshing ();
				}
			};

			//
			// Create our view
			//
			SwitchToMode (false);
			((UIView)docsView).AddSubview (refresh);

			//
			// Set the add button
			//
			SetNormalNavItems (false);

			//
			// Set the toobar
			//
			SetToolbarItems (new[] { 
				thereforeBtn,
				new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),
				fileSystemsBtn
			}, false);

			//
			// Load the documents
			//
			LoadDocs ().ContinueWith (t => {
				if (t.IsFaulted)
					Debug.WriteLine (t.Exception);
			});
		}

		void SetNormalNavItems (bool animated)
		{
			if (ios7) {
				var tint = Praeclarum.Graphics.ColorEx.GetUIColor (DocumentAppDelegate.Shared.App.TintColor);

				actionBtn.TintColor = tint;
				addBtn.TintColor = tint;
				EditButtonItem.TintColor = tint;
			}

			NavigationItem.LeftItemsSupplementBackButton = true;
			NavigationItem.SetLeftBarButtonItems (new UIBarButtonItem[] {
				actionBtn,
			}, animated);
			NavigationItem.SetRightBarButtonItems (new UIBarButtonItem[] {
				addBtn,
				EditButtonItem,
			}, animated);
		}

		void SetEditingNavItems (bool animated)
		{
			if (ios7) {
				EditButtonItem.TintColor = UIColor.White;
				dupBtn.TintColor = UIColor.White;
				deleteBtn.TintColor = UIColor.White;
			}

			NavigationItem.LeftItemsSupplementBackButton = false;
			NavigationItem.SetLeftBarButtonItems (new UIBarButtonItem[] {
				dupBtn,
				deleteBtn,
			}, animated);
			NavigationItem.SetRightBarButtonItems (new UIBarButtonItem[] {
				EditButtonItem,
			}, animated);
		}

		void SetSelectingNavItems (bool animated)
		{
			if (ios7) {
				cancelSelBtn.TintColor = UIColor.White;
			}

			NavigationItem.LeftItemsSupplementBackButton = false;
			NavigationItem.SetLeftBarButtonItems (new UIBarButtonItem[] {
				cancelSelBtn,
			}, animated);
			NavigationItem.SetRightBarButtonItems (new UIBarButtonItem[] {
			}, animated);
		}

		void UpdateEditingForSelection ()
		{
			dupBtn.Enabled = docsView.SelectedDocuments.Count > 0;
			deleteBtn.Enabled = docsView.SelectedDocuments.Count > 0;
		}

		void SwitchToMode (bool animated)
		{
			var b = View.Bounds;


			var oldView = docsView;
			if (oldView != null) {
				oldView.SortChanged -= HandleSortChanged;
				oldView.SelectedDocuments.CollectionChanged -= HandleSelectedDocumentsChanged;
				oldView.RenameRequested -= HandleRenameRequested;
			}

			var newView = viewMode == DocumentsViewMode.List ? 
			        (IDocumentsView)new DocumentListView (b) : 
			        (IDocumentsView)new DocumentThumbnailsView (b);



			docsView = newView;
			docsView.IsSyncing = IsSyncing;
			docsView.Items = items;
			docsView.Sort = DocumentAppDelegate.Shared.Settings.DocumentsSort;
			docsView.SortChanged += HandleSortChanged;
			docsView.SelectedDocuments.CollectionChanged += HandleSelectedDocumentsChanged;
			docsView.RenameRequested += HandleRenameRequested;

			var longPress = new UILongPressGestureRecognizer (HandleLongPress) {
				MinimumPressDuration = 0.5,
			};

			View = (UIView)newView;
			View.AddGestureRecognizer (longPress);
		}

		void HandleRenameRequested (DocumentReference docRef, object arg2)
		{
			var name = docRef.Name;

			var c = new TextInputController {
				Title = "Rename " + (docRef.File.IsDirectory ? "Folder" : DocumentAppDelegate.Shared.App.DocumentBaseName),
				InputText = name,
				ValidateFunc = n => DocumentAppDelegate.ValidateNewName (n, docRef.Name),
			};

			var nc = new UINavigationController (c);
			nc.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;

			c.Cancelled += (ss, ee) => nc.DismissViewController (true, null);
			c.Done += async (ss, ee) => {
				nc.DismissViewController (true, null);

				if (!string.IsNullOrWhiteSpace (c.InputText) && c.InputText != name) {
					await Rename (docRef, c.InputText);
				}
			};

			PresentViewController (nc, true, null);
		}

		UIAlertView alert;

		async Task Rename (DocumentReference docRef, string newName)
		{
			try {
				var oldIndex = Docs.FindIndex (x => x.File.Path == docRef.File.Path);

				var r = await docRef.Rename (newName);

				if (r) {

					Console.WriteLine ("RENAME to {0}: {1}", newName, r);

					if (oldIndex >= 0) {
						await LoadDocs ();
						var newIndex = Docs.FindIndex (x => x.File.Path == docRef.File.Path);
						if (newIndex >= 0 && newIndex != oldIndex) {
							docsView.ShowItem (newIndex, true);
						}
					}
				}
				else {
					alert = new UIAlertView ("Failed to Rename", "You may not have permission.", null, "OK");
					alert.Show ();
				}

//				AppDelegate.Shared.UpdateDocListName (docIndex);

			} catch (Exception ex) {
				Debug.WriteLine (ex);
			}
		}

		void HandleLongPress (UILongPressGestureRecognizer gestureRecognizer)
		{
			if (IsSyncing)
				return;

			if (gestureRecognizer.State == UIGestureRecognizerState.Began) {

				var p = gestureRecognizer.LocationInView ((UIView)docsView);

				var item = docsView.GetItemAtPoint (Praeclarum.Graphics.RectangleEx.ToPointF (p));

				if (item == null || item.Reference.File.IsDirectory)
					return;

				if (!Editing && !selecting) {
					SetEditing (true, true);
				}

				docsView.SelectedDocuments.Add (item.Reference.File.Path);
			}
//
//			CGPoint p = [gestureRecognizer locationInView:self.collectionView];
//
//			NSIndexPath *indexPath = [self.collectionView indexPathForItemAtPoint:p];
//			if (indexPath == nil){
//				NSLog(@"				couldn't find index path");            
//			} else {
//				// get the cell at indexPath (the one you long pressed)
//				UICollectionViewCell* cell =
//					[self.collectionView cellForItemAtIndexPath:indexPath];
//				// do stuff with the cell
//			}
		}

		async void HandleSelectedDocumentsChanged (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (Editing) {
				UpdateEditingForSelection ();
				return;
			}
			if (selecting) {
				if (docsView.SelectedDocuments.Count == 1) {
					await PerformActionOnDocument (docsView.SelectedDocuments [0]);
				}
			}
		}

		async Task PerformActionOnDocument (string filePath)
		{
			var docRef = this.Docs.FirstOrDefault (x => x.File.Path == filePath);
			if (docRef != null) {

				await DocumentAppDelegate.Shared.PerformActionOnDocument (docRef, this);

			}

			SetSelecting (false, true);
		}

		bool IsSyncing {
			get {
				var fs = FileSystemManager.Shared.ActiveFileSystem;
				return Docs.Count == 0 && (fs.IsSyncing || fs is EmptyFileSystem);
			}
		}

		bool forcingSort = false;

		async void HandleSortChanged (object sender, EventArgs e)
		{
			if (forcingSort)
				return;

			DocumentAppDelegate.Shared.Settings.DocumentsSort = docsView.Sort;
			await LoadDocs ();
		}

		NSTimer refreshTimer;

		public override async void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if (NavigationController != null) {

				var toolbar = NavigationController.Toolbar;
				var navbar = NavigationController.NavigationBar;

				toolbar.BarStyle = UIBarStyle.Default;
				navbar.BarStyle = UIBarStyle.Default;
				if (ios7) {
					toolbar.BarTintColor = null;
					navbar.BarTintColor = null;
				} else {
					toolbar.TintColor = null;
					navbar.TintColor = null;
				}

				NavigationController.SetToolbarHidden (false, animated);
			}
			refreshTimer = NSTimer.CreateRepeatingScheduledTimer (RefreshListTimesInterval, RefreshListTimes);

			var needsLoad = false;

			var currentSort = DocumentAppDelegate.Shared.Settings.DocumentsSort;

			if (docsView.Sort != currentSort) {
				forcingSort = true;
				docsView.Sort = currentSort;
				forcingSort = false;
				needsLoad = true;
			}

			if (needsLoad) {
				try {
					await LoadDocs ();
				} catch (Exception ex) {
					Debug.WriteLine (ex);
				}
			}

			SetOpenedDocument (DocumentAppDelegate.Shared.OpenedDocIndex, animated);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			SetTitle ();

			DocumentAppDelegate.Shared.Settings.SetWorkingDirectory (FileSystemManager.Shared.ActiveFileSystem, Directory);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			if (refreshTimer != null) {
				refreshTimer.Invalidate ();
				refreshTimer = null;
			}
		}

		static void ShowSyncError ()
		{
			var alert = new UIAlertView (
				"", 
				"Syncing is taking more than " + (int)(SyncTimeout.TotalSeconds+0.5) + " seconds.",
				null,
				"OK");
			alert.Show ();
		}

		void RefreshListTimes ()
		{
			docsView.RefreshListTimes ();
		}

		void HandleLamda (object sender, EventArgs e)
		{
			DocumentAppDelegate.Shared.ShowSettings (this);
		}

		void HandleStorage (object sender, EventArgs e)
		{
			DocumentAppDelegate.Shared.ShowStorage (this);
		}

		void HandleAction (object sender, EventArgs e)
		{
			SetSelecting (true, true);
		}

		void HandleCancelSelection (object sender, EventArgs e)
		{
			SetSelecting (false, true);
		}

		IFile[] GetSelectedFiles ()
		{
			var q = from path in docsView.SelectedDocuments
			        let dr = Docs.FirstOrDefault (x => x.File.Path == path)
				        where dr != null
			        select dr.File;
			return q.ToArray ();
		}

		async void HandleDuplicate (object sender, EventArgs e)
		{
			if (await DocumentAppDelegate.Shared.DuplicateDocuments (GetSelectedFiles (), dupBtn)) {
				SetEditing (false, true);
			}
		}

		async void HandleDelete (object sender, EventArgs e)
		{
			if (await DocumentAppDelegate.Shared.DeleteDocuments (GetSelectedFiles (), deleteBtn)) {
				SetEditing (false, true);
			}
		}

		public override UIStatusBarStyle PreferredStatusBarStyle ()
		{
			return Editing ? UIStatusBarStyle.LightContent : UIStatusBarStyle.Default;
		}

		readonly bool ios7 = UIDevice.CurrentDevice.CheckSystemVersion (7, 0);

		void SetSpecialNav (bool animated)
		{
			if (ios7) {
				NavigationController.NavigationBar.BarTintColor = UIApplication.SharedApplication.KeyWindow.TintColor;
				NavigationController.NavigationBar.TintColor = UIColor.White;
				NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
				SetNeedsStatusBarAppearanceUpdate ();
			} else {
				NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
			}
			UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
			NavigationController.SetToolbarHidden (true, animated);
			UpdateEditingForSelection ();
		}

		void SetNormalNav (bool animated)
		{
			if (ios7) {
				NavigationController.NavigationBar.BarTintColor = null;
				NavigationController.NavigationBar.TintColor = UIApplication.SharedApplication.KeyWindow.TintColor;
				NavigationController.NavigationBar.BarStyle = UIBarStyle.Default;
				SetNeedsStatusBarAppearanceUpdate ();
			} else {
				NavigationController.NavigationBar.BarStyle = UIBarStyle.Default;
			}

			UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.Default;
			NavigationController.SetToolbarHidden (false, animated);
		}

		bool selecting = false;

		void SetSelecting (bool selecting, bool animated)
		{
			if (this.selecting == selecting)
				return;

			this.selecting = selecting;

			if (selecting) {

				SetSpecialNav (animated);

				SetTitle ();

				SetSelectingNavItems (animated);

			} else {

				SetNormalNav (animated);

				SetTitle ();

				SetNormalNavItems (animated);
			}

			docsView.SetSelecting (selecting, animated);
		}

		public override void SetEditing (bool editing, bool animated)
		{
			base.SetEditing (editing, animated);

			if (editing) {

				SetSpecialNav (animated);

				SetTitle ();

				SetEditingNavItems (animated);

			} else {

				SetNormalNav (animated);

				SetTitle ();

				SetNormalNavItems (animated);
			}

			docsView.SetEditing (editing, animated);
		}

		void HandleDone (object sender, EventArgs e)
		{
			NavigationController.NavigationBar.BarStyle = UIBarStyle.Default;
		}

		void HandleAdd (object sender, EventArgs e)
		{
			DocumentAppDelegate.Shared.ShowAddUI (NavigationItem.RightBarButtonItem, dup: false, folder: true);
		}

		void HandleTitleTap (object sender, EventArgs e)
		{
			if (IsRoot)
				return;

			var name = DirectoryName;

			var c = new TextInputController {
				Title = "Rename",
				InputText = name,
				ValidateFunc = n => DocumentAppDelegate.ValidateNewName (n, name),
			};

			var nc = new UINavigationController (c);
			nc.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;

			c.Cancelled += (ss, ee) => nc.DismissViewController (true, null);
			c.Done += async (ss, ee) => {
				nc.DismissViewController (true, null);

				if (c.InputText != name) {

					try {
						var FileSystem = FileSystemManager.Shared.ActiveFileSystem;

						var newDir = Path.Combine (Path.GetDirectoryName (Directory), c.InputText);

						if (await FileSystem.Move (Directory, newDir)) {
							Directory = newDir;
							SetTitle ();
							DocumentAppDelegate.Shared.Settings.SetWorkingDirectory (FileSystem, Directory);
						}
						else {
							var alert = new UIAlertView ("Rename Error", FileSystem.Id + " did not allow the folder to be renamed.", null, "OK");
							alert.Show ();
						}

					} catch (Exception ex) {
						Debug.WriteLine (ex);

					}
				}
			};

			PresentViewController (nc, true, null);
		}

		protected virtual void ReloadData ()
		{
			if (docsView == null)
				return;

			docsView.IsSyncing = IsSyncing;
			docsView.Items = items;
			docsView.ReloadData ();
		}
	}

	
}
