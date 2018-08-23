using System;
using System.Collections.Generic;
using System.Linq;
using Praeclarum.UI;
using Praeclarum.IO;
using UIKit;
using Foundation;
using System.Collections.Specialized;
using Praeclarum;
using System.Threading.Tasks;

namespace Praeclarum.UI
{
	public class StorageForm : PForm
	{
		public StorageForm ()
		{
			Title = "Choose Storage";

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
				var f = item as IFileSystemProvider;
				if (f != null) {
					var tname = f.GetType ().Name;
					return tname.Replace ("Provider", "");
				}

				return base.GetItemTitle (item);
			}

			public override string GetItemTitle (object item)
			{
				var f = item as IFileSystemProvider;
				if (f != null)
					return f.Name;

				return base.GetItemTitle (item);
			}

			public override bool SelectItem (object item)
			{
				var f = item as IFileSystemProvider;
				if (f != null) {

					AddFileSystemAsync (f).ContinueWith (t => {
						if (t.IsFaulted)
							Log.Error (t.Exception);
					});

					return false;
				}

				return false;
			}

			async Task AddFileSystemAsync (IFileSystemProvider f)
			{
				await f.ShowAddUI (Form);
				await Form.DismissAsync (true);
			}
		}

		class FileSystemsSection : PFormSection
		{
			object addStorage = "Add Storage";

			NotifyCollectionChangedEventHandler h;
			NSTimer timer;

			public FileSystemsSection ()
			{
				Refresh ();

				h = HandleFileSystemsChanged;

				FileSystemManager.Shared.FileSystems.CollectionChanged += h;

				timer = NSTimer.CreateRepeatingScheduledTimer (1, FormatTick);
			}

			public override void Dismiss ()
			{
				base.Dismiss ();

				if (timer != null) {
					timer.Invalidate ();
					timer = null;
				}

				if (h != null) {
					FileSystemManager.Shared.FileSystems.CollectionChanged -= h;
					h = null;
				}
			}

			void FormatTick (NSTimer obj)
			{
				SetNeedsFormat ();
			}

			void HandleFileSystemsChanged (object sender, NotifyCollectionChangedEventArgs e)
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
					Items.Add (addStorage);
			}

			public override bool GetItemEnabled (object item)
			{
				var f = item as IFileSystem;
				if (f != null)
					return f.IsAvailable;

				return base.GetItemEnabled (item);
			}

			public override bool GetItemChecked (object item)
			{
				return item == FileSystemManager.Shared.ActiveFileSystem;
			}

			public override string GetItemTitle (object item)
			{
				var f = item as IFileSystem;
				if (f != null) {
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
				var f = item as IFileSystem;
				if (f != null)
					return f.GetType ().Name;

				return null;
			}

			public override bool GetItemNavigates (object item)
			{
				return item == addStorage;
			}

			public override bool SelectItem (object item)
			{
				if (item == addStorage) {

					var chooseProviderForm = new PForm ("Add Storage");
					chooseProviderForm.Sections.Add (new FileSystemProvidersSection ());

					Form.NavigationController.PushViewController (chooseProviderForm, true);

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

			static IEnumerable<object> GetAddOptions ()
			{
				var fman = FileSystemManager.Shared;
				var fs = 
					from p in fman.Providers
					where p.CanAddFileSystem
					select (object)p;
				return fs;
			}
		}
	}
}
