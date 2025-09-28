#nullable enable

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

				var fman = FileSystemManager.Shared;

				foreach (var f in fman.Providers.Where (x => x.CanAddFileSystem)) {
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
				await fileSystemProvider.ShowAddUI (form);
				await form.DismissAsync (true);
			}
		}

		class FileSystemsSection : PFormSection
		{
			readonly object _addStorage = "Add Storage";

			NotifyCollectionChangedEventHandler? _h;
			NSTimer? _timer;

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
				Refresh ();
				SetNeedsReload ();
			}

			void Refresh ()
			{
				Items.Clear ();

				var fman = FileSystemManager.Shared;

				foreach (var f in fman.FileSystems) {
					Items.Add (f);
				}

				if (fman.Providers.Any (x => x.CanAddFileSystem))
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

				Form?.DismissAsync (true).ContinueWith (t => {
					if (t.IsFaulted)
						Log.Error (t.Exception);
				});
				DocumentAppDelegate.Shared.SetFileSystemAsync (fs, true).ContinueWith (t => {
					if (t.IsFaulted)
						Log.Error (t.Exception);
				});

				return true;
			}
		}
	}
}
