using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;
using System;

namespace Praeclarum.IO
{
	public class FileSystemManager
	{
		public ObservableCollection<IFileSystemProvider> Providers { get; private set; }

		public ObservableCollection<IFileSystem> FileSystems { get; private set; }

		public IFileSystem ActiveFileSystem {
			get;
			set;
		}

		public static FileSystemManager Shared { get; set; }

		public FileSystemManager ()
		{
			Providers = new ObservableCollection<IFileSystemProvider> ();
			FileSystems = new ObservableCollection<IFileSystem> ();
		}

		public void Add (IFileSystem fs)
		{
			FileSystems.Add (fs);
		}

		public void Add (IFileSystemProvider fss)
		{
			Providers.Add (fss);
			foreach (var fs in fss.GetFileSystems ())
				FileSystems.Add (fs);
		}

		public IFileSystem ChooseFileSystem (string lastFileSystemId)
		{
			var fs = FileSystems.FirstOrDefault (x => x.IsAvailable && x.Id == lastFileSystemId);
			#if __IOS__
			if (fs == null)
				fs = FileSystems.OfType<DeviceFileSystem> ().First (x => x.IsAvailable);
			#endif
			if (fs == null)
				fs = FileSystems.First (x => x.IsAvailable);

			return fs;
		}

		public static void EnsureDirectoryExists (string dir)
		{
			if (string.IsNullOrEmpty (dir) || dir == "/")
				return;

			if (!System.IO.Directory.Exists (dir)) {
				EnsureDirectoryExists (System.IO.Path.GetDirectoryName (dir));
				System.IO.Directory.CreateDirectory (dir);
			}
		}
	}
}

