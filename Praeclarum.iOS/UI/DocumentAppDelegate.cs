using System;
using System.Threading.Tasks;
using UIKit;
using Foundation;
using Praeclarum.IO;
using System.IO;
using System.Globalization;
using System.Linq;
using Praeclarum.App;
using System.Collections.Generic;
using System.Diagnostics;
using Dropbox.CoreApi.iOS;

namespace Praeclarum.UI
{
	[Register ("DocumentAppDelegate")]
	public class DocumentAppDelegate : UIApplicationDelegate
	{
		static readonly bool ios7 = UIDevice.CurrentDevice.CheckSystemVersion (7, 0);
		static readonly bool ios9 = UIDevice.CurrentDevice.CheckSystemVersion (9, 0);

		protected UIWindow window;

		protected readonly MRU mru = new MRU ();

		public override UIWindow Window {
			get {
				return window;
			}
			set {
			}
		}

		public DocumentApplication App { get; protected set; }

		public UIColor TintColor
		{
			get {
				return Praeclarum.Graphics.ColorEx.GetUIColor (App.TintColor);
			}
		}

		public static DocumentAppDelegate Shared { get; private set; }

		protected static IFileSystem ActiveFileSystem { get { return FileSystemManager.Shared.ActiveFileSystem; } }

		public static bool IsPhone { get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; } }

		public IDocumentAppSettings Settings {
			get;
			private set;
		}

		protected virtual IDocumentAppSettings CreateSettings ()
		{
			return new DocumentAppSettings (NSUserDefaults.StandardUserDefaults);
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			Shared = this;

			if (App == null) {
				throw new ApplicationException ("You must set the App property before calling FinishedLaunching");
			}

			//
			// Initialize the caches
			//
			try {
				var docsDir = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
				var cachesDir = Path.GetFullPath (Path.Combine (docsDir, "../Library/Caches"));
				ThumbnailCache = new ImageCache (Path.Combine (cachesDir, "Thumbnails"));
			} catch (Exception ex) {
				Log.Error (ex);
			}

			//
			// Pay attention to the culture
			//
			try {
				UpdateCurrentCulture ();
				NSLocale.Notifications.ObserveCurrentLocaleDidChange ((s, e) => UpdateCurrentCulture ());
			} catch (Exception ex) {
				Log.Error (ex);
			}

			//
			// Learn about the device
			//
			try {
				DeviceFileSystemProvider.DeviceName = UIDevice.CurrentDevice.Name;
				//			if (!UIDevice.CurrentDevice.CheckSystemVersion (7, 0)) {
				//				app.SetStatusBarStyle (UIStatusBarStyle.BlackOpaque, false);
				//			}
			} catch (Exception ex) {
				Log.Error (ex);				
			}

			//
			// Load the settings
			//
			try {
				Settings = CreateSettings ();
				Settings.RunCount++;
				//			Console.WriteLine ("RUN COUNT = " + Settings.RunCount);
			} catch (Exception ex) {
				Log.Error (ex);				
			}


			//
			// Apply the theme
			//
			try {
				WhiteTheme.IsModern = ios7;

				UpdateTheme ();

			} catch (Exception ex) {
				Log.Error (ex);				
			}

			//
			// Initialize the file system manager
			//
			try {
				if (Settings.IsFirstRun () && !string.IsNullOrEmpty (App.AutoOpenDocumentPath))
					Settings.LastDocumentPath = App.AutoOpenDocumentPath;
				
				OpenedDocIndex = -1;
				FileSystemManager.Shared = new FileSystemManager ();
				FileSystemManager.Shared.ActiveFileSystem = new EmptyFileSystem {
					Description = "Loading Storage...",
				};
			} catch (Exception ex) {
				Log.Error (ex);				
			}

			//
			// Load the MRU
			//
			mru.InitializeMRU ();

			//
			// Construct the UI
			//
			try {
				var docList = CreateDirectoryViewController ("");
				docListNav = new UINavigationController (docList);
				docListNav.NavigationBar.BarStyle = Theme.NavigationBarStyle;
				docListNav.ToolbarHidden = false;
				Theme.Apply (docListNav);
			} catch (Exception ex) {
				Log.Error (ex);				
			}

			window = new UIWindow (UIScreen.MainScreen.Bounds);

			try {
				SetRootViewController ();
				
				if (ios7) {
					window.TintColor = Praeclarum.Graphics.ColorEx.GetUIColor (App.TintColor);
				}
			} catch (Exception ex) {
				Log.Error (ex);				
			}

			window.MakeKeyAndVisible ();

			var shouldPerformAdditionalDelegateHandling = true;

			UIApplicationShortcutItem scitem = null;
			if (ios9) {
				var scitemKey = UIApplication.LaunchOptionsShortcutItemKey;
				if (launchOptions != null && launchOptions.ContainsKey (scitemKey)) {
					shouldPerformAdditionalDelegateHandling = false;
					scitem = launchOptions [scitemKey] as UIApplicationShortcutItem;
				}
			}

			//
			// Init the file system
			//
			try {
				var uiSync = TaskScheduler.FromCurrentSynchronizationContext ();
				initFileSystemTask = InitFileSystem ();
					
				initFileSystemTask.ContinueWith (async t => {
					if (!t.IsFaulted) {
						App.OnFileSystemInitialized ();
						try {
							CurrentDocumentListController.LoadDocs ().ContinueWith (tt => {
								if (tt.IsFaulted) {
									Debug.WriteLine (tt.Exception);
								}
							});
						} catch (Exception ex) {
							Log.Error (ex);
						}
						try {
							if (scitem != null) {
								await HandleShortcutItemAsync (scitem);
							}
						} catch (Exception ex) {
							Log.Error (ex);
						}
					} else {
						Debug.WriteLine (t.Exception);
					}
				}, uiSync);
			} catch (Exception ex) {
				Log.Error (ex);				
			}

			return shouldPerformAdditionalDelegateHandling;
		}

		Task initFileSystemTask;

		Theme theme = new LightTheme ();
		public Theme Theme { get { return theme; } set { SetTheme (value); } }

		public void UpdateTheme ()
		{
			SetTheme (Settings.DarkMode ? (Theme)new DarkTheme () : new LightTheme ());
		}

		protected virtual void SetTheme (Theme newTheme)
		{
			this.theme = newTheme;
			this.theme.Apply ();
			UpdateFonts ();
		}

		protected UINavigationController docListNav;
		protected UINavigationController detailNav;

		protected virtual void SetRootViewController ()
		{
		}

		protected virtual void UpdateFonts ()
		{
		}

		protected virtual Task OpenUrlAsync (NSUrl url)
		{
			return Task.FromResult (0);
		}

		public override async void PerformActionForShortcutItem (UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
		{
			try {
				await HandleShortcutItemAsync (shortcutItem);
			} catch (Exception ex) {
				Log.Error (ex);
			}
			try {
				completionHandler (true);
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		async Task HandleShortcutItemAsync (UIApplicationShortcutItem scitem)
		{
			switch (scitem.Type) {
			case "new":
				{
					await AddAndOpenNewDocument ();
				}
				break;
			case "open":
				{
					var path = scitem.UserInfo ["path"].ToString ();
					var fsId = scitem.UserInfo ["fsId"].ToString ();
					if (FileSystem.Id != fsId) {
						var fs = FileSystemManager.Shared.ChooseFileSystem (fsId);
						await SetFileSystemAsync (fs, false);
					}
					await OpenDocument (path, false);
				}
				break;
			}
		}

		void OpenPendingUrl (NSTimer obj)
		{
			try {
				OpenPendingUrlAsync ().ContinueWith (t => {
					if (t.IsFaulted)
						Console.WriteLine ();
				});
			} catch (Exception ex) {
				Console.WriteLine ("OpenPendingUrl failed", ex);
			}
		}

		async Task OpenPendingUrlAsync ()
		{
			var url = pendingUrl;
			pendingUrl = null;

			if (url != null) {
				await initFileSystemTask;
				await OpenUrlAsync (url);
			}
		}

		bool uiInitialized = false;
		NSUrl pendingUrl = null;

		public override void DidEnterBackground (UIApplication application)
		{
			try {
				var ed = CurrentDocumentEditor;
				if (ed != null) {
					ed.DidEnterBackground ();
				}
			} catch (Exception ex) {
				Log.Error (ex);				
			}
		}

		public override void WillEnterForeground (UIApplication application)
		{
			try {
				UpdateFonts ();
			} catch (Exception ex) {
				Log.Error (ex);				
			}

			try {
				var ed = CurrentDocumentEditor;
				if (ed != null) {
					ed.WillEnterForeground ();
				}
			} catch (Exception ex) {
				Log.Error (ex);				
			}
		}

		bool HandleDropboxUrl (NSUrl url)
		{
			var session = Session.SharedSession;
			if (session.HandleOpenUrl (url) && session.IsLinked) {
				var fman = FileSystemManager.Shared;
				var fs = fman.FileSystems.OfType<DropboxFileSystem> ().FirstOrDefault (x => x.UserId == session.UserIds.FirstOrDefault ());
				if (fs != null) {
					Debug.WriteLine ("Dropbox: Existing account detected!");
				}
				else {
					Debug.WriteLine ("Dropbox: App linked successfully!");
					fs = new DropboxFileSystem (session);
					FileSystemManager.Shared.Add (fs);
				}
				if (DropboxFileSystemProvider.AddCompletionSource != null) {
					DropboxFileSystemProvider.AddCompletionSource.SetResult (fs);
					DropboxFileSystemProvider.AddCompletionSource = null;
				}
				if (fs.IsAvailable) {
					SetFileSystemAsync (fs, true).ContinueWith (t =>  {
						if (t.IsFaulted) {
							Debug.WriteLine (t.Exception);
						}
					});
				}
				return true;
			}
			return false;
		}

		public override bool OpenUrl (UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			try {
				if (HandleDropboxUrl (url))
					return true;
			} catch (Exception ex) {
				Log.Error (ex);
			}

			try {
				if (url.Scheme == App.UrlScheme || url.Scheme == "file") {
					// Ignore pending operation
					Settings.LastDocumentPath = "";
				
					pendingUrl = url;
					if (uiInitialized) {
						OpenPendingUrl (null);
					}
					return true;
				}
			} catch (Exception ex) {
				Log.Error (ex);				
			}

			return false;
		}

		protected string GetPathFromTitle (string rawTitle)
		{
			var title = (rawTitle??"").Trim ().Replace ("/", "-").Replace ("?", "-").Replace ("*", "-").Replace(":", "-");

			var baseName = Path.GetFileNameWithoutExtension (title);

			var ext = Path.GetExtension (title);

			if (string.IsNullOrEmpty (ext))
				ext = "." + App.DefaultExtension;

			return Path.Combine (
				CurrentDocumentListController.Directory, 
				baseName + ext);
		}

		protected static string GetPathFromRawPath (string rawPath)
		{
			var path = rawPath.Trim ();

			var ext = Path.GetExtension (path);

			if (string.IsNullOrEmpty (ext)) {
				ext = "." + Shared.App.DefaultExtension;
				var baseName = Path.GetFileNameWithoutExtension (path);
				var dir = Path.GetDirectoryName (path);
				return Path.Combine (dir, baseName + ext);
			}

			return path;
		}

		async Task InitializeFileSystemAsync (IFileSystem fs)
		{
			if (fs.FileExtensions.Count > 0)
				return;

			fs.FileExtensions.Clear ();
			foreach (var f in App.FileExtensions) {
				fs.FileExtensions.Add (f);
			}

			await fs.Initialize ();
		}

		public async Task SetFileSystemAsync (IFileSystem newFileSystem, bool animated)
		{
			if (newFileSystem == null || newFileSystem == ActiveFileSystem)
				return;

			Settings.UseCloud = newFileSystem is CloudFileSystem;
			Settings.FileSystem = newFileSystem.Id;

			await InitializeFileSystemAsync (newFileSystem);

			if (ActiveFileSystem != null) {
				ActiveFileSystem.FilesChanged -= HandleFilesChanged;
			}

			FileSystemManager.Shared.ActiveFileSystem = newFileSystem;
			newFileSystem.FilesChanged += HandleFilesChanged;

			await CreateDocListHierarchy (Settings.GetWorkingDirectory (newFileSystem), animated);
		}

		bool firstHierarchy = true;

		async Task CreateDocListHierarchy (string dir, bool animated)
		{
			var fs = FileSystemManager.Shared.ActiveFileSystem;


			if (dir.Length > 0 && dir [0] != '/')
				dir = "/" + dir;

			var dirs = dir.Split (Path.DirectorySeparatorChar);

			var oldVcs = docListNav.ViewControllers.Where (x => !(x is DocumentsViewController)).ToList ();

			var newVcs = new List<UIViewController> ();

			var p = "";

			foreach (var d in dirs) {
				p = Path.Combine (p, d);
				var dlist = CreateDirectoryViewController (p);
				newVcs.Add (dlist);
			}

			newVcs.AddRange (oldVcs);
			docListNav.SetViewControllers (newVcs.ToArray (), animated);

			uiInitialized = true;

			if (firstHierarchy) {
				firstHierarchy = false;

				if (pendingUrl != null) {
					NSTimer.CreateScheduledTimer (0.1, OpenPendingUrl);
				} else {
					await OpenLastDocument ();
				}
			}

		}

		bool openedLastDocument = false;

		async Task OpenLastDocument ()
		{
			var lastPath = Settings.LastDocumentPath;

			if (string.IsNullOrEmpty (lastPath)) {
				openedLastDocument = true;
				return;
			}

			openedLastDocument |= await OpenDocument (lastPath, false);
		}

		public async Task<bool> OpenDocument (string path, bool animated)
		{
			if (string.IsNullOrEmpty (path))
				return false;

			var fileDir = Path.GetDirectoryName (path);
			if (fileDir == "/")
				fileDir = "";

			var dl = CurrentDocumentListController;

			if (dl.Directory != fileDir) {
				await CreateDocListHierarchy (fileDir, animated);
				dl = CurrentDocumentListController;
			}

			// Incase we just created the file, try to refresh for it
			// Still doesn't work for iCloud :-(
			await FileSystem.Sync (TimeSpan.FromSeconds (10));
			await dl.LoadDocs ();

			var i = dl.Docs.FindIndex (x => x.File.Path == path);
			if (i >= 0) {
				await OpenDocument (i, animated);
				return true;
			} else {
				Console.WriteLine ("Could not open '{0}' because it could not be found", path);
			}

			return false;
		}

		public IFileSystem FileSystem {
			get { return ActiveFileSystem; }
		}

		protected string DropboxSyncKey = "";
		protected string DropboxSyncSecret = "";

		async Task InitFileSystem ()
		{
			Console.WriteLine ("Initializing File System");

			var fsman = FileSystemManager.Shared;

			if (!Settings.AskedToUseCloud && CloudFileSystemProvider.CloudAvailable) {
				Settings.UseCloud = await CloudFileSystemProvider.AskToUseCloud ();
				Settings.AskedToUseCloud = true;
			}

			fsman.Add (new CloudFileSystemProvider ());

			fsman.Add (new DeviceFileSystemProvider ());

			try {
				fsman.Add (new DropboxFileSystemProvider (DropboxSyncKey, DropboxSyncSecret));
			} catch (Exception ex) {
				Debug.WriteLine (ex);
			}


			//
			// Choose the active file system
			//
			var lastFS = Settings.FileSystem;

			// Interpret UseCloud for v1.1 users
			if (Settings.UseCloud) {
				lastFS = fsman.FileSystems.OfType<CloudFileSystem> ().First ().Description;
				Settings.UseCloud = false;
			}

			await SetFileSystemAsync (fsman.ChooseFileSystem (lastFS), false);

			//
			// Add the docs
			//
			if (ShouldRestoreDocs ()) {
				await RestoreDocumentation ();
			}

			Console.WriteLine ("File System Initialized");
		}

		public virtual Task RestoreDocumentation ()
		{
			Settings.DocumentationVersion = Version;
			return Task.FromResult<object> (null);
		}

		bool ShouldRestoreDocs ()
		{
			return (Settings.IsFirstRun ()) ||
				(Version != Settings.DocumentationVersion);
		}

		async void HandleFilesChanged (object sender, EventArgs e)
		{
//			Console.WriteLine ("FILES CHANGED");

			foreach (var dl in docListNav.ViewControllers.OfType<DocumentsViewController> ()) {
				await dl.LoadDocs ();
			}

			if (!openedLastDocument) {
				await OpenLastDocument ();
			}
		}

		public string Version { 
			get { return NSBundle.MainBundle.ObjectForInfoDictionary ("CFBundleVersion").ToString (); }
		}

		public DocumentsViewController CurrentDocumentListController {
			get {
				return docListNav.ViewControllers.OfType<DocumentsViewController> ().LastOrDefault ();
			}
		}

		public IDocumentEditor CurrentDocumentEditor {
			get {
				var vcs = (detailNav ?? docListNav).ViewControllers;
				return vcs.OfType<IDocumentEditor> ().LastOrDefault ();
			}
		}

		List<DocumentReference> Docs {
			get {
				var c = CurrentDocumentListController;
				return c != null ? c.Docs : new List<DocumentReference> ();
			}
		}

		string openedDocPath = null;

		public string OpenedDocPath { get { return openedDocPath; } }

		public int OpenedDocIndex
		{
			get {
				if (string.IsNullOrEmpty (openedDocPath))
					return -1;

				for (var i = 0; i < Docs.Count; i++)
					if (Docs [i].File.Path == openedDocPath)
						return i;

				return -1;
			}
			set {
				if (0 <= value && value < Docs.Count)
					openedDocPath = Docs [value].File.Path;
				else
					openedDocPath = null;
			}
		}

		void InvalidateThumbnail (IFile file, bool deleteThumbnail, bool reloadThumbnail)
		{
			if (deleteThumbnail) {
				ThumbnailCache.RemoveImage (GetThumbnailKey (file, Theme), removeFromDisk: true);
			}

			if (reloadThumbnail) {
				var docIndex = Docs.FindIndex (x => x.File.Path == file.Path);
				if (docIndex >= 0) {
					CurrentDocumentListController.UpdateDocument (docIndex);
				}
			}
		}

		public async Task CloseDocumentEditor (IDocumentEditor editor, bool unbindUI, bool deleteThumbnail, bool reloadThumbnail)
		{
			var docRef = editor.DocumentReference;
			var doc = docRef.Document;

			if (doc == null)
				return;

			Debug.WriteLine ("CLOSE " + docRef.File.Path);

			if (!doc.IsOpen)
				return;
			docRef.IsNew = false;

			Settings.LastDocumentPath = "";

			try {
				await docRef.Close ();

				editor.UnbindDocument ();
				if (unbindUI) {
					editor.UnbindUI ();
				}

				InvalidateThumbnail (docRef.File, deleteThumbnail:deleteThumbnail, reloadThumbnail:reloadThumbnail);

			} catch (Exception ex) {
				Debug.WriteLine (ex);
			}
		}

		async Task CloseOpenedDoc (bool reloadThumbnail = true)
		{
			OpenedDocIndex = -1;
			var oldEditor = CurrentDocumentEditor;
			if (oldEditor != null) {
				await CloseDocumentEditor (oldEditor, unbindUI: true, deleteThumbnail: true, reloadThumbnail: reloadThumbnail);
			}
		}

		public bool CanOpen (int docIndex)
		{
			var ds = Docs;

			if (docIndex < 0 || docIndex >= ds.Count)
				return false;

			var docRef = ds [docIndex];
			return !docRef.File.IsDirectory;
		}

		public void OpenDirectory (int docIndex, bool animated)
		{
			var d = Docs [docIndex];

			var dlist = CreateDirectoryViewController (d.File.Path);

			docListNav.PushViewController (dlist, animated);
		}

		protected virtual DocumentsViewController CreateDirectoryViewController (string path)
		{
			return new DocumentsViewController (path, DocumentsViewMode.List);
		}

		public async Task OpenDocument (int docIndex, bool animated)
		{
			if (docIndex == OpenedDocIndex)
				return;

			if (!CanOpen (docIndex))
				return;

			var advance = docIndex > OpenedDocIndex;

			//
			// Close the doc
			//
			await CloseOpenedDoc ();				

			//
			// Create the new editor
			//
			var docRef = Docs [docIndex];
//			Debug.WriteLine ("CREATING EDITOR C");
			var newEditor = App.CreateDocumentEditor (docIndex, Docs);
			if (newEditor == null)
				throw new ApplicationException ("CreateDocumentEditor must return an editor");
			var newEditorVC = (UIViewController)newEditor;

			//
			// Show the doc in the list
			//
			OpenedDocIndex = docIndex;
			CurrentDocumentListController.SetOpenedDocument (docIndex, animated);
			DismissSheetsAndPopovers (animated);

			//
			// Show the editor
			//
			ShowEditor (docIndex, advance, animated, newEditorVC);

			lastOpenTime = DateTime.UtcNow;

			//
			// Load the file
			//
			try {

				await docRef.Open ();
				newEditor.BindDocument ();
				
			} catch (Exception ex) {
				ShowErrorAndExit (ex);
			}

			//
			// Save it to the MRU
			//
			try {
				mru.AddToMRU (FileSystem, docRef);
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		protected virtual void ShowEditor (int docIndex, bool advance, bool animated, UIViewController newEditorVC)
		{
		}

		UIAlertView openErrorAlert;

		void ShowErrorAndExit (Exception exception)
		{
			try {
				Log.Error (exception);

				Exception ex = exception;
				while (ex.InnerException != null)
					ex = ex.InnerException;

				openErrorAlert = new UIAlertView ("Cannot Open", ex.Message, null, "OK");
				openErrorAlert.Clicked +=  async (sender, e) => {
					try {
						await CloseOpenedDoc ();
					} catch (Exception ex2) {
						Log.Error (ex2);						
					}
				};
				openErrorAlert.Show ();

			} catch (Exception exx) {
				Log.Error (exx);
			}
		}

		protected DateTime lastOpenTime = DateTime.UtcNow;

		protected virtual PForm CreateSettingsForm ()
		{
			return new PForm ("Settings");
		}

		public void ShowSettings (UIViewController controller)
		{			
			if (DismissSheetsAndPopovers ()) {
				return;
			}

			var pform = CreateSettingsForm ();

			var n = new UINavigationController (pform);
			n.NavigationBar.BarStyle = Theme.NavigationBarStyle;
			n.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
			controller.PresentViewController (n, true, null);
		}

		public void ShowStorage (UIViewController controller)
		{			
			if (DismissSheetsAndPopovers ()) {
				return;
			}

			var pform = new StorageForm ();

			var n = new UINavigationController (pform);
			n.NavigationBar.BarStyle = Theme.NavigationBarStyle;
			n.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
			controller.PresentViewController (n, true, null);
		}

		string lastClipboard = "";

		protected virtual string GetNewDocumentText ()
		{
			return "";
		}

		public string CurrentDirectory {
			get {
				var c = CurrentDocumentListController;
				return c != null ? c.Directory : "";
			}
		}

		public async Task AddAndOpenNewDocument ()
		{
			var directory = CurrentDirectory;

			await AddAndOpenDocRef (await DocumentReference.New (
				directory, 
				App.DocumentBaseName, 
				App.DefaultExtension,
				ActiveFileSystem, 
				App.CreateDocument,
				GetNewDocumentText ()));
		}

		public bool ShowAddFromClipboard { get; set; }

		public void ShowAddUI (UIBarButtonItem addButton, bool dup, bool folder)
		{
			if (DismissSheetsAndPopovers ()) {
				return;
			}

			var directory = CurrentDocumentListController.Directory;

			if (!ActiveFileSystem.IsWritable)
				return;

			var form = new Form {
				new FormAction (
					"New " + App.DocumentBaseName,
					async () => await AddAndOpenNewDocument ()),
			};

			var pbtext = GetPasteboardText ();
			if (ShowAddFromClipboard && !string.IsNullOrWhiteSpace (pbtext) && lastClipboard != pbtext) {
				lastClipboard = pbtext;
				form.Add (new FormAction (
					"From Clipboard",
					async () => await AddAndOpenDocRef (await DocumentReference.New (
						directory, 
						App.DocumentBaseName, 
						App.DefaultExtension,
						ActiveFileSystem, 
						App.CreateDocument, 
						GetNewDocumentText () + pbtext))));
			}

			var odi = OpenedDocIndex;
			var source = (dup && 0 <= odi && odi < Docs.Count) ?
			             Docs [odi] :
			             null;
			if (source != null) {
				form.Add (new FormAction (
					"Duplicate",
					async () => await AddAndOpenDocRef (await source.Duplicate (ActiveFileSystem))));
			}

			if (folder && GetDirectoryDepth (directory) < ActiveFileSystem.MaxDirectoryDepth) {
				form.Add (new FormAction (
					"Folder",
					AddFolder));
			}

			if (form.Count > 1) {
				form.Title = "Add";
				form.Add (new FormAction ("Cancel"));

				ActionSheet = form.ToActionSheet ();
				ActionSheet.ShowFrom (addButton, true);
			} else {
				((FormAction)form [0]).Execute ();
			}
		}

		static int GetDirectoryDepth (string directory)
		{
			var depth = 0;

			try {
				var path = directory;
				while (!string.IsNullOrEmpty (path)) {
					path = Path.GetDirectoryName (path);
					depth++;
				}
			} catch (Exception ex) {
				Debug.WriteLine (ex);				
			}

			return depth;
		}

		static string GetPasteboardText ()
		{
			try {
				var pb = UIPasteboard.General;
				var ss = pb.Strings;
				if (ss == null || ss.Length < 1)
					return null;
				return ss [0];
			}
			catch (Exception) {
				return null;
			}
		}

		public static async Task<string> ValidateNewName (string t, string existingName = null)
		{
			if (t == existingName)
				return null;
			if (t.IndexOfAny (new[] { '/', '\\', '*', '?' }) >= 0)
				return "Name cannot contain /, \\, ?, nor *.";
			var fs = FileSystemManager.Shared.ActiveFileSystem;
			foreach (var ext in fs.FileExtensions) {
				if (await fs.FileExists (t + "." + ext))
					return "Name already used. Please enter something different.";
			}
			return null;
		}

		void AddFolder ()
		{
			var c = new TextInputController {
				Title = "Create Folder",
				InputText = "",
				Hint = "Enter the name of the new folder.",
				ValidateFunc = n => ValidateNewName (n),
			};

			var presenter = docListNav.TopViewController;

			var nc = new UINavigationController (c);
			nc.NavigationBar.BarStyle = Theme.NavigationBarStyle;
			nc.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;

			c.Cancelled += (sender, e) => nc.DismissViewController (true, null);
			c.Done += async (sender, e) => {
				nc.DismissViewController (true, null);

				try {
					var newDirName = c.InputText;
					var dl = CurrentDocumentListController;
					var path = Path.Combine (dl.Directory, newDirName);

					if (await FileSystem.CreateDirectory (path)) {
						docListNav.PushViewController (CreateDirectoryViewController (path), false);
					}
					else {
						new UIAlertView (
							"Error",
							FileSystem.Id + " did not allow the directory to be created.",
							null,
							"OK").Show ();
					}

				} catch (Exception ex) {
					Debug.WriteLine (ex);
				}
			};

			presenter.PresentViewController (nc, true, null);
		}

		protected async Task OpenPath (string path)
		{
			var fileDir = Path.GetDirectoryName (path);
			if (fileDir == "/")
				fileDir = "";

			var dl = CurrentDocumentListController;

			if (dl.Directory != fileDir) {

				OpenedDocIndex = -1; // Make sure we don't clash indices

				await CreateDocListHierarchy (fileDir, true);
				dl = CurrentDocumentListController;

			}

			// Make sure we loaded some docs
			await dl.LoadDocs ();

			//
			// Hopefully find it in the new list
			//
			var index = dl.Docs.FindIndex (x => x.File.Path == path);

			//
			// If it is, open it
			//
			if (index >= 0) {
				await OpenDocument (index, true);
			} else {
				Console.WriteLine ("Document list does not contain " + path);
			}
		}

		void AddDocRef (DocumentReference dr)
		{
			CurrentDocumentListController.InsertDocument (0, dr);
		}

		public async Task AddAndOpenDocRef (DocumentReference dr)
		{
			var fileDir = Path.GetDirectoryName (dr.File.Path);
			if (fileDir == "/")
				fileDir = "";

			var dl = CurrentDocumentListController;

			if (dl.Directory != fileDir) {

				await CreateDocListHierarchy (fileDir, true);
				dl = CurrentDocumentListController;

			}

			dl.InsertDocument (0, dr);

			await OpenDocument (0, true);
		}

		public async Task<bool> MoveDocuments (IFile[] files, UIBarButtonItem duplicateButton, UIViewController fromViewController)
		{
			if (DismissSheetsAndPopovers ())
				return false;

			if (files.Length == 0)
				return false;

			//
			// Make sure all the file systems are initialized
			//
			foreach (var fs in FileSystemManager.Shared.FileSystems) {
				await InitializeFileSystemAsync (fs);
			}

			//
			// Find where we're supposed to move to
			//
			var tcs = new TaskCompletionSource<bool> ();

			var form = new MoveDocumentsForm {
				Title = "Move " + DescribeFiles (files) + " to",
				AutoCancelButton = true,
				AutoDoneButton = false,
			};
			form.Dismissed += tcs.SetResult;
			var formNav = new UINavigationController (form) {
				ModalPresentationStyle = UIModalPresentationStyle.FormSheet,
			};
			formNav.NavigationBar.BarStyle = Theme.NavigationBarStyle;
			await fromViewController.PresentViewControllerAsync (formNav, true);

			var shouldMove = await tcs.Task;

			if (!shouldMove)
				return false;

			//
			// Skip the move?
			//
			if (ActiveFileSystem == form.FileSystem && CurrentDirectory == form.Directory) {
				return true;
			}

			//
			// Make sure we're not trying to move a directory into its own child
			//
			if (ActiveFileSystem == form.FileSystem) {
				if (files.Any (x => x.IsDirectory && form.Directory.StartsWith (x.Path, StringComparison.Ordinal))) {
					throw new Exception ("You cannot move a folder into one of its subfolders.");
				}
			}

			//
			// Perform the move
			//
			var list = CurrentDocumentListController;
			foreach (var f in files) {
				await MoveDoc (f, list, form.FileSystem, form.Directory, false);
			}

			//
			// Update the UI
			//
			await list.LoadDocs ();

			return true;
		}

		async Task MoveDoc (IFile file, DocumentsViewController listC, IFileSystem dest, string destDir, bool animated)
		{
			await ActiveFileSystem.MoveAsync (file, dest, destDir);
			listC.RemoveDocuments (new[]{file.Path}, animated);
		}

		public async Task<bool> DuplicateDocuments (IFile[] files, UIBarButtonItem duplicateButton)
		{
			if (DismissSheetsAndPopovers ())
				return false;

			if (files.Length == 0)
				return false;

			//
			// If there is only 1 file, just do it
			//
			if (files.Length == 1) {
				await DuplicateFile (files [0]);
				return true;
			}

			//
			// Ask if we should Dup
			//
			var tcs = new TaskCompletionSource<int> ();

			ActionSheet = new UIActionSheet ();
			var msg = "Duplicate " + DescribeFiles (files);
			ActionSheet.AddButton (msg);
			ActionSheet.AddButton ("Cancel");
			ActionSheet.CancelButtonIndex = 1;
			ActionSheet.Clicked += (ss, se) => {
				try {
					tcs.SetResult ((int)se.ButtonIndex);
				} catch (Exception ex) {
					Log.Error (ex);					
				}
			};

			ActionSheet.ShowFrom (duplicateButton, true);

			var button = await tcs.Task;

			if (button != 0)
				return false;

			//
			// Perform the dup
			//
			foreach (var p in files) {
				await DuplicateFile (p);
			}

			return true;
		}

		string DescribeFiles (IFile[] files)
		{
			var msg = "";
			if (files.Length == 1) {
				msg = files [0].IsDirectory ? "Folder" : App.DocumentBaseName;
			} else {
				var ndir = files.Count (x => x.IsDirectory);
				var ndoc = files.Length - ndir;
				var head = "";
				if (ndoc > 0) {
					msg = ndoc + " " + (ndoc > 1 ? App.DocumentBaseNamePluralized : App.DocumentBaseName);
					head = ", ";
				}
				if (ndir > 0) {
					msg += head + ndir + (ndir > 1 ? " Folders" : " Folder");
				}
			}
			return msg;
		}

		public async Task<bool> DeleteDocuments (IFile[] files, UIBarButtonItem deleteButton)
		{
			if (DismissSheetsAndPopovers ())
				return false;

			if (files.Length == 0)
				return false;

			//
			// Ask if we should delete
			//
			var tcs = new TaskCompletionSource<int> ();

			ActionSheet = new UIActionSheet ();
			ActionSheet.AddButton ("Delete " + DescribeFiles (files));
			ActionSheet.AddButton ("Cancel");
			ActionSheet.DestructiveButtonIndex = 0;
			ActionSheet.CancelButtonIndex = 1;
			ActionSheet.Clicked += (ss, se) => {
				try {
					tcs.SetResult ((int)se.ButtonIndex);
				} catch (Exception ex) {
					Log.Error (ex);					
				}
			};

			ActionSheet.ShowFrom (deleteButton, true);

			var button = await tcs.Task;

			if (button != 0)
				return false;

			//
			// Perform the delete
			//
			try {
				await DeleteDocs (files.Select(x => x.Path).ToArray());
				foreach (var f in files) {
					InvalidateThumbnail (f, deleteThumbnail: true, reloadThumbnail: false);
				}	
			} catch (Exception ex) {
				Console.WriteLine (ex);	
			}

			return true;
		}

		public void DeleteOpenedDocument (UIBarButtonItem deleteButton)
		{
			if (DismissSheetsAndPopovers ())
				return;

			var docIndex = OpenedDocIndex;

			var docRef = Docs [docIndex];

			ActionSheet = new UIActionSheet ();
			ActionSheet.AddButton ("Delete " + App.DocumentBaseName);
			ActionSheet.AddButton ("Cancel");
			ActionSheet.DestructiveButtonIndex = 0;
			ActionSheet.CancelButtonIndex = 1;

			ActionSheet.Clicked += async (ss, se) => {

				if (se.ButtonIndex != 0) return;

				if (docIndex >= 0) {
					await CloseOpenedDoc (reloadThumbnail: false);
				}

				await DeleteDocs (new[]{docRef.File.Path});

				if (Docs.Count > 0) {
					var newIndex = Math.Min (Docs.Count - 1, Math.Max (0, docIndex));
					await OpenDocument (newIndex, true);
				} else if (FileSystem.IsWritable) {
					await AddAndOpenDocRef (await DocumentReference.New (
						CurrentDocumentListController.Directory,
						App.DocumentBaseName,
						App.DefaultExtension,
						ActiveFileSystem,
						App.CreateDocument
					));
				}
			};

			ActionSheet.ShowFrom (deleteButton, true);
		}

		async Task DuplicateFile (IFile file)
		{
			try {
				var newFile = await ActiveFileSystem.DuplicateAsync (file);
				if (newFile != null) {
					AddDocRef (new DocumentReference (newFile, App.CreateDocument, isNew:false));
				}
			} catch (Exception ex) {
				Alert ("Failed to Duplicate " + file.Path, ex);
			}
		}

		async Task DeleteDocs (string[] paths)
		{
			List<string> delPaths = new List<string> ();
			foreach (var path in paths) {
				try {

					var deleted = await FileSystem.DeleteFile (path);

					if (deleted) {
						delPaths.Add(path);
						Console.WriteLine ("DELETE {0}", path);
					} else {
						var alert = new UIAlertView (
							           "Unable to Delete", 
							           "An error occured while trying to delete. If the problem persists, try restarting " + App.Name + ".",
							           null, "OK");
						alert.Show ();
					}

				} catch (Exception ex) {
					Console.WriteLine (ex);
				}
			}

			CurrentDocumentListController.RemoveDocuments (delPaths.ToArray(), true);

		}

		public async Task UpdateDocListAsync ()
		{
			await CurrentDocumentListController.LoadDocs ();
		}

		public void UpdateDocListName (int docIndex)
		{
			if (docIndex < 0 || docIndex >= Docs.Count)
				return;

			CurrentDocumentListController.UpdateDocument (docIndex);
		}

		public UIActionSheet ActionSheet { get; set; }
		public UIPopoverController Popover { get; set; }

		public bool DismissSheetsAndPopovers (bool animated = true)
		{
			var r = false;
			if (ActionSheet != null && ActionSheet.Visible) {
				ActionSheet.DismissWithClickedButtonIndex (ActionSheet.CancelButtonIndex, animated);
				r = true;
			}
			if (Popover != null && Popover.PopoverVisible) {
				Popover.Dismiss (animated);
				r = true;
			}
			return r;
		}

		public static CultureInfo CurrentCulture = CultureInfo.InvariantCulture;

		static void UpdateCurrentCulture ()
		{
			CultureInfo cult = null;

			var current = NSLocale.CurrentLocale;
			var id = current.LocaleIdentifier;

			try {
				var fullName = id.Replace ('_', '-');
				cult = CultureInfo.GetCultureInfo (fullName);
			} catch (Exception) {
				cult = null;
			}

			if (cult == null) {
				try {
					var justRegionFormat = id.Substring (id.IndexOf ('_') + 1);
					cult = CultureInfo.GetCultureInfo (justRegionFormat);
				} catch (Exception) {
					cult = null;
				}
			}

			if (cult == null) {
				Debug.WriteLine ("Failed to recognize locale: " + id);
				cult = CultureInfo.InvariantCulture;
			}

			Console.WriteLine ("Culture set to: " + cult);

			CurrentCulture = cult;
		}

		#region Quick UI stuff

		public static void Alert (string title, Exception ex)
		{
			Console.WriteLine (ex);
			var v = new UIAlertView (
				        title,
				        ex.Message,
				        null,
				        "OK");
			v.Show ();
		}

		#endregion

		#region Thumbnails

		public ImageCache ThumbnailCache { get; private set; }

		public virtual string GetThumbnailKey (IFile file, Theme theme)
		{
			if (file == null)
				return "";

			return string.Format ("{0}-{1}{2}",
				FileSystemManager.Shared.ActiveFileSystem.Id,
				file.Path.Replace ('/', '-').Replace ('.', '-'),
				theme.IsDark ? "-Dark14" : "");
		}

		public virtual async Task<UIImage> GenerateThumbnailAsync (DocumentReference docRef, Praeclarum.Graphics.SizeF size, Theme theme)
		{
			UIImage r = null;

			IDocument doc = null;
			LocalFileAccess local = null;
			var opened = false;

			//
			// Draw the document
			//
			try {
				local = await docRef.File.BeginLocalAccess ();
				var f = local.LocalPath;

				doc = App.CreateDocument (f);
				await doc.OpenAsync ();
				opened = true;

//				Console.WriteLine ("GenerateThumbnail: " + docRef.File.Path + " " + docRef.File.ModifiedTime);

				r = await GenerateDocumentThumbnailAsync (doc, size, theme);

			} catch (Exception ex) {
				Debug.WriteLine ("FAILED to genenerate thumbnail for {0}, {1}", docRef.File.Path, ex.Message);
//				Debug.WriteLine (ex);
			}

			if (opened) {
				try {
					await doc.CloseAsync ();					
				} catch (Exception ex) {
					Console.WriteLine (ex);
				}
			}

			if (local != null) {
				try {
					await local.End ();
				} catch (Exception ex) {
					Console.WriteLine (ex);
				}
			}

			return r;
		}

		async Task<UIImage> GenerateDocumentThumbnailAsync (IDocument s, Praeclarum.Graphics.SizeF size, Theme theme)
		{
			var scale = UIScreen.MainScreen.Scale;
			return await Task.Run (() => {

				var width = (int)(size.Width * scale);
				var height = (int)(size.Height * scale);

				using (var colorSpace = CoreGraphics.CGColorSpace.CreateDeviceRGB ()) {
					using (var c = new CoreGraphics.CGBitmapContext (
						              IntPtr.Zero,
						              width,
						              height,
						              8,
						              4 * width,
						              colorSpace,
						              CoreGraphics.CGImageAlphaInfo.NoneSkipFirst)) {

						c.TranslateCTM (0, height);
						c.ScaleCTM (scale, -scale);

						var g = new Praeclarum.Graphics.CoreGraphicsGraphics (c, true);
						App.DrawThumbnail (s, g, size, theme);

						//
						// Create the bitmap
						//
						return UIImage.FromImage (c.ToImage (), scale, UIImageOrientation.Up);
					}
				}
			});
		}

		#endregion

		#region Actions

		public async Task PerformActionOnDocument (DocumentReference docRef, UIViewController fromController)
		{
			try {
				
				if (docRef == null)
					return;

				var ad = this;

				if (ad.DismissSheetsAndPopovers ())
					return;

				NSObject[] items = new NSObject[0];
				UIActivity[] aa = new UIActivity[0];

				try {

					var d = (await docRef.Open ()) as TextDocument;

					if (d != null) {
						items = await d.GetActivityItemsAsync (fromController);
						aa = await d.GetActivitiesAsync (fromController);
					}

					await docRef.Close ();
									
				} catch (Exception ex) {
					Debug.WriteLine (ex);
				}

				if (items.Length > 0) {
					var tcs = new TaskCompletionSource<bool> ();
					var a = new UIActivityViewController (items, aa);
					a.ModalPresentationStyle = UIModalPresentationStyle.Popover;

					a.CompletionHandler = (x,success) => {
						Console.WriteLine ("COMPLETE {0} {1}", x, success);
						tcs.SetResult (success);
					};

					if (UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) {
						if (a.PopoverPresentationController != null) {
							try {
								a.PopoverPresentationController.BarButtonItem = fromController.NavigationItem.LeftBarButtonItem;
							} catch (Exception ex) {
								a.PopoverPresentationController.SourceView = fromController.View;
							}
						}
					}

					fromController.PresentViewController (a, true, null);

					await tcs.Task;
				}

			} catch (Exception ex) {
				Console.WriteLine ("Perform Act of Doc Failed: " + ex);
			}

		
		}

		#endregion

		#region MRU

		protected class MRU
		{
			List<Tuple<string,string>> entries = new List<Tuple<string, string>> ();

			public void InitializeMRU ()
			{
				if (ios9) {
					entries =
						UIApplication.SharedApplication.ShortcutItems
							.Where (i => i.Type == "open" && i.UserInfo != null)
							.Select (scitem => {
								var path = scitem.UserInfo ["path"].ToString ();
								var fsId = scitem.UserInfo ["fsId"].ToString ();
								return Tuple.Create (fsId, path);
						}).ToList ();
				}
			}

			public void AddToMRU (IFileSystem fs, DocumentReference docRef)
			{
				var key = Tuple.Create (fs.Id, docRef.File.Path);

				var i = entries.IndexOf (key);

				if (i == 0) {
					// OK
					return;
				} else if (i > 0) {
					entries.RemoveAt (i);
				}
				entries.Insert (0, key);


				Console.WriteLine ("SAVE MRU {0}", entries);

				if (ios9) {
					var icon = UIApplicationShortcutIcon.FromType (UIApplicationShortcutIconType.Compose);
					var newItems = entries.Take (3).Select (e => {
						var fsId = e.Item1;
						var path = e.Item2;
						var name = System.IO.Path.GetFileNameWithoutExtension (path);
						var userInfo = new NSDictionary<NSString, NSObject> (
							keys: new[] {new NSString("fsId"), new NSString("path")},
							values: new NSObject[] {new NSString(fsId), new NSString(path)});						
						var item = new UIMutableApplicationShortcutItem("open", "Open " + name, "", icon, userInfo);
						return item;
					}).ToArray ();
					UIApplication.SharedApplication.ShortcutItems = newItems;
				}
			}
		}

		#endregion
	}
}

