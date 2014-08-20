using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Praeclarum.IO
{
	public class EmptyFileSystem : IFileSystem
	{
		public ICollection<string> FileExtensions { get; private set; }

		public EmptyFileSystem ()
		{
			FileExtensions = new Collection<string> ();
			Description = "Empty";
		}

		public event EventHandler FilesChanged;

		public Task Initialize ()
		{
			return Task.FromResult<object> (null);
		}

		public string Id {
			get {
				return "Empty";
			}
		}
		public string ShortDescription { get { return Description; } }

		public int MaxDirectoryDepth { get { return 0; } }

		public string Description { get; set; }

		public bool IsWritable { get { return false; } }

		public bool IsAvailable { get { return true; } }
		public string AvailabilityReason { get { return ""; } }

		public bool IsSyncing { get { return false; } }
		public string SyncStatus { get { return ""; } }

		public string GetLocalPath (string path)
		{
			return "";
		}

		public Task<IFile> GetFile (string path)
		{
			throw new Exception ("Empty File System contains no files");
		}

		public Task<IFile> CreateFile (string path, byte[] contents)
		{
			var tcs = new TaskCompletionSource<IFile> ();
			tcs.SetResult (new DeviceFile (path, path));
			return tcs.Task;
		}

		public Task<bool> CreateDirectory (string path)
		{
			return Task.FromResult (false);
		}

		public Task<bool> Move (string fromPath, string toPath)
		{
			return Task.FromResult (false);
		}

		public Task<bool> DeleteFile (string path)
		{
			var tcs = new TaskCompletionSource<bool> ();
			tcs.SetResult (false);
			return tcs.Task;
		}

		public Task<List<IFile>> ListFiles (string directory)
		{
			return Task.FromResult (files);
		}
		List<IFile> files = new List<IFile> ();

		public Task<bool> FileExists (string path)
		{
			return Task.FromResult (false);
		}
	}
}

