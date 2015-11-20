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

		#region IFileSystem implementation
		public event EventHandler FilesChanged;
		public async Task Initialize ()
		{
		}

		public bool ListFilesIsFast { get { return false; } }

		public Task<RestClientMetadataLoadedEventArgs> LoadMetadataAsync (string directory)
		{
			var c = GetClient ();
			var tcs = new TaskCompletionSource<RestClientMetadataLoadedEventArgs> ();
			EventHandler<RestClientMetadataLoadedEventArgs> onSuccess = null;
			EventHandler<RestClientErrorEventArgs> onFail = null;
			onSuccess = (s, e) => {
				if (e.Metadata.Path == directory) {
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
			c.LoadMetadata (directory);
			return tcs.Task;
		}

		public Task<RestClientFileLoadedEventArgs> LoadFileAsync (string path, string destinationPath)
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

		public Task<RestClientFileUploadedEventArgs> UploadFileAsync (string path, string parentRev, string sourcePath)
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

		public async Task<List<IFile>> ListFiles (string directory)
		{
			var d = directory;
			if (!d.StartsWith ("/", StringComparison.Ordinal)) {
				d = "/" + d;
			}
			var r = await LoadMetadataAsync (d);//.ConfigureAwait (false);
			var res = r.Metadata.Contents.Select (GetDropboxFile).Cast<IFile> ().ToList ();
			return res;
		}

		public Task<IFile> GetFile (string path)
		{
			throw new NotImplementedException ();
		}
		public Task<IFile> CreateFile (string path, byte[] contents)
		{
			throw new NotImplementedException ();
		}
		public Task<bool> CreateDirectory (string path)
		{
			throw new NotImplementedException ();
		}
		public Task<bool> FileExists (string path)
		{
			throw new NotImplementedException ();
		}
		public Task<bool> DeleteFile (string path)
		{
			throw new NotImplementedException ();
		}
		public Task<bool> Move (string fromPath, string toPath)
		{
			throw new NotImplementedException ();
		}
		public string Id {
			get {
				return "Dropbox";
			}
		}
		public string Description {
			get {
				return IsAvailable ? "Dropbox: " + UserId : "Dropbox";
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
			return Path + "@" + Rev;
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
			if (filename.StartsWith ("/", StringComparison.Ordinal)) {
				filename = filename.Substring (1);
			}
			var lp = System.IO.Path.Combine (tmpDir, filename);

			FileSystemManager.EnsureDirectoryExists (System.IO.Path.GetDirectoryName (lp));

			await FileSystem.LoadFileAsync (Path, lp);

			var data = await Task.Run (() => File.ReadAllText (lp, Encoding.UTF8));//.ConfigureAwait (false);

			return new DropboxLocal (this, lp, data);
		}

		public Task<bool> Move (string newPath)
		{
			throw new NotImplementedException ();
		}

		public string Path {
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
				await file.FileSystem.UploadFileAsync (file.Path, file.Rev, LocalPath);
			}
		}
	}
}

