using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Praeclarum.IO
{
	public interface IFileSystemProvider
	{
		string Name { get;  }

		bool CanAddFileSystem {
			get;
		}

		Task ShowAddUI (object parent);

		IEnumerable<IFileSystem> GetFileSystems ();
	}

	public interface IFileSystem
	{
		string Id { get; }

		Task Initialize ();

		string Description { get; }
		string ShortDescription { get; }

		bool IsAvailable { get; }
		string AvailabilityReason { get; }

		bool IsSyncing { get; }
		string SyncStatus { get; }

		bool IsWritable { get; }

		int MaxDirectoryDepth { get; }

		ICollection<string> FileExtensions { get; }

		event EventHandler FilesChanged;
		Task<List<IFile>> ListFiles (string directory);

		Task<IFile> GetFile (string path);

		/// <summary>
		/// Overwrites
		/// </summary>
		Task<IFile> CreateFile (string path, string contents);

		Task<bool> CreateDirectory (string path);
		Task<bool> FileExists (string path);
		Task<bool> DeleteFile (string path);
		Task<bool> Move (string fromPath, string toPath);

		string GetLocalPath (string path);
	}

	public interface IFile
	{
		string Path { get; }
		bool IsDirectory { get; }
		DateTime ModifiedTime { get; }

		Task<LocalFileAccess> BeginLocalAccess ();

		bool IsDownloaded { get; }
		double DownloadProgress { get; }

		Task<bool> Move (string newPath);
	}

	public class LocalFileAccess
	{
		public string LocalPath { get; private set; }

		public LocalFileAccess (string path)
		{
			LocalPath = path;
		}

		public virtual Task End ()
		{
			return Task.FromResult<object> (null);
		}
	}


	public static class IFileSystemEx
	{
		public static async Task Sync (this IFileSystem fs, TimeSpan timeout)
		{
//			Console.WriteLine ("SYNCCCC");

			var LoopTime = TimeSpan.FromSeconds (1);
			var MaxLoops = (int)(timeout.TotalSeconds / LoopTime.TotalSeconds);

			for (int i = 0; i < MaxLoops; i++) {

//				Console.WriteLine ("SYNC STATUS " + fs.SyncStatus);

				if (!fs.IsSyncing) {
					return;
				}

				await Task.Delay (LoopTime);
			}
		}
	}

	public static class IFileEx
	{
		public static async Task<bool> Copy (this IFile src, IFileSystem destFileSystem, string destPath)
		{
#if PORTABLE
			return false;
#else
			IFile dest = null;
			var r = false;
			LocalFileAccess srcLocal = null;
			LocalFileAccess destLocal = null;

			try {

				dest = await destFileSystem.CreateFile (destPath, "");

				srcLocal = await src.BeginLocalAccess ();
				destLocal = await dest.BeginLocalAccess ();

				var srcLocalPath = srcLocal.LocalPath;
				var destLocalPath = destLocal.LocalPath;

				System.IO.File.Copy (srcLocalPath, destLocalPath, overwrite: true);

				r = true;

								
			} catch (Exception ex) {
				Debug.WriteLine (ex);
				r = false;
			}

			if (srcLocal != null) await srcLocal.End ();
			if (destLocal != null) await destLocal.End ();

			return r;

//			await Task.Factory.StartNew (() => {
//
//				var fc = new NSFileCoordinator (filePresenterOrNil: null);
//				NSError coordErr;
//
//				fc.CoordinateReadWrite (
//					srcPath, NSFileCoordinatorReadingOptions.WithoutChanges,
//					destPath, NSFileCoordinatorWritingOptions.ForReplacing,
//					out coordErr, (readUrl, writeUrl) => {
//
//					var r = false;
//					try {
//						File.Copy (readUrl.Path, writeUrl.Path, overwrite: true);
//						r = true;
//					} catch (Exception) {
//						r = false;
//					}
//					tcs.SetResult (r);
//				});
//			});
#endif
		}
	}
}

