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

#pragma warning disable 67
		public event EventHandler FilesChanged;
#pragma warning restore 67

		public Task Initialize ()
		{
			return Task.FromResult<object> (null);
		}

		public string Id {
			get {
				return "Empty";
			}
		}
		public string IconUrl => null;
		public string ShortDescription { get { return Description; } }
		
		public bool CanRemoveFileSystem => false;
		public void RemoveFileSystem () { }

		public int MaxDirectoryDepth { get { return 0; } }

		public string Description { get; set; }

		public bool IsWritable { get { return false; } }

		public bool JustForApp { get { return true; } }

		public bool IsAvailable { get { return true; } }
		public string AvailabilityReason { get { return ""; } }

		public virtual bool IsSyncing => false;
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
            throw new Exception ("Empty File System cannot hold files");
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
		public bool ListFilesIsFast { get { return true; } }
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

	public class LoadingFileSystem : EmptyFileSystem
	{
		public override bool IsSyncing => true;

		public LoadingFileSystem ()
		{
			Description = "Loading Storage...";
		}
	}
}

