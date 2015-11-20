using System;
using Praeclarum.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dropbox.CoreApi.iOS;
using System.Linq;
using UIKit;

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

		public string UserId { get; private set; }

		public DropboxFileSystem (Session session)
		{
			this.session = session;
			UserId = session.UserIds.FirstOrDefault () ?? "Unknown";
			FileExtensions = new System.Collections.ObjectModel.Collection<string> ();
		}

		RestClient GetClient ()
		{
			return new RestClient (session);
		}

		DropboxFile GetDropboxFile (Metadata meta)
		{
			return new DropboxFile (session, meta);
		}

		#region IFileSystem implementation
		public event EventHandler FilesChanged;
		public async Task Initialize ()
		{
		}
		public bool ListFilesIsFast { get { return false; } }
		public async Task<List<IFile>> ListFiles (string directory)
		{
			var c = GetClient ();

			var tcs = new TaskCompletionSource<RestClientMetadataLoadedEventArgs> ();
			EventHandler<RestClientMetadataLoadedEventArgs> onSuccess = null;
			EventHandler<RestClientErrorEventArgs> onFail = null;
			var error = "";
			onSuccess = (s, e) => {
				c.MetadataLoaded -= onSuccess;
				c.LoadMetadataFailed -= onFail;
				tcs.SetResult (e);
			};
			onFail = (s, e) => {
				c.MetadataLoaded -= onSuccess;
				c.LoadMetadataFailed -= onFail;
				error = e.Error.Description ?? "";
				tcs.SetResult (null);
			};
			c.MetadataLoaded += onSuccess;
			c.LoadMetadataFailed += onFail;
			c.LoadMetadata (directory);

			var r = await tcs.Task.ConfigureAwait (false);

			if (r == null) {
				throw new Exception (error);
			}

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

	public class DropboxFile : IFile
	{
		readonly Session session;
		Metadata meta;
		public DropboxFile (Session session, Metadata meta)
		{
			this.session = session;
			this.meta = meta;
		}

		public override string ToString ()
		{
			return Path;
		}

		#region IFile implementation

		public Task<LocalFileAccess> BeginLocalAccess ()
		{
			throw new NotImplementedException ();
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
}

