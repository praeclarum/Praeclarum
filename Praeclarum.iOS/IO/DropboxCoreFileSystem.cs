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

		public DropboxFileSystemProvider (string appKey, string appSecret)
		{
			// Create a new Dropbox Session, choose the type of access that your app has to your folders.
			// Session.RootAppFolder = The app will only have access to its own folder located in /Applications/AppName/
			// Session.RootDropbox = The app will have access to all the files that you have granted permission
			var session = new Session (appKey, appSecret, Session.RootDropbox);
			// The session that you have just created, will live through all the app
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
		public string UserId { get; private set; }

		public DropboxFileSystem (Session session)
		{
			UserId = session.UserIds.FirstOrDefault () ?? "Unknown";
			FileExtensions = new System.Collections.ObjectModel.Collection<string> ();
		}
		#region IFileSystem implementation
		public event EventHandler FilesChanged;
		public async Task Initialize ()
		{
		}
		public Task<List<IFile>> ListFiles (string directory)
		{
			throw new NotImplementedException ();
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
}

