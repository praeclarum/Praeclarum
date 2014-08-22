using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

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
		Task<IFile> CreateFile (string path, byte[] contents);

		Task<bool> CreateDirectory (string path);
		Task<bool> FileExists (string path);
		Task<bool> DeleteFile (string path);
		Task<bool> Move (string fromPath, string toPath);
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
		public static Task<IFile> CreateFile (this IFileSystem fs, string path, string contents)
		{
			var bytes = System.Text.Encoding.UTF8.GetBytes (contents);
			return fs.CreateFile (path, bytes);
		}

		public static async Task<byte[]> ReadAllBytesAsync (this IFile file)
		{
			var a = await file.BeginLocalAccess ();
			Exception err = null;
			var r = new byte[0];
			try {
				r = File.ReadAllBytes (a.LocalPath);
			}
			catch (Exception ex) {
				err = ex;
			}
			await a.End ();
			if (err != null)
				throw new AggregateException (err);
			return r;
		}

		public static Task<IFile> DuplicateAsync (this IFileSystem fs, IFile file)
		{
			return fs.CopyAsync (file, fs, Path.GetDirectoryName (file.Path));
		}

		public static Task<IFile> CopyAsync (this IFileSystem src, IFile file, IFileSystem dest, string destDir)
		{
			if (file.IsDirectory)
				return CopyDirectoryAsync (src, file, dest, destDir);
			return CopyFileAsync (src, file, dest, destDir);
		}

		static async Task<IFile> CopyFileAsync (this IFileSystem src, IFile file, IFileSystem dest, string destDir)
		{
			var contents = await file.ReadAllBytesAsync ();
			var newPath = await dest.GetAvailableNameAsync (Path.Combine (destDir, Path.GetFileName (file.Path)));
			return await dest.CreateFile (newPath, contents);
		}

		static async Task<IFile> CopyDirectoryAsync (this IFileSystem src, IFile file, IFileSystem dest, string destDir)
		{
			var newPath = await dest.GetAvailableNameAsync (Path.Combine (destDir, Path.GetFileName (file.Path)));

			var r = await dest.CreateDirectory (newPath);
			if (!r)
				throw new Exception ("Failed to create destination directory " + newPath + " on " + dest);

			var srcFiles = await src.ListFiles (file.Path);
			foreach (var f in srcFiles) {
				await src.CopyAsync (f, dest, newPath);
			}

			return await dest.GetFile (newPath);
		}

		public static Task MoveAsync (this IFileSystem src, IFile file, IFileSystem dest, string destDir)
		{
			if (file.IsDirectory)
				return MoveDirectoryAsync (src, file, dest, destDir);
			return MoveFileAsync (src, file, dest, destDir);
		}

		static async Task MoveFileAsync (this IFileSystem src, IFile file, IFileSystem dest, string destDir)
		{
			await src.CopyFileAsync (file, dest, destDir);
			if (!await src.DeleteFile (file.Path)) {
				throw new Exception ("Failed to delete " + file.Path + " from " + src);
			}
		}

		static async Task MoveDirectoryAsync (this IFileSystem src, IFile file, IFileSystem dest, string destDir)
		{
			await src.CopyDirectoryAsync (file, dest, destDir);
			if (!await src.DeleteFile (file.Path)) {
				throw new Exception ("Failed to delete directory " + file.Path + " from " + src);
			}
		}

		public static async Task<string> GetAvailableNameAsync (this IFileSystem fs, string path)
		{
			if (!await fs.FileExists (path))
				return path;

			var dir = Path.GetDirectoryName (path);
			var name = Path.GetFileNameWithoutExtension (path);
			var ext = Path.GetExtension (path);

			var postfix = " Copy";

			Func<string> makeName = () => Path.Combine (dir, name + postfix + ext);

			var newPath = makeName ();

			while (await fs.FileExists (newPath)) {
				postfix += " Copy";
				newPath = makeName ();
			}

			return newPath;
		}

		public static async Task<bool> Sync (this IFileSystem fs, TimeSpan timeout)
		{
			if (!fs.IsAvailable)
				return false;

//			Console.WriteLine ("SYNCCCC");

			var LoopTime = TimeSpan.FromSeconds (0.5);
			var MaxLoops = (int)(timeout.TotalSeconds / LoopTime.TotalSeconds);

			for (int i = 0; i < MaxLoops; i++) {

//				Console.WriteLine ("SYNC STATUS " + fs.SyncStatus);

				if (!fs.IsSyncing) {
					return true;
				}

				await Task.Delay (LoopTime);
			}

			return false;
		}

		public static async Task<string> GetUniquePath (this IFileSystem fs, string basePath)
		{
			var folder = System.IO.Path.GetDirectoryName (basePath);
			var baseName = System.IO.Path.GetFileNameWithoutExtension (basePath);
			var extension = System.IO.Path.GetExtension (basePath);

			var path = basePath;

			if (await fs.FileExists (path)) {
				var uniqueId = 2;
				path = System.IO.Path.Combine (folder, baseName + " " + uniqueId + extension);
				while (await fs.FileExists (path)) {
					uniqueId++;
					path = System.IO.Path.Combine (folder, baseName + " " + uniqueId + extension);
				}
			}

			return path;
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

