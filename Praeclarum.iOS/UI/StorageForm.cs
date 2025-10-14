#nullable enable

using System;
using System.Linq;
using Praeclarum.IO;
using Foundation;
using System.Collections.Specialized;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Praeclarum.UI
{
	public class StorageForm : PForm
	{
		public StorageForm ()
		{
			base.Title = "Choose Storage";

			Sections.Add (new FileSystemsSection ());
		}

		class FileSystemProvidersSection : PFormSection
		{
			public FileSystemProvidersSection ()
			{
				Title = "Choose a Provider";

				Refresh ();
			}

			void Refresh ()
			{
				Items.Clear ();

				foreach (var f in FileSystemManager.Shared.Providers.Where (x => x.CanAddFileSystem)) {
					Items.Add (f);
				}
			}

			public override string GetItemImage (object item)
			{
				if (item is IFileSystemProvider f) {
					var iconImage = f.IconUrl;
					if (iconImage is not null)
						return iconImage;
					var typeName = f.GetType ().Name;
					return typeName.Replace ("Provider", "");
				}

				return base.GetItemImage (item);
			}

			public override string GetItemTitle (object item)
			{
				return item is IFileSystemProvider f ? f.Name : base.GetItemTitle (item);
			}

			public override bool SelectItem (object item)
			{
				if (item is not IFileSystemProvider f)
				{
					return false;
				}

				AddFileSystemAsync (f).ContinueWith (t => {
					if (t.IsFaulted)
						Log.Error (t.Exception);
				});

				return false;
			}

			async Task AddFileSystemAsync (IFileSystemProvider fileSystemProvider)
			{
				if (Form is not {} form)
					return;
				if (await fileSystemProvider.ShowAddUI (form) is {} newFileSystem)
				{
					FileSystemManager.Shared.Add (newFileSystem);
				}
				await form.DismissAsync (true);
			}
		}

		class FileSystemsSection : PFormSection
		{
			readonly object _addStorage = "Add Storage";

			NotifyCollectionChangedEventHandler? _h;
			NSTimer? _timer;

			private bool ignoreChanges;

			public FileSystemsSection ()
			{
				Refresh ();

				_h = HandleFileSystemsChanged;

				FileSystemManager.Shared.FileSystems.CollectionChanged += _h;

				_timer = NSTimer.CreateRepeatingScheduledTimer (1, FormatTick);
			}

			public override void Dismiss ()
			{
				base.Dismiss ();

				if (_timer != null) {
					_timer.Invalidate ();
					_timer = null;
				}

				if (_h != null) {
					FileSystemManager.Shared.FileSystems.CollectionChanged -= _h;
					_h = null;
				}
			}

			void FormatTick (NSTimer obj)
			{
				SetNeedsFormat ();
			}

			void HandleFileSystemsChanged (object? sender, NotifyCollectionChangedEventArgs e)
			{
				if (ignoreChanges)
					return;
				Refresh ();
				SetNeedsReload ();
			}

			void Refresh ()
			{
				Items.Clear ();

				var fileSystemManager = FileSystemManager.Shared;

				foreach (var f in fileSystemManager.FileSystems) {
					Items.Add (f);
				}

				if (fileSystemManager.Providers.Any (x => x.CanAddFileSystem))
					Items.Add (_addStorage);
			}

			public override bool GetItemEnabled (object item)
			{
				if (item is IFileSystem f)
					return f.IsAvailable;

				return base.GetItemEnabled (item);
			}

			public override bool GetItemChecked (object item)
			{
				return item == FileSystemManager.Shared.ActiveFileSystem;
			}

			public override string GetItemTitle (object item)
			{
				if (item is IFileSystem f) {
					var desc = f.Description;
					if (!f.IsAvailable) {
						desc += " (" + f.AvailabilityReason + ")";
					} else if (f.IsSyncing) {
						desc += " (" + f.SyncStatus + ")";
					}
					return desc;
				}

				return base.GetItemTitle (item);
			}

			public override string GetItemImage (object item)
			{
				if (item is IFileSystem f)
				{
					if (f.IconUrl is { } url)
						return url;
					return f.GetType ().Name;
				}
				return base.GetItemImage (item);
			}

			public override bool GetItemNavigates (object item)
			{
				return item == _addStorage;
			}

			public override bool SelectItem (object item)
			{
				if (item == _addStorage) {

					var chooseProviderForm = new PForm ("Add Storage");
					chooseProviderForm.Sections.Add (new FileSystemProvidersSection ());

					Form?.NavigationController?.PushViewController (chooseProviderForm, true);

					return true;
				}

				var fs = item as IFileSystem;
				SetNeedsFormat ();

				DocumentAppDelegate.Shared.SetFileSystemAsync (fs, true).ContinueWith (Log.TaskError);

				return true;
			}

			public override EditAction GetItemEditActions (object item)
			{
				return item is IFileSystem { CanRemoveFileSystem: true } ? EditAction.Delete : EditAction.None;
			}

			public override void DeleteItem (object item)
			{
				base.DeleteItem (item);
				if (item is not IFileSystem fs)
					return;
				var fileSystemManager = FileSystemManager.Shared;
				try
				{
					ignoreChanges = true;
					fileSystemManager.FileSystems.Remove (fs);
				}
				finally
				{
					ignoreChanges = false;
				}
				fs.RemoveFileSystem ();
				// If we deleted the active file system, switch to another available one
				if (ReferenceEquals(fs, fileSystemManager.ActiveFileSystem)
				    && fileSystemManager.FileSystems.FirstOrDefault (x => x.IsAvailable) is {} newFileSystem)
				{
					DocumentAppDelegate.Shared.SetFileSystemAsync (newFileSystem, false).ContinueWith (t =>
					{
						if (t.IsFaulted)
							Log.Error (t.Exception);
					});
				}
			}
		}
	}
}
