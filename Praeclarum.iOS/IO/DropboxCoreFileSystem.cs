using System;
using Praeclarum.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dropbox.CoreApi.iOS;
using System.Linq;
using UIKit;
using System.Text;
using System.IO;

namespace Praeclarum.IO
{
	public class DropboxFileSystemProvider : IFileSystemProvider
	{
		public static TaskCompletionSource<DropboxFileSystem> AddCompletionSource {
			get;
			set;
		}

		public DropboxFileSystemProvider (string appKey, string appSecret, bool appFolder)
		{
			var session = new Session (appKey, appSecret, appFolder ? Session.RootAppFolder : Session.RootDropbox);
			Session.SharedSession = session;
		}

		#region IFileSystemProvider implementation

		public Task ShowAddUI (object parent)
		{
			var vc = parent as UIViewController;
			if (vc == null)
				return Task.FromResult<IFileSystem> (null);

			AddCompletionSource = new TaskCompletionSource<DropboxFileSystem> ();
			Session.SharedSession.LinkFromController (vc);
			return AddCompletionSource.Task;
		}

		public IEnumerable<IFileSystem> GetFileSystems ()
		{
			var fss = new List<IFileSystem> ();

			var session = Session.SharedSession;

			if (session != null && session.IsLinked) {
				var fs = new DropboxFileSystem (session);
				fss.Add (fs);
			}

			return fss;
		}

		public string Name {
			get {
				return "Dropbox";
			}
		}

		public bool CanAddFileSystem {
			get {
				return true;
			}
		}

		#endregion
	}

	public class DropboxFileSystem : IFileSystem
	{
		readonly Session session;
		readonly RestClient sharedClient;

		public string UserId { get; private set; }
		public string DisplayName { get; private set; }

		public DropboxFileSystem (Session session)
		{
			this.session = session;
			sharedClient = new RestClient (session);
			UserId = session.UserIds.FirstOrDefault () ?? "Unknown";
			FileExtensions = new System.Collections.ObjectModel.Collection<string> ();
		}

		public RestClient GetClient ()
		{
			return sharedClient;
		}

		DropboxFile GetDropboxFile (Metadata meta)
		{
			return new DropboxFile (this, meta);
		}

		public Task<RestClientMetadataLoadedEventArgs> DropboxLoadMetadataAsync (string path)
		{
			var c = GetClient ();
			var tcs = new TaskCompletionSource<RestClientMetadataLoadedEventArgs> ();
			EventHandler<RestClientMetadataLoadedEventArgs> onSuccess = null;
			EventHandler<RestClientErrorEventArgs> onFail = null;
			onSuccess = (s, e) => {
				if (e.Metadata.Path == path) {
					c.MetadataLoaded -= onSuccess;
					c.LoadMetadataFailed -= onFail;
					tcs.SetResult (e);
				}
			};
			onFail = (s, e) => {
				c.MetadataLoaded -= onSuccess;
				c.LoadMetadataFailed -= onFail;
				var error = e.Error.Description ?? "";
				tcs.SetException (new Exception (error));
			};
			c.MetadataLoaded += onSuccess;
			c.LoadMetadataFailed += onFail;
			c.LoadMetadata (path);
			return tcs.Task;
		}

		public Task<RestClientFileLoadedEventArgs> DropboxLoadFileAsync (string path, string destinationPath)
		{
			var c = GetClient ();
			var tcs = new TaskCompletionSource<RestClientFileLoadedEventArgs> ();
			EventHandler<RestClientFileLoadedEventArgs> onSuccess = null;
			EventHandler<RestClientErrorEventArgs> onFail = null;
			onSuccess = (s, e) => {
				if (e.DestPath == destinationPath) {
					c.FileLoaded -= onSuccess;
					c.LoadFileFailed -= onFail;
					tcs.SetResult (e);
				}
			};
			onFail = (s, e) => {
				c.FileLoaded -= onSuccess;
				c.LoadFileFailed -= onFail;
				var error = e.Error.Description ?? "";
				tcs.SetException (new Exception (error));
			};
			c.FileLoaded += onSuccess;
			c.LoadFileFailed += onFail;
			c.LoadFile (path, destinationPath);
			return tcs.Task;
		}

		public Task<RestClientFileUploadedEventArgs> DropboxUploadFileAsync (string path, string parentRev, string sourcePath)
		{
			var c = GetClient ();
			var tcs = new TaskCompletionSource<RestClientFileUploadedEventArgs> ();
			EventHandler<RestClientFileUploadedEventArgs> onSuccess = null;
			EventHandler<RestClientErrorEventArgs> onFail = null;
			onSuccess = (s, e) => {
				if (e.SrcPath == sourcePath) {
					c.FileUploaded -= onSuccess;
					c.UploadFileFailed -= onFail;
					tcs.SetResult (e);
				}
			};
			onFail = (s, e) => {
				c.FileUploaded -= onSuccess;
				c.UploadFileFailed -= onFail;
				var error = e.Error.Description ?? "";
				tcs.SetException (new Exception (error));
			};
			c.FileUploaded += onSuccess;
			c.UploadFileFailed += onFail;
			c.UploadFile (Path.GetFileName (path), Path.GetDirectoryName (path), parentRev, sourcePath);
			return tcs.Task;
		}

		public Task<RestClientFolderCreatedEventArgs> DropboxCreateFolderAsync (string path, string parentRev, string sourcePath)
		{
			var c = GetClient ();
			var tcs = new TaskCompletionSource<RestClientFolderCreatedEventArgs> ();
			EventHandler<RestClientFolderCreatedEventArgs> onSuccess = null;
			EventHandler<RestClientErrorEventArgs> onFail = null;
			onSuccess = (s, e) => {
				if (e.Folder.Path == path) {
					c.FolderCreated -= onSuccess;
					c.CreateFolderFailed -= onFail;
					tcs.SetResult (e);
				}
			};
			onFail = (s, e) => {
				c.FolderCreated -= onSuccess;
				c.CreateFolderFailed -= onFail;
				var error = e.Error.Description ?? "";
				tcs.SetException (new Exception (error));
			};
			c.FolderCreated += onSuccess;
			c.CreateFolderFailed += onFail;
			c.CreateFolder (path);
			return tcs.Task;
		}

		public Task<RestClientPathDeletedEventArgs> DropboxDeletePathAsync (string path)
		{
			var c = GetClient ();
			var tcs = new TaskCompletionSource<RestClientPathDeletedEventArgs> ();
			EventHandler<RestClientPathDeletedEventArgs> onSuccess = null;
			EventHandler<RestClientErrorEventArgs> onFail = null;
			onSuccess = (s, e) => {
				if (e.Path == path) {
					c.PathDeleted -= onSuccess;
					c.DeletePathFailed -= onFail;
					tcs.SetResult (e);
				}
			};
			onFail = (s, e) => {
				c.PathDeleted -= onSuccess;
				c.DeletePathFailed -= onFail;
				var error = e.Error.Description ?? "";
				tcs.SetException (new Exception (error));
			};
			c.PathDeleted += onSuccess;
			c.DeletePathFailed += onFail;
			c.DeletePath (path);
			return tcs.Task;
		}

		public Task<RestClientPathMovedEventArgs> DropboxMovePathAsync (string fromPath, string toPath)
		{
			var c = GetClient ();
			var tcs = new TaskCompletionSource<RestClientPathMovedEventArgs> ();
			EventHandler<RestClientPathMovedEventArgs> onSuccess = null;
			EventHandler<RestClientErrorEventArgs> onFail = null;
			onSuccess = (s, e) => {
				if (e.From_path == fromPath) {
					c.PathMoved -= onSuccess;
					c.MovePathFailedWithError -= onFail;
					tcs.SetResult (e);
				}
			};
			onFail = (s, e) => {
				c.PathMoved -= onSuccess;
				c.MovePathFailedWithError -= onFail;
				var error = e.Error.Description ?? "";
				tcs.SetException (new Exception (error));
			};
			c.PathMoved += onSuccess;
			c.MovePathFailedWithError += onFail;
			c.MoveFrom (fromPath, toPath);
			return tcs.Task;
		}

		public Task<RestClientAccountInfoLoadedEventArgs> DropboxLoadAccountInfoAsync ()
		{
			var c = GetClient ();
			var tcs = new TaskCompletionSource<RestClientAccountInfoLoadedEventArgs> ();
			EventHandler<RestClientAccountInfoLoadedEventArgs> onSuccess = null;
			EventHandler<RestClientErrorEventArgs> onFail = null;
			onSuccess = (s, e) => {
				c.AccountInfoLoaded -= onSuccess;
				c.LoadAccountInfoFailed -= onFail;
				tcs.SetResult (e);
			};
			onFail = (s, e) => {
				c.AccountInfoLoaded -= onSuccess;
				c.LoadAccountInfoFailed -= onFail;
				var error = e.Error.Description ?? "";
				tcs.SetException (new Exception (error));
			};
			c.AccountInfoLoaded += onSuccess;
			c.LoadAccountInfoFailed += onFail;
			c.LoadAccountInfo ();
			return tcs.Task;
		}

		#region IFileSystem implementation

		public bool JustForApp {
			get {
				return session.Root == Session.RootAppFolder;
			}
		}

		public event EventHandler FilesChanged;

		public async Task Initialize ()
		{
			try {
				var r = await DropboxLoadAccountInfoAsync ();
				DisplayName = r.Info.DisplayName;				
			} catch (Exception ex) {
				Log.Error (ex);
				DisplayName = UserId;
			}
		}

		public bool ListFilesIsFast { get { return false; } }

		public async Task<List<IFile>> ListFiles (string directory)
		{
			var d = directory;
			if (!d.StartsWith ("/", StringComparison.Ordinal)) {
				d = "/" + d;
			}
			var r = await DropboxLoadMetadataAsync (d);

			var exts = FileExtensions.Select (x => "." + x).ToDictionary (x => x);

			var res =
				r.Metadata.Contents.
				Select (GetDropboxFile).
				Where (x => x.IsDirectory || exts.ContainsKey (Path.GetExtension (x.Path))).
				Cast<IFile> ().
				ToList ();
			return res;
		}

		public async Task<IFile> GetFile (string path)
		{
			if (!path.StartsWith ("/", StringComparison.Ordinal)) {
				path = "/" + path;
			}
			var meta = await DropboxLoadMetadataAsync (path);
			return GetDropboxFile (meta.Metadata);
		}
		public async Task<IFile> CreateFile (string path, byte[] contents)
		{
			if (!path.StartsWith ("/", StringComparison.Ordinal)) {
				path = "/" + path;
			}
			var src = await Task.Run(() => {
				var p = Path.GetTempFileName ();
				File.WriteAllBytes (p, contents);
				return p;
			});//.ConfigureAwait (false);
			await DropboxUploadFileAsync (path, null, src);
			return await GetFile (path);
		}
		public async Task<bool> CreateDirectory (string path)
		{
			if (!path.StartsWith ("/", StringComparison.Ordinal)) {
				path = "/" + path;
			}
			try {
				await DropboxDeletePathAsync (path);
				return true;
			} catch (Exception ex) {
				Log.Error (ex);
				return false;
			}
		}
		public async Task<bool> FileExists (string path)
		{
			try {
				var f = await GetFile (path);
				return f != null;
			} catch (Exception) {
				return false;
			}
		}
		public async Task<bool> DeleteFile (string path)
		{
			if (!path.StartsWith ("/", StringComparison.Ordinal)) {
				path = "/" + path;
			}
			try {
				await DropboxDeletePathAsync (path);
				return true;
			} catch (Exception ex) {
				Log.Error (ex);
				return false;
			}
		}
		public async Task<bool> Move (string fromPath, string toPath)
		{
			if (!fromPath.StartsWith ("/", StringComparison.Ordinal)) {
				fromPath = "/" + fromPath;
			}
			if (!toPath.StartsWith ("/", StringComparison.Ordinal)) {
				toPath = "/" + toPath;
			}
			try {
				await DropboxMovePathAsync (fromPath, toPath);
				return true;
			} catch (Exception ex) {
				Log.Error (ex);
				return false;
			}
		}
		public string Id {
			get {
				return "Dropbox";
			}
		}
		public string Description {
			get {
				return IsAvailable ? "Dropbox: " + DisplayName : "Dropbox";
			}
		}
		public string ShortDescription {
			get {
				return "Dropbox";
			}
		}
		public bool IsAvailable {
			get {
				return true;
			}
		}
		public string AvailabilityReason {
			get {
				return "";
			}
		}
		public bool IsSyncing {
			get {
				return false;
			}
		}
		public string SyncStatus {
			get {
				return "Syncing";
			}
		}
		public bool IsWritable {
			get {
				return true;
			}
		}
		public int MaxDirectoryDepth {
			get {
				return short.MaxValue;
			}
		}
		public ICollection<string> FileExtensions {
			get;
			private set;
		}
		#endregion
	}

	class DropboxFile : IFile
	{
		public readonly DropboxFileSystem FileSystem;
		Metadata meta;
		public DropboxFile (DropboxFileSystem fs, Metadata meta)
		{
			this.FileSystem = fs;
			this.meta = meta;
		}

		public override string ToString ()
		{
			return DropboxPath + "@" + Rev;
		}

		public string Rev { get { return meta.Revision; } }

		#region IFile implementation

		public async Task<LocalFileAccess> BeginLocalAccess ()
		{
			if (IsDirectory)
				throw new InvalidOperationException ("Only files permit local access");

			var docsDir = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
			var cachesDir = System.IO.Path.GetFullPath (System.IO.Path.Combine (docsDir, "../Library/Caches"));

			var tmpDir = System.IO.Path.Combine (cachesDir, "DropboxTemp");

			var filename = Path;
			
			var lp = System.IO.Path.Combine (tmpDir, filename);

			FileSystemManager.EnsureDirectoryExists (System.IO.Path.GetDirectoryName (lp));

			await FileSystem.DropboxLoadFileAsync (DropboxPath, lp);

			var data = await Task.Run (() => File.ReadAllText (lp, Encoding.UTF8));

			return new DropboxLocal (this, lp, data);
		}

		public Task<bool> Move (string newPath)
		{
			return FileSystem.Move (Path, newPath);
		}

		public string Path {
			get {
				var p = meta.Path;
				if (p.StartsWith ("/", StringComparison.Ordinal)) {
					p = p.Substring (1);
				}
				return p;
			}
		}

		public string DropboxPath {
			get {
				return meta.Path;
			}
		}

		public bool IsDirectory {
			get {
				return meta.IsDirectory;
			}
		}

		public DateTime ModifiedTime {
			get {
				return (DateTime)meta.LastModifiedDate;
			}
		}

		public bool IsDownloaded {
			get {
				return true;
			}
		}

		public double DownloadProgress {
			get {
				return 1;
			}
		}

		#endregion
	}

	class DropboxLocal : LocalFileAccess
	{
		readonly DropboxFile file;
		readonly string remoteData;

		public DropboxLocal (DropboxFile file, string localPath, string data)
			: base (localPath)
		{
			this.file = file;
			this.remoteData = data;
		}

		public override async Task End ()
		{
			var data = await Task.Run (() => File.ReadAllText (LocalPath, Encoding.UTF8));
			if (data != remoteData) {
				// Get the newest revision
				var newestFile = (DropboxFile)await file.FileSystem.GetFile (file.Path);
				await file.FileSystem.DropboxUploadFileAsync (file.DropboxPath, newestFile.Rev, LocalPath);
			}
		}
	}
}

