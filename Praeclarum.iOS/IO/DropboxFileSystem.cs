using System;
using DropBoxSync.iOS;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using UIKit;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace Praeclarum.IO
{
	public class DropboxFileSystemProvider : IFileSystemProvider
	{
		public string Name { get { return "Dropbox"; } }
		public bool CanAddFileSystem { get { return true; } }
		public Task ShowAddUI (object parent)
		{
			var vc = parent as UIViewController;
			if (vc == null)
				return Task.FromResult<IFileSystem> (null);

			AddCompletionSource = new TaskCompletionSource<IFileSystem> ();
			DBAccountManager.SharedManager.LinkFromController (vc);
			return AddCompletionSource.Task;
		}

		public static TaskCompletionSource<IFileSystem> AddCompletionSource;

		public IEnumerable<IFileSystem> GetFileSystems ()
		{
			return fss.Cast<IFileSystem> ();
		}

		List<DropboxFileSystem> fss;

		public DropboxFileSystemProvider (string key, string secret)
		{
			fss = new List<DropboxFileSystem> ();

			var manager = new DBAccountManager (key, secret);
			DBAccountManager.SharedManager = manager;

			var account = manager.LinkedAccount;
			if (account != null && account.Linked) {
				var dbfs = new DBFilesystem (account);
				var fs = new DropboxFileSystem (account, dbfs);
				fss.Add (fs);
				DBFilesystem.SharedFilesystem = dbfs;
			}
		}
	}

	public class DropboxFileSystem : IFileSystem
	{
		DBAccount account;
		DBFilesystem filesystem;

		public DropboxFileSystem (DBAccount account, DBFilesystem filesystem)
		{
			if (account == null)
				throw new ArgumentNullException ("account");
			if (filesystem == null)
				throw new ArgumentNullException ("filesystem");
			
			this.account = account;
			this.filesystem = filesystem;
			FileExtensions = new Collection<string> ();
		}

		public override string ToString ()
		{
			return Description;
		}

		public event EventHandler FilesChanged;

		public Task Initialize ()
		{
			return Task.FromResult<object> (null);
		}

		public int MaxDirectoryDepth { get { return short.MaxValue; } }

		public async Task<IFile> GetFile (string path)
		{
			var dbpath = new DBPath (path);

			var fileInfo = await filesystem.FileInfoForPathAsync (dbpath);

			return new DropboxFile (fileInfo, filesystem);
		}

		/// <summary>
		/// Overwrites
		/// </summary>
		public async Task<IFile> CreateFile (string path, byte[] contents)
		{
			var dbpath = new DBPath (path);

			// Delete it so that CreateFile overwrites
			try {
				await filesystem.DeletePathAsync (dbpath);				
			} catch (Exception) {
			}

			var file = await filesystem.CreateFileAsync (dbpath);
			if (file == null)
				throw new Exception ("Failed to create file");

			if (contents != null) {
				var r = await file.WriteDataAsync (Foundation.NSData.FromArray (contents));
				if (!r)
					throw new Exception ("Failed to write contents of new file");
			}

			file.Close ();

			var fileInfo = await filesystem.FileInfoForPathAsync (dbpath);

			return new DropboxFile (fileInfo, filesystem);
		}

		public Task<bool> CreateDirectory (string path)
		{
			var dbpath = new DBPath (path);

			return filesystem.CreateFolderAsync (dbpath);
		}

		public Task<bool> Move (string fromPath, string toPath)
		{
			return Task.Run (() => {
				var r = false;
				try {
					DBError err;
					r = filesystem.MovePath (new DBPath (fromPath), new DBPath (toPath), out err);
					Debug.WriteLineIf (!r, err);
				} catch (Exception ex) {
					r = false;
					Debug.WriteLine (ex);				
				}
				return r;
			});
		}

		public string GetLocalPath (string path)
		{
			return path;
		}

		public string Id {
			get {
				return "Dropbox " + account.UserId;
			}
		}

		public string Description {
			get {
				var info = account.Info;
				var userName = (info != null ? info.DisplayName : account.UserId);

				return IsAvailable ? "Dropbox: " + userName : "Dropbox";
			}
		}

		public string ShortDescription { get { return "Dropbox"; } }

		public string UserId { get { return account.UserId; } }

		public bool IsAvailable { get { return true; } }
		public string AvailabilityReason { get { return ""; } }

		public bool IsSyncing {
			get {
				return !IsSyncd;
			}
		}
		public bool IsSyncd {
			get {
				var s = filesystem.Status;
				return filesystem.CompletedFirstSync && (s.IsActive);
			}
		}
		public string SyncStatus { get { return "Syncing"; } }



		public bool IsWritable {
			get {
				return filesystem.CompletedFirstSync;
			}
		}

		public ICollection<string> FileExtensions {
			get;
			private set;
		}

		public async Task<List<IFile>> ListFiles (string directory)
		{
			var exs = FileExtensions.Select (x => "." + x).ToArray ();
//			Console.WriteLine (filesystem.Status);
			var dbPath = new DBPath (directory);
			var dbFiles = await filesystem.ListFolderAsync (dbPath);

			var q = from f in dbFiles
					let n = f.Path.Name
					where !n.StartsWith (".")
					where f.IsFolder || (Array.IndexOf (exs, Path.GetExtension (n)) >= 0)
					select (IFile)new DropboxFile (f, filesystem);

			DropboxDirectoryObserver dbObserver;

			if (!dbObservers.TryGetValue (directory, out dbObserver)) {
				dbObserver = new DropboxDirectoryObserver ();
				filesystem.AddObserverForPathAndChildren (dbObserver, dbPath, () => {
					OnFilesChanged ();
				});
				dbObservers.Add (directory, dbObserver);
			}

			return q.ToList ();
		}

		Dictionary<string, DropboxDirectoryObserver> dbObservers =
			new Dictionary<string, DropboxDirectoryObserver> ();

		class DropboxDirectoryObserver : Foundation.NSObject
		{
		}

		void OnFilesChanged ()
		{
			var ev = FilesChanged;
			if (ev != null)
				ev (this, EventArgs.Empty);
		}

		public async Task<bool> FileExists (string path)
		{
			try {
				var info = await filesystem.FileInfoForPathAsync (new DBPath (path));
				return info != null;
			} catch (Exception) {
				return false;
			}
		}

		public async Task<bool> DeleteFile (string path)
		{
			try {
				var r = await filesystem.DeletePathAsync (new DBPath (path));
//				Console.WriteLine ("DB DELETE {0}", r);
				return r;
			} catch (Exception ex) {
				Debug.WriteLine (ex);
				return false;
			}
		}
	}

	public class DropboxFile : IFile
	{
		DBFileInfo fileInfo;
		DBFilesystem fileSystem;

		public DropboxFile (DBFileInfo fileInfo, DBFilesystem fileSystem)
		{
			if (fileInfo == null)
				throw new ArgumentNullException ("fileInfo");
			if (fileSystem == null)
				throw new ArgumentNullException ("fileSystem");
			this.fileInfo = fileInfo;
			this.fileSystem = fileSystem;
		}

		public override string ToString ()
		{
			return fileInfo.Path.StringValue;
		}

		public bool IsDirectory { get { return fileInfo.IsFolder; } }

		public async Task<LocalFileAccess> BeginLocalAccess ()
		{
			if (IsDirectory)
				throw new InvalidOperationException ("Only files permit local access");

			string data = "";

			await fileLock.WaitAsync ();
			DBFile file = null;
			try {
				//
				// Sometimes we get informed of new files before the files
				// are actually ready to be read. If that's the case then
				// the modified times are different. We try for up to 10 seconds
				// to get the new file.
				//
				var expectedTime = (DateTime)fileInfo.ModifiedTime;

				for (int i = 0; i < 10; i++) {
					file = await fileSystem.OpenFileAsync (fileInfo.Path);
					data = await file.ReadStringAsync ();

					var modTime = (DateTime)file.Info.ModifiedTime;

					if (modTime < expectedTime) {
						file.Close ();
						file = null;
						await Task.Delay (i*100);
					}
					else {
						break;
					}
				}
			} finally {
				if (file != null)
					file.Close ();
				fileLock.Release ();				
			}

			var docsDir = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
			var cachesDir = System.IO.Path.GetFullPath (System.IO.Path.Combine (docsDir, "../Library/Caches"));

			var tmpDir = System.IO.Path.Combine (cachesDir, "DropboxTemp");

			FileSystemManager.EnsureDirectoryExists (tmpDir);

			var filename = fileInfo.Path.Name;
			var lp = System.IO.Path.Combine (tmpDir, filename);

			File.WriteAllText (lp, data, Encoding.UTF8);

			return new DropboxLocal (this, lp, data);

		}

		readonly SemaphoreSlim fileLock = new SemaphoreSlim (1);

		public async Task WriteString (string contents)
		{
			await fileLock.WaitAsync ();
			try {
				var file = await fileSystem.OpenFileAsync (fileInfo.Path);
				await file.WriteStringAsync (contents);
				file.Close ();
				fileInfo = await fileSystem.FileInfoForPathAsync (fileInfo.Path);
			}
			finally {
				fileLock.Release ();
			}
		}

		public async Task<bool> Move (string newPath)
		{
			var newDBPath = new DBPath (newPath);
			var r = await fileSystem.MovePathAsync (fileInfo.Path, newDBPath);
			if (r) {
				fileInfo = await fileSystem.FileInfoForPathAsync (newDBPath);
			}
			return r;
		}

		public string Path {
			get {
				return fileInfo.Path.StringValue;
			}
		}

		public DateTime ModifiedTime {
			get {
				return (DateTime)fileInfo.ModifiedTime;
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
				await file.WriteString (data);
			}
		}
	}
}

