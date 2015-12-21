using System;
using System.Threading.Tasks;
using UIKit;
using Foundation;
using Praeclarum.IO;
using System.IO;
using System.Linq;
using Praeclarum.App;
using System.Collections.Generic;
using System.Diagnostics;

namespace Praeclarum.UI
{
	public enum DocumentsViewMode
	{
		List,
		Thumbnails,
	}

	[Register ("DocumentsViewController")]
	public class DocumentsViewController : UIViewController, IUIViewControllerPreviewingDelegate
	{
		IDocumentsView docsView;
		IUIViewControllerPreviewing docsPreview;

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

			var settingsImage = UIImage.FromBundle ("Settings.png");
			if (settingsImage != null) {
				thereforeBtn = new UIBarButtonItem (settingsImage, UIBarButtonItemStyle.Plain, HandleLamda);
			} else {
				thereforeBtn = new UIBarButtonItem (appName, UIBarButtonItemStyle.Plain, HandleLamda);
			}

			var theme = appDel.Theme;

			addBtn = theme.CreateAddButton (HandleAdd);
			actionBtn = theme.CreateActionButton (HandleAction);
			deleteBtn = theme.CreateDeleteButton (HandleDelete);
			dupBtn = theme.CreateDuplicateButton (HandleDuplicate);
			moveBtn = theme.CreateMoveButton (HandleMove);
			cancelSelBtn = theme.CreateCancelButton (HandleCancelSelection);
			patronBtn = new UIBarButtonItem ("Support " + appName, UIBarButtonItemStyle.Plain, HandlePatron);

			NavigationItem.BackBarButtonItem = new UIBarButtonItem (
				"Back",
				UIBarButtonItemStyle.Plain,
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

		public void RemoveDocuments (string[] paths, bool animated)
		{
			List<int> indices = new List<int> ();
			foreach (var path in paths) {
				var docIndex = Docs.FindIndex (x => x.File.Path == path);

				if (docIndex < 0 || docIndex >= Docs.Count)
					return;

				indices.Add (docIndex);
			}

			indices.Sort ();
			var offset = 0;

			foreach (var docIndex in indices) {
				Docs.RemoveAt (docIndex - offset);
				items.RemoveAt (docIndex - offset);
				offset++;
			}

			docsView.DeleteItems (indices.ToArray(), animated);
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

			Title = (Editing) ? "" : 
			        (selecting ? "Select a " + DocumentAppDelegate.Shared.App.DocumentBaseName : 
			        (IsRoot ? DocumentAppDelegate.Shared.App.DocumentBaseNamePluralized : DirectoryName));
		}

		public async Task LoadDocs ()
		{
			try {
				await LoadDocsUnsafe ();
			} catch (Exception ex) {
				Console.WriteLine ("LoadDocs Failed: " + ex);
			}
		}

		public Task LoadFolders ()
		{
			return LoadDocs ();
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
					if (fs.ListFilesIsFast) {
//						Console.WriteLine ("Listing subrefs for " + item.Reference.File);				
						item.SubReferences = (from f in await fs.ListFiles (item.Reference.File.Path)
							where !f.IsDirectory
							select new DocumentReference (f, dctor, isNew: false)).ToList ();
					}
					else {
						item.SubReferences = new List<DocumentReference> ();
					}
				}
			}

			//
			// Sort them
			//
			if (SortOrder == DocumentsSort.Date) {

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

			UpdateToolbar (false);
		}

		DocumentsSort SortOrder { get { return docsView != null ? docsView.Sort : DocumentsSort.Name; } }

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
			WhenViewLoaded (() =>
				docsView.SetOpenedDocument (docIndex, animated));
		}

		bool viewLoaded = false;

		List<Action> viewLoadedActions = new List<Action>();
		protected void WhenViewLoaded(Action action) {
			if (viewLoaded) {
				action ();
			} else {
				viewLoadedActions.Add (action);
			}
		}


		#endregion

		UIRefreshControl refresh;
		UIBarButtonItem thereforeBtn;
		readonly UIBarButtonItem addBtn;
		readonly UIBarButtonItem actionBtn;
		readonly UIBarButtonItem deleteBtn;
		readonly UIBarButtonItem dupBtn;
		readonly UIBarButtonItem moveBtn;
		readonly UIBarButtonItem cancelSelBtn;
		readonly UIBarButtonItem patronBtn;

		static readonly TimeSpan SyncTimeout = TimeSpan.FromSeconds (20);
		static readonly TimeSpan RefreshListTimesInterval = TimeSpan.FromSeconds (30);

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			//
			// Create the refresh control
			//
			try {
				refresh = new UIRefreshControl {
					TintColor = UIColor.FromWhiteAlpha (59 / 255.0f, 1),
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
					} finally {
						refresh.EndRefreshing ();
					}
				};
			} catch (Exception ex) {
				Log.Error (ex);				
			}

			//
			// Create our view
			//
			try {
				SwitchToMode (false);
				((UIView)docsView).AddSubview (refresh);				
			} catch (Exception ex) {
				Log.Error (ex);				
			}

			//
			// Set the add button
			//
			try {
				SetNormalNavItems (false);
			} catch (Exception ex) {
				Log.Error (ex);				
			}

			//
			// Load the documents
			//
			try {
				LoadDocs ().ContinueWith (t => {
					if (t.IsFaulted)
						Log.Error (t.Exception);

					BeginInvokeOnMainThread(() => {
						//
						// Do delayed actions
						//
						try {
							viewLoaded = true;
							foreach (var a in viewLoadedActions) {
								a();
							}
							viewLoadedActions.Clear();
						} catch (Exception ex) {
							Log.Error (ex);
						}
					});
				});
			} catch (Exception ex) {
				Log.Error (ex);				
			}

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
				thereforeBtn,
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
				moveBtn.TintColor = UIColor.White;
				deleteBtn.TintColor = UIColor.White;
			}

			NavigationItem.LeftItemsSupplementBackButton = false;
			NavigationItem.SetLeftBarButtonItems (new UIBarButtonItem[] {
				dupBtn,
				moveBtn,
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
			moveBtn.Enabled = docsView.SelectedDocuments.Count > 0;
			deleteBtn.Enabled = docsView.SelectedDocuments.Count > 0;
		}

		void SwitchToMode (bool animated)
		{
			var b = View.Bounds;

			var oldView = docsView;
			var oldPreview = docsPreview;
			if (oldView != null) {
				oldView.SortChanged -= HandleSortChanged;
				oldView.SelectedDocuments.CollectionChanged -= HandleSelectedDocumentsChanged;
				oldView.RenameRequested -= HandleRenameRequested;
				if (ios9 && oldPreview != null) {
					UnregisterForPreviewingWithContext (oldPreview);
				}
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
			if (ios9) {
				docsPreview = RegisterForPreviewingWithDelegate (this, View);
			}
		}

		public UIViewController GetViewControllerForPreview (IUIViewControllerPreviewing previewingContext, CoreGraphics.CGPoint location)
		{
			try {

				if (docsView == null) return null;

				var p = new Praeclarum.Graphics.PointF ((float)location.X, (float)location.Y);
				var item = docsView.GetItemAtPoint (p);

				if (item == null) return null;

				var dref = item.Reference;
				if (dref == null) return null;

				var docIndex = Docs.FindIndex (x => x.File.Path == dref.File.Path);
				if (docIndex < 0) return null;

				var newEditor = DocumentAppDelegate.Shared.App.CreateDocumentEditor (docIndex, Docs);
				if (newEditor == null) return null;

				newEditor.IsPreviewing = true;

				BindEditorAsync (newEditor).ContinueWith (t => {
					if (t.IsFaulted) {
						Log.Error(t.Exception);
					}
				});

				return (UIViewController)newEditor;

			} catch (Exception ex) {
				Log.Error (ex);
				return null;
			}
		}

		static async Task BindEditorAsync (IDocumentEditor newEditor)
		{
			var docRef = newEditor.DocumentReference;
			if (!docRef.IsOpen) {
				await docRef.Open ();
			}
			newEditor.BindDocument ();
		}

		public async void CommitViewController (IUIViewControllerPreviewing previewingContext, UIViewController viewControllerToCommit)
		{
			try {
				var ed = viewControllerToCommit as IDocumentEditor;
				if (ed != null) {
					DocumentAppDelegate.Shared.OpenDocument (ed.DocumentReference.File.Path, false);
				}
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		void HandleRenameRequested (DocumentReference docRef, object arg2)
		{
			var name = docRef.Name;
			var dir = Path.GetDirectoryName (docRef.File.Path);
			var c = new TextInputController {
				Title = "Rename " + (docRef.File.IsDirectory ? "Folder" : DocumentAppDelegate.Shared.App.DocumentBaseName),
				InputText = name,
				ValidateFunc = n => DocumentAppDelegate.ValidateNewName (dir, n, docRef.Name),
			};

			var nc = new UINavigationController (c);
			nc.NavigationBar.BarStyle = DocumentAppDelegate.Shared.Theme.NavigationBarStyle;
			nc.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;

			var presenter = this;

			c.Cancelled += (ss, ee) => presenter.DismissViewController (true, null);
			c.Done += async (ss, ee) => {
				presenter.DismissViewController (true, null);

				if (!string.IsNullOrWhiteSpace (c.InputText) && c.InputText != name) {
					await Rename (docRef, c.InputText);
				}
			};

			presenter.PresentViewController (nc, true, null);
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

				if (item == null)
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
			BeginInvokeOnMainThread (() => SetSelecting (false, true));

			var docRef = this.Docs.FirstOrDefault (x => x.File.Path == filePath);
			if (docRef != null) {

				await DocumentAppDelegate.Shared.PerformActionOnDocument (docRef, this);

			}
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

			var appdel = DocumentAppDelegate.Shared;

			UpdateToolbar (animated);

			//
			// Style the navigation controller
			//
			if (NavigationController != null) {
				NavigationController.SetNavigationBarHidden (false, animated);
				appdel.Theme.Apply (NavigationController);
			}

			//
			// Create the auto time refresher
			//
			refreshTimer = NSTimer.CreateRepeatingScheduledTimer (RefreshListTimesInterval, RefreshListTimes);

			//
			// Update the sort order
			//
			var currentSort = DocumentAppDelegate.Shared.Settings.DocumentsSort;

			if (docsView.Sort != currentSort) {
				forcingSort = true;
				docsView.Sort = currentSort;
				forcingSort = false;
			}

			//
			// Update the view
			//
			await LoadDocs ();

			//
			// Show which doc is open
			//
			SetOpenedDocument (appdel.OpenedDocIndex, animated);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			try {
				SetTitle ();
				
				DocumentAppDelegate.Shared.Settings.SetWorkingDirectory (FileSystemManager.Shared.ActiveFileSystem, Directory);
			} catch (Exception ex) {
				Log.Error (ex);				
			}
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			try {
				if (refreshTimer != null) {
					refreshTimer.Invalidate ();
					refreshTimer = null;
				}
			} catch (Exception ex) {
				Log.Error (ex);				
			}
		}

		public override void WillRotate (UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate (toInterfaceOrientation, duration);

			try {
				docsView.UpdateLayout ();
			} catch (Exception ex) {
				Log.Error (ex);				
			}
		}

		protected virtual void UpdateToolbar (bool animated)
		{
			try {
				var appdel = DocumentAppDelegate.Shared;

				var items = new List<UIBarButtonItem> ();

				var needsPatronBar = false;
				if (appdel.App.IsPatronSupported) {
					needsPatronBar = !appdel.Settings.IsPatron;
				}
				if (needsPatronBar) {
					items.Add (new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace));
					items.Add (patronBtn);
					items.Add (new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace));
				}

				SetToolbarItems (items.ToArray (), animated);			

				if (NavigationController != null) {
					NavigationController.SetToolbarHidden (items.Count == 0, animated);
				}
			} catch (Exception ex) {
				Log.Error (ex);				
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

		void RefreshListTimes (NSTimer timer)
		{
			docsView.RefreshListTimes ();
		}

		void HandleLamda (object sender, EventArgs e)
		{
			DocumentAppDelegate.Shared.ShowSettings (this);
		}

		async void HandlePatron (object sender, EventArgs e)
		{
			await DocumentAppDelegate.Shared.ShowPatronAsync ();
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

		async void HandleMove (object sender, EventArgs e)
		{
			try {
				await DocumentAppDelegate.Shared.MoveDocuments (GetSelectedFiles (), moveBtn, this);
			} catch (Exception ex) {
				DocumentAppDelegate.Alert ("Move Failed", ex);
			}
			SetEditing (false, true);
		}

		async void HandleDuplicate (object sender, EventArgs e)
		{
			try {
				if (await DocumentAppDelegate.Shared.DuplicateDocuments (GetSelectedFiles (), dupBtn)) {
					SetEditing (false, true);
				}
			} catch (Exception ex) {
				DocumentAppDelegate.Alert ("Duplicate Failed", ex);
			}
		}

		async void HandleDelete (object sender, EventArgs e)
		{
			try {
				if (await DocumentAppDelegate.Shared.DeleteDocuments (GetSelectedFiles (), deleteBtn)) {
					SetEditing (false, true);
				}
			} catch (Exception ex) {
				DocumentAppDelegate.Alert ("Delete Failed", ex);
			}
		}

		public override UIStatusBarStyle PreferredStatusBarStyle ()
		{
			try {
				return Editing ? UIStatusBarStyle.LightContent : DocumentAppDelegate.Shared.Theme.StatusBarStyle;
			} catch (Exception ex) {
				Log.Error (ex);
				return UIStatusBarStyle.Default;
			}
		}

		readonly bool ios7 = UIDevice.CurrentDevice.CheckSystemVersion (7, 0);
		readonly bool ios9 = UIDevice.CurrentDevice.CheckSystemVersion (9, 0);

		void SetSpecialNav (bool animated)
		{
			if (ios7) {
//				NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
				NavigationController.NavigationBar.BarTintColor = UIApplication.SharedApplication.KeyWindow.TintColor;
				NavigationController.NavigationBar.TintColor = UIColor.White;
				SetNeedsStatusBarAppearanceUpdate ();
			} else {
//				NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
			}
			NavigationController.SetToolbarHidden (true, animated);
			UpdateEditingForSelection ();
		}

		void SetNormalNav (bool animated)
		{
			if (ios7) {
				var theme = DocumentAppDelegate.Shared.Theme;
//				NavigationController.NavigationBar.BarStyle = UIBarStyle.Default;
				NavigationController.NavigationBar.BarTintColor = theme.NavigationBackgroundColor;
				NavigationController.NavigationBar.TintColor = UIApplication.SharedApplication.KeyWindow.TintColor;
				SetNeedsStatusBarAppearanceUpdate ();
			} else {
//				NavigationController.NavigationBar.BarStyle = UIBarStyle.Default;
			}

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

			try {
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

			} catch (Exception ex) {
				Log.Error (ex);				
			}
		}

		void HandleDone (object sender, EventArgs e)
		{
			try {
				NavigationController.NavigationBar.BarStyle = UIBarStyle.Default;
			} catch (Exception ex) {
				Log.Error (ex);				
			}
		}

		void HandleAdd (object sender, EventArgs e)
		{
			try {
				DocumentAppDelegate.Shared.ShowAddUI (NavigationItem.RightBarButtonItem, dup: false, folder: true);
			} catch (Exception ex) {
				Log.Error (ex);				
			}
		}

		void HandleTitleTap (object sender, EventArgs e)
		{
			if (IsRoot)
				return;

			var name = DirectoryName;
			var dir = Path.GetDirectoryName (this.Directory);

			var c = new TextInputController {
				Title = "Rename",
				InputText = name,
				ValidateFunc = n => DocumentAppDelegate.ValidateNewName (dir, n, name),
			};

			var nc = new UINavigationController (c);
			nc.NavigationBar.BarStyle = DocumentAppDelegate.Shared.Theme.NavigationBarStyle;
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

			var showPatron = false;
			var appdel = DocumentAppDelegate.Shared;
			if (appdel.App.IsPatronSupported) {
				showPatron = !appdel.Settings.IsPatron;
			}

			docsView.IsSyncing = IsSyncing;
			docsView.Items = items;
			docsView.ShowPatron = showPatron;
			docsView.ReloadData ();
		}
	}

	
}
