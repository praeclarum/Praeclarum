using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UIKit;
using Foundation;

using Praeclarum.IO;

#if !NO_DROPBOX
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.FileRequests;
using Dropbox.Api.Users;

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
			var session = new DropboxSession (appKey, appSecret, appFolder ? DropboxSessionRoot.App : DropboxSessionRoot.Dropbox);
			DropboxSession.SharedSession = session;
		}

#region IFileSystemProvider implementation

		public Task ShowAddUI (object parent)
		{
			var vc = parent as UIViewController;
			if (vc == null)
				return Task.FromResult<IFileSystem> (null);

			return DropboxSession.SharedSession.LinkFromControllerAsync (vc);
		}

		public IEnumerable<IFileSystem> GetFileSystems ()
		{
			var fss = new List<IFileSystem> ();

			var session = DropboxSession.SharedSession;

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

	public enum DropboxSessionRoot
	{
		App,
		Dropbox,
	}

	public class DropboxSession
	{
		public static DropboxSession SharedSession { get; set; }

		readonly string appSecret;

		public DropboxSessionRoot Root { get; }
		public string AccessToken { get; }

		public bool IsLinked { get; }
		public string AccountId { get; }

		public DropboxSession(string accessToken, string appSecret, DropboxSessionRoot root)
		{
			AccessToken = accessToken;
			this.appSecret = appSecret;
			Root = root;
			AccountId = "";
		}

		public bool HandleOpenUrl (NSUrl url)
		{
			return false;
		}

		public Task LinkFromControllerAsync (UIViewController vc)
		{
			return Task.FromResult (0);
		}
	}

	public class DropboxFileSystem : IFileSystem
	{
		readonly DropboxSession session;
		readonly DropboxClient sharedClient;

		public string UserId { get; private set; }
		public string DisplayName { get; private set; }

		public DropboxFileSystem (DropboxSession session)
		{
			this.session = session;
			sharedClient = new DropboxClient (session.AccessToken);
			UserId = session.AccountId;
			FileExtensions = new System.Collections.ObjectModel.Collection<string> ();
		}

		public DropboxClient GetClient ()
		{
			return sharedClient;
		}

		DropboxFile GetDropboxFile (Metadata meta)
		{
			return new DropboxFile (this, meta);
		}

		public Task<Metadata> DropboxLoadMetadataAsync (string path)
		{
			var c = GetClient ();
			return c.Files.GetMetadataAsync(path);
		}

		public async Task<FileRequest> DropboxLoadFileAsync (string path, string destinationPath)
		{
			var m = await DropboxLoadMetadataAsync(path).ConfigureAwait (false);
			var c = GetClient ();
			return await c.FileRequests.GetAsync(m.AsFile.Id).ConfigureAwait (false);
		}

		public Task<FileMetadata> DropboxUploadFileAsync (string path, string parentRev, string sourcePath)
		{
			var c = GetClient ();
			return Task.Run(async () =>
			{
				using (var inputStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
				{
					return await c.Files.UploadAsync(path, body: inputStream).ConfigureAwait (false);
				}
			});
		}

		//public Task<RestClientFolderCreatedEventArgs> DropboxCreateFolderAsync (string path, string parentRev, string sourcePath)
		//{
		//	var c = GetClient ();
		//	var tcs = new TaskCompletionSource<RestClientFolderCreatedEventArgs> ();
		//	EventHandler<RestClientFolderCreatedEventArgs> onSuccess = null;
		//	EventHandler<RestClientErrorEventArgs> onFail = null;
		//	onSuccess = (s, e) => {
		//		if (e.Folder.Path == path) {
		//			c.FolderCreated -= onSuccess;
		//			c.CreateFolderFailed -= onFail;
		//			tcs.SetResult (e);
		//		}
		//	};
		//	onFail = (s, e) => {
		//		c.FolderCreated -= onSuccess;
		//		c.CreateFolderFailed -= onFail;
		//		var error = e.Error.Description ?? "";
		//		tcs.SetException (new Exception (error));
		//	};
		//	c.FolderCreated += onSuccess;
		//	c.CreateFolderFailed += onFail;
		//	c.CreateFolder (path);
		//	return tcs.Task;
		//}

		public Task<DeleteResult> DropboxDeletePathAsync (string path)
		{
			var c = GetClient ();
			return c.Files.DeleteV2Async(path);
		}

		public Task<RelocationResult> DropboxMovePathAsync (string fromPath, string toPath)
		{
			var c = GetClient ();
			return c.Files.MoveV2Async(fromPath, toPath);
		}

		public Task<BasicAccount> DropboxLoadAccountInfoAsync ()
		{
			var c = GetClient ();
			return c.Users.GetAccountAsync (session.AccountId);
		}

#region IFileSystem implementation

		public bool JustForApp {
			get {
				return session.Root == DropboxSessionRoot.App;
			}
		}

		public event EventHandler FilesChanged;

		public async Task Initialize ()
		{
			try {
				var r = await DropboxLoadAccountInfoAsync ();
				DisplayName = r.Name.DisplayName;
			} catch (Exception ex) {
				Log.Error (ex);
				DisplayName = UserId;
			}
		}

		public bool ListFilesIsFast { get { return false; } }

		public async Task<List<IFile>> ListFiles (string directory)
		{
			var c = GetClient();
			var d = directory;
			if (!d.StartsWith ("/", StringComparison.Ordinal)) {
				d = "/" + d;
			}
			var r = await c.Files.ListFolderAsync (d).ConfigureAwait (false);

			var exts = FileExtensions.Select (x => "." + x).ToDictionary (x => x);

			var res =
				r.Entries.
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
			var meta = await DropboxLoadMetadataAsync (path).ConfigureAwait (false);
			return GetDropboxFile (meta);
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
			}).ConfigureAwait (false);
			await DropboxUploadFileAsync (path, null, src).ConfigureAwait (false);
			return await GetFile (path).ConfigureAwait (false);
		}
		public async Task<bool> CreateDirectory (string path)
		{
			if (!path.StartsWith ("/", StringComparison.Ordinal)) {
				path = "/" + path;
			}
			try {
				await DropboxDeletePathAsync (path).ConfigureAwait (false);
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

		public string Rev { get { return meta.IsFile ? meta.AsFile.Rev : ""; } }

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

		public async Task<bool> Move (string newPath)
		{
			if (await FileSystem.Move (Path, newPath)) {
				try {
					var f = (DropboxFile)await FileSystem.GetFile (newPath);
					meta = f.meta;
					return true;
				} catch (Exception ex) {
					Log.Error (ex);
				}
			}
			return false;
		}

		public string Path { get; }

		public string DropboxPath { get; }

		public bool IsDirectory {
			get {
				return meta.IsFolder;
			}
		}

		public DateTime ModifiedTime {
			get {
				return meta.IsFile ? meta.AsFile.ClientModified : new DateTime (1970, 1, 1);
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

#endif
