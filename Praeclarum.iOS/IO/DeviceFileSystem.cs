using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Praeclarum.IO
{
	public class DeviceFileSystemProvider : IFileSystemProvider
	{
		public string Name { get; private set; }
		public bool CanAddFileSystem { get { return false; } }
		public Task ShowAddUI (object parent)
		{
			return Task.FromResult<object> (null);
		}
		public IEnumerable<IFileSystem> GetFileSystems ()
		{
			yield return new DeviceFileSystem ();
		}
		public static string DeviceName { get; set; }
		static DeviceFileSystemProvider ()
		{
			DeviceName = "Device";
		}
		public DeviceFileSystemProvider ()
		{
//			Name = "Device";

			Name = DeviceName;
		}
	}

	public class DeviceFileSystem : IFileSystem
	{
		readonly string documentsPath;

		public ICollection<string> FileExtensions { get; private set; }

		public DeviceFileSystem ()
		{
			FileExtensions = new Collection<string> ();
			documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);

			Description = DeviceFileSystemProvider.DeviceName;
		}
		public override string ToString ()
		{
			return Description;
		}
		public string Id {
			get {
				return "Device";
			}
		}

		public int MaxDirectoryDepth { get { return short.MaxValue; } }

		public string Description { get; private set; }
		public string ShortDescription { get { return Description; } }

		public bool IsWritable { get { return true; } }

		public bool JustForApp { get { return true; } }

		public bool IsAvailable { get { return true; } }
		public string AvailabilityReason { get { return ""; } }

		public bool IsSyncing { get { return false; } }
		public string SyncStatus { get { return ""; } }

		public event EventHandler FilesChanged;

		public Task<IFile> GetFile (string path)
		{
			var file = new DeviceFile (path, documentsPath);
			return Task.FromResult<IFile> (file);
		}

		public bool ListFilesIsFast { get { return true; } }

		public Task<List<IFile>> ListFiles (string directory)
		{
			var dirPath = Path.Combine (documentsPath, directory);

			var len = documentsPath.Length;
			if (!documentsPath.EndsWith ("/", StringComparison.Ordinal))
				len++;

			return Task.Run (() => {

//				Console.WriteLine ("DELAYING LIST FILES {0}", directory);
//				System.Threading.Thread.Sleep (3000);

				var fs = new List<string> ();
				foreach (var ex in FileExtensions) {
					fs.AddRange (Directory.GetFiles (dirPath, "*." + ex));
				}
				fs.AddRange (from d in Directory.GetDirectories (dirPath) 
							let fn = Path.GetFileName (d)
							where fn.Length > 0 && fn[0] != '.' && !fn.Contains ("Temp")
				             select d);

				return fs.
						Select (x => new DeviceFile (x.Substring (len), documentsPath)).
						Cast<IFile> ().
						ToList ();
			});
		}

		public Task<bool> FileExists (string path)
		{
			var localPath = GetLocalPath (path);
			return Task.Run (() => File.Exists (localPath) || Directory.Exists (localPath));
		}

		public Task Initialize ()
		{
			return Task.FromResult<object> (null);
//			Console.WriteLine ("DELAYING INITIALIZE");
//			await Task.Delay (TimeSpan.FromSeconds (5));
		}

		public string GetLocalPath (string path)
		{
			return Path.Combine (documentsPath, path);
		}

		public Task<IFile> CreateFile (string path, byte[] contents)
		{
			return Task.Run<IFile> (() => {
				var r = new DeviceFile (path, documentsPath);
				if (contents != null) {
					var dir = Path.GetDirectoryName (r.LocalPath);
					if (!Directory.Exists (dir))
						Directory.CreateDirectory (dir);
					File.WriteAllBytes (r.LocalPath, contents);
				}
				return r;
			});
		}

		public Task<bool> CreateDirectory (string path)
		{
			return Task.Run (() => {
				try {
					var dir = GetLocalPath (path);
					if (!Directory.Exists (dir))
						Directory.CreateDirectory (dir);
					return true;
				} catch (Exception ex) {
					Debug.WriteLine (ex);
					return false;
				}
			});
		}

		public Task<bool> Move (string fromPath, string toPath)
		{
			return Task.Run (() => {
				try {
					var fromLocalPath = GetLocalPath (fromPath);
					var toLocalPath = GetLocalPath (toPath);

					if (Directory.Exists (fromLocalPath)) {
						Directory.Move (fromLocalPath, toLocalPath);
					}
					else {
						File.Move (fromLocalPath, toLocalPath);
					}
					return true;
				} catch (Exception ex) {
					Debug.WriteLine (ex);
					return false;
				}
			});
		}

		public Task<bool> DeleteFile (string path)
		{
			var localPath = Path.Combine (documentsPath, path);

			return Task.Factory.StartNew (() => {
				try {
					if (Directory.Exists (localPath)) {
						Directory.Delete (localPath, true);
					}
					else if (File.Exists (localPath)) {
						File.Delete (localPath);
					}
					return true;
				} catch (Exception ex) {
					Console.WriteLine (ex);
					return false;
				}
			});
		}
	}

	public class DeviceFile : IFile
	{
		readonly string documentsPath;

		public string Path { get; private set; }
		public string LocalPath { get; private set; }
		public bool IsDownloaded { get { return true; } }
		public double DownloadProgress { get { return 1; } }
		public DateTime ModifiedTime { get; private set; }

		public bool IsDirectory { get; private set; }

		public Task<LocalFileAccess> BeginLocalAccess ()
		{
			return Task.FromResult (new LocalFileAccess (LocalPath));
		}

		public Task<bool> Move (string newPath)
		{
			return Task.Factory.StartNew (() => {
				try {
					var newLocalPath = System.IO.Path.Combine (documentsPath, newPath);
					File.Move (LocalPath, newLocalPath);
					Path = newPath;
					LocalPath = newLocalPath;
					return true;
				} catch (Exception) {
					return false;
				}
			});
		}

		public DeviceFile (string path, string documentsPath)
		{
			this.documentsPath = documentsPath;
			Path = path;
			LocalPath = System.IO.Path.Combine (documentsPath, path);
			IsDirectory = Directory.Exists (LocalPath);

			try {
				var fileInfo = new FileInfo (LocalPath);
				ModifiedTime = fileInfo.LastWriteTimeUtc;
			} catch (Exception) {
				ModifiedTime = DateTime.UtcNow;
			}
		}

		public DeviceFile (string localPath)
			: this (System.IO.Path.GetFileName (localPath), System.IO.Path.GetDirectoryName (localPath))
		{
		}
	}
}

