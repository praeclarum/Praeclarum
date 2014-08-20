using System;
using Praeclarum.IO;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace Praeclarum.UI
{
	public class MoveDocumentsForm : PForm
	{
		public IFileSystem FileSystem { get; private set; }
		public string Directory { get; private set; }

		public MoveDocumentsForm ()
		{
			Title = "Move to";

			var fileSystems = FileSystemManager.Shared.FileSystems;

			foreach (var fs in fileSystems) {
				Sections.Add (new FileSystemSection (fs));
			}
		}

		void SelectItem (IFileSystem fs, string dir)
		{
			FileSystem = fs;
			Directory = dir;
			DismissAsync ();
		}

		class FileSystemSection : PFormSection
		{
			readonly IFileSystem fileSystem;

			public FileSystemSection (IFileSystem fileSystem)
			{
				this.fileSystem = fileSystem;

				AddFileSystemsAsync ().ContinueWith (task => {
					if (!task.IsFaulted)
						SetNeedsReload ();
				}, TaskScheduler.FromCurrentSynchronizationContext ());
			}

			async Task AddFileSystemsAsync ()
			{
				await fileSystem.Sync (TimeSpan.FromSeconds (5));
				await AddDirAsync ("");
			}

			async Task AddDirAsync (string dir)
			{
				try {
					Items.Add (dir);

					var files = await fileSystem.ListFiles (dir);
					foreach (var f in files.OrderBy (x => x.Path)) {
						if (f.IsDirectory) {
							await AddDirAsync (f.Path);
						}
					}

				} catch (Exception ex) {
					Console.WriteLine (ex);
				}
			}

			public override string GetItemTitle (object item)
			{
				var path = (string)item;
				if (!path.StartsWith ("/", StringComparison.Ordinal))
					path = "/" + path;
				var d = path.Count (x => x == '/');
				string title;
				if (path == "/") {
					title = fileSystem.Description;
				} else {
					title = new string (' ', 6 * d) + Path.GetFileName (path);
				}
				return title;
			}

			public override bool GetItemEnabled (object item)
			{
				return fileSystem.IsWritable;
			}

			public override bool SelectItem (object item)
			{
				var f = Form as MoveDocumentsForm;
				if (f == null)
					return false;

				f.SelectItem (fileSystem, (string)item);

				return true;
			}

			public override bool GetItemChecked (object item)
			{
				var path = (string)item;
				return fileSystem == FileSystemManager.Shared.ActiveFileSystem && (path == DocumentAppDelegate.Shared.CurrentDirectory);
			}

			public override string GetItemImage (object item)
			{
				return fileSystem.GetType ().Name;
			}
		}
	}
}

